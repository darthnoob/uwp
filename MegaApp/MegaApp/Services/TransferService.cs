using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.ViewModels;

namespace MegaApp.Services
{
    static class TransferService
    {
        /// <summary>
        /// Global transfers queue
        /// </summary>
        private static TransferQueue _megaTransfers;
        public static TransferQueue MegaTransfers
        {
            get
            {
                if (_megaTransfers != null) return _megaTransfers;
                _megaTransfers = new TransferQueue();
                return _megaTransfers;
            }
        }

        /// <summary>
        /// Global transfer listener
        /// </summary>
        private static GlobalTransferListener _globalTransferListener;
        public static GlobalTransferListener GlobalTransferListener
        {
            get
            {
                if (_globalTransferListener != null) return _globalTransferListener;
                _globalTransferListener = new GlobalTransferListener();
                return _globalTransferListener;
            }
        }

        #region Public Methods

        /// <summary>
        /// Moves a transfer to the completed transfers list.
        /// </summary>
        /// <param name="megaTransfers"><see cref="TransferQueue"/> which contains the transfers list(s).</param>
        /// <param name="megaTransfer"><see cref="TransferObjectModel"/> of the transfer to move.</param>
        public static void MoveMegaTransferToCompleted(TransferQueue megaTransfers, TransferObjectModel megaTransfer)
        {
            switch (megaTransfer.Type)
            {
                case MTransferType.TYPE_DOWNLOAD:
                    megaTransfers.Downloads.Remove(megaTransfer);
                    break;
                case MTransferType.TYPE_UPLOAD:
                    megaTransfers.Uploads.Remove(megaTransfer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(megaTransfer.Type), megaTransfer.Type, null);
            }

            megaTransfers.Completed.Add(megaTransfer);
        }

        /// <summary>
        /// Update a transfers list.
        /// </summary>
        /// <param name="megaTransfers"><see cref="TransferQueue"/> which contains the transfers list(s).</param>
        /// <param name="type">Type of the transfers list to update (Download or Upload).</param>
        /// <param name="cleanTransfers">Boolean value which indicates if clean the transfers list before update or not.</param>
        public static void UpdateMegaTransferList(TransferQueue megaTransfers, MTransferType type, bool cleanTransfers = false)
        {
            // List to store the uploads that are pending on preparation (copy file to the temporary upload folder), 
            // because they have not already added to the SDK queue
            List<TransferObjectModel> uploadsInPreparation = new List<TransferObjectModel>();

            if (cleanTransfers)
            {
                switch (type)
                {
                    case MTransferType.TYPE_DOWNLOAD:                        
                        megaTransfers.Downloads.Clear();
                        break;

                    case MTransferType.TYPE_UPLOAD:
                        // Store the uploads pending on preparation and clear the list
                        uploadsInPreparation = megaTransfers.Uploads.Where(t => t.PreparingUploadCancelToken != null).ToList();
                        megaTransfers.Uploads.Clear();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            var transfers = SdkService.MegaSdk.getTransfers(type);
            var numTransfers = transfers.size();
            for (int i = 0; i < numTransfers; i++)
                AddTransferToList(megaTransfers, transfers.get(i));

            // Restore the uploads in preparation
            if (type == MTransferType.TYPE_UPLOAD)
            {
                foreach (var upload in uploadsInPreparation)
                    megaTransfers.Add(upload);
            }
        }

        /// <summary>
        /// Add a <see cref="MTransfer"/> to the corresponding transfers list if it is not already included.
        /// </summary>
        /// <param name="megaTransfers"><see cref="TransferQueue"/> which contains the transfers list(s).</param>
        /// <param name="transfer"><see cref="MTransfer"/> to be added to the corresponding transfer list.</param>
        /// <returns>The <see cref="TransferObjectModel"/> corresponding to the <see cref="MTransfer"/>.</returns>
        public static TransferObjectModel AddTransferToList(TransferQueue megaTransfers, MTransfer transfer)
        {
            // Folder transfers are not included into the transfers list.
            if (transfer?.isFolderTransfer() == true) return null;

            // Search if the transfer already exists into the transfers list.
            var megaTransfer = SearchTransfer(megaTransfers.SelectAll(), transfer);
            if (megaTransfer != null) return megaTransfer;

            // If doesn't exist create a new one and add it to the transfers list
            megaTransfer = CreateTransferObjectModel(transfer);            
            if (megaTransfer != null)                
                megaTransfers.Add(megaTransfer);

            return megaTransfer;
        }

        /// <summary>
        /// Search into a transfers list the <see cref="TransferObjectModel"/> corresponding to a <see cref="MTransfer"/>.
        /// </summary>
        /// <param name="transfersList">Transfers list where search the transfer.</param>
        /// <param name="transfer">Transfer to search.</param>
        /// <returns>The transfer object if exists or NULL in other case.</returns>
        public static TransferObjectModel SearchTransfer(IList<TransferObjectModel> transfersList, MTransfer transfer)
        {
            // Folder transfers are not included into the transfers list.
            if (transfersList == null || transfer == null || transfer.isFolderTransfer()) return null;

            TransferObjectModel megaTransfer = null;
            try
            {
                megaTransfer = transfersList.FirstOrDefault(
                    t => (t.Transfer != null && t.Transfer.getTag() == transfer.getTag()) ||
                        (t.TransferPath != null && t.TransferPath.Equals(transfer.getPath())));
            }
            catch (Exception e)
            {
                var fileName = transfer.getFileName();
                var message = (fileName == null) ? "Error searching transfer" :
                    string.Format("Error searching transfer. File: '{0}'", fileName);
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, message, e);
                return null;
            }

            return megaTransfer;
        }

        /// <summary>
        /// Create a <see cref="TransferObjectModel"/> from a <see cref="MTransfer"/>.
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns>The new <see cref="TransferObjectModel"/></returns>
        public static TransferObjectModel CreateTransferObjectModel(MTransfer transfer)
        {
            if (transfer == null) return null;

            try
            {
                TransferObjectModel megaTransfer = null;

                switch (transfer.getType())
                {
                    case MTransferType.TYPE_DOWNLOAD:
                        MNode node = transfer.getPublicMegaNode() ?? // If is a public node
                            SdkService.MegaSdk.getNodeByHandle(transfer.getNodeHandle()); // If not

                        if (node == null) return null;

                        megaTransfer = new TransferObjectModel(
                            NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, node, null),
                            MTransferType.TYPE_DOWNLOAD, transfer.getPath());
                        break;

                    case MTransferType.TYPE_UPLOAD:
                        var parentNode = SdkService.MegaSdk.getNodeByHandle(transfer.getParentHandle());

                        if (parentNode == null) return null;

                        megaTransfer = new TransferObjectModel(
                            NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, parentNode, null),
                            MTransferType.TYPE_UPLOAD, transfer.getPath());
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (megaTransfer != null)
                {
                    GetTransferAppData(transfer, megaTransfer);

                    megaTransfer.Transfer = transfer;
                    megaTransfer.TransferState = transfer.getState();
                    megaTransfer.TransferPriority = transfer.getPriority();
                    megaTransfer.IsBusy = false;
                    megaTransfer.TotalBytes = transfer.getTotalBytes();
                    megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                    megaTransfer.TransferSpeed = string.Empty;
                    megaTransfer.TransferMeanSpeed = 0;

                    megaTransfer.TransferState = !SdkService.MegaSdk.areTransfersPaused((int)transfer.getType())
                        ? MTransferState.STATE_QUEUED : MTransferState.STATE_PAUSED;
                }

                return megaTransfer;
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Get the transfer "AppData" (substrings separated by '#')
        /// <para>- Substring 1: Boolean value to indicate if the download is for Save For Offline (SFO).</para>
        /// <para>- Substring 2: String which contains the download folder path external to the app sandbox cache.</para>
        /// </summary>
        /// <param name="transfer">MEGA SDK transfer to obtain the "AppData".</param>
        /// <param name="megaTransfer">App transfer object to be displayed.</param>
        /// <returns>Boolean value indicating if all was good.</returns>
        public static bool GetTransferAppData(MTransfer transfer, TransferObjectModel megaTransfer)
        {
            // Default values
            megaTransfer.IsSaveForOfflineTransfer = false;
            megaTransfer.ExternalDownloadPath = null;

            // Only the downloads can contain app data
            if (transfer.getType() != MTransferType.TYPE_DOWNLOAD)
                return false;

            // Get the transfer "AppData"
            string transferAppData = transfer.getAppData();
            if (string.IsNullOrWhiteSpace(transferAppData) || !transferAppData.Contains("#"))
                return false;

            // Split the string into the substrings separated by '#'
            string[] splittedAppData = transferAppData.Split("#".ToCharArray(), 2);
            if(splittedAppData.Count() < 1)
                return false;

            // Set the corresponding values
            megaTransfer.IsSaveForOfflineTransfer = Convert.ToBoolean(splittedAppData[0]);

            if(splittedAppData.Count() >= 2)
                megaTransfer.ExternalDownloadPath = splittedAppData[1];

            return true;
        }

        /// <summary>
        /// Create the transfer "AppData" string (substrings separated by '#')
        /// - Substring 1: Boolean value to indicate if the download is for Save For Offline (SFO).
        /// - Substring 2: String which contains the download folder path external to the app sandbox cache.
        /// </summary>
        /// <returns>"AppData" string (substrings separated by '#')</returns>
        public static string CreateTransferAppDataString(bool isSaveForOfflineTransfer = false,
            string downloadFolderPath = null)
        {
            return string.Concat(isSaveForOfflineTransfer.ToString(), "#", downloadFolderPath);
        }

        /// <summary>
        /// Checks if the destination download path external to the app exists 
        /// and has a right name, right permissions, etc.
        /// </summary>
        /// <param name="downloadPath">Download folder path.</param>
        /// <returns>TRUE if all is OK or FALSE in other case.</returns>
        public static async Task<bool> CheckExternalDownloadPathAsync(string downloadPath)
        {
            // Extra check to try avoid null values
            if (string.IsNullOrWhiteSpace(downloadPath))
            {
                UiService.OnUiThread(async() =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_DownloadFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_SelectFolderFailedNoErrorCode"));
                });
                return false;
            }

            // Check for illegal characters in the download path
            if (FolderService.HasIllegalChars(downloadPath))
            {
                UiService.OnUiThread(async() =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_DownloadFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_InvalidFolderNameOrPath"), downloadPath));
                });
                return false;
            }

            bool pathExists = true; //Suppose that the download path exists
            try { await StorageFolder.GetFolderFromPathAsync(downloadPath); }
            catch (FileNotFoundException) { pathExists = false; }
            catch (UnauthorizedAccessException)
            {
                UiService.OnUiThread(async() =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_DowloadPathUnauthorizedAccess_Title"),
                        ResourceService.AppMessages.GetString("AM_DowloadPathUnauthorizedAccess"));
                });
                return false;
            }
            catch (Exception e)
            {
                UiService.OnUiThread(async() =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_DownloadFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_DownloadPathUnknownError"),
                        e.GetType().Name + " - " + e.Message));
                });
                return false;
            }

            // Create the download path if not exists
            if (!pathExists)
                return await CreateExternalDownloadPathAsync(downloadPath);

            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a destination download path external to the app.
        /// </summary>
        /// <param name="downloadPath">The external download folder path.</param>
        /// <returns>TRUE if all went well or FALSE in other case.</returns>
        private static async Task<bool> CreateExternalDownloadPathAsync(string downloadPath)
        {
            string rootPath = Path.GetPathRoot(downloadPath);
            string tempDownloadPath = downloadPath;

            List<string> foldersNames = new List<string>(); //Folders that will be needed create
            List<string> foldersPaths = new List<string>(); //Paths where will needed create the folders

            //Loop to follow the reverse path to search the first missing folder
            while (string.Compare(tempDownloadPath, rootPath) != 0)
            {
                try { await StorageFolder.GetFolderFromPathAsync(tempDownloadPath); }
                catch (UnauthorizedAccessException)
                {
                    //The folder exists, but probably is a restricted access system folder in the download path. 
                    break; // Exit the loop.
                }
                catch (FileNotFoundException) //Folder not exists
                {
                    //Include the folder name that will be needed create and the corresponding path
                    foldersNames.Insert(0, Path.GetFileName(tempDownloadPath));
                    foldersPaths.Insert(0, new DirectoryInfo(tempDownloadPath).Parent.FullName);
                }
                finally
                {
                    //Upgrade to the next path to check (parent folder)
                    tempDownloadPath = new DirectoryInfo(tempDownloadPath).Parent.FullName;
                }
            }

            // Create each necessary folder of the download path
            for (int i = 0; i < foldersNames.Count; i++)
            {
                try
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(foldersPaths.ElementAt(i));
                    await folder.CreateFolderAsync(Path.GetFileName(foldersNames.ElementAt(i)), CreationCollisionOption.OpenIfExists);
                }
                catch (Exception e)
                {
                    UiService.OnUiThread(async() =>
                    {
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_DownloadFailed_Title"),
                            string.Format(ResourceService.AppMessages.GetString("AM_CreateDownloadPathError"),
                            e.GetType().Name + " - " + e.Message));
                    });
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
