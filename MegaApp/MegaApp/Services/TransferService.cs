using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
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

        public static void UpdateMegaTransferList(TransferQueue megaTransfers, MTransferType type)
        {
            UiService.OnUiThread(() =>
            {
                switch (type)
                {
                    case MTransferType.TYPE_DOWNLOAD:
                        megaTransfers.Downloads.Clear();
                        break;
                    case MTransferType.TYPE_UPLOAD:
                        megaTransfers.Uploads.Clear();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            });

            var transfers = SdkService.MegaSdk.getTransfers(type);
            var numTransfers = transfers.size();
            for (int i = 0; i < numTransfers; i++)
                AddTransferToList(megaTransfers, transfers.get(i));
        }
        
        public static void AddTransferToList(TransferQueue megaTransfers, MTransfer transfer)
        {
            TransferObjectModel megaTransfer;
            if (transfer.getType() == MTransferType.TYPE_DOWNLOAD)
            {
                
                MNode node = transfer.getPublicMegaNode() ?? // If is a public node
                             SdkService.MegaSdk.getNodeByHandle(transfer.getNodeHandle()); // If not

                if (node == null) return;

                megaTransfer = new TransferObjectModel(
                    NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, node, null),
                    TransferType.Download, transfer.getPath());
            }
            else
            {
                var parentNode = SdkService.MegaSdk.getNodeByPath(transfer.getParentPath());
                megaTransfer = new TransferObjectModel(
                    NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, parentNode, null),
                    TransferType.Upload, transfer.getPath());
            }

            UiService.OnUiThread(() =>
            {
                GetTransferAppData(transfer, megaTransfer);

                megaTransfer.Transfer = transfer;
                megaTransfer.Status = TransferStatus.Queued;
                megaTransfer.IsBusy = true;
                megaTransfer.TotalBytes = transfer.getTotalBytes();
                megaTransfer.TransferedBytes = transfer.getTransferredBytes();

                megaTransfer.TransferSpeed = !SdkService.MegaSdk.areTransfersPaused((int)transfer.getType()) 
                    ? transfer.getSpeed().ToStringAndSuffixPerSecond() 
                    : string.Empty;

                megaTransfers.Add(megaTransfer);
            });
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
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_DownloadFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_SelectFolderFailedNoErrorCode"));
                return false;
            }

            // Check for illegal characters in the download path
            if (FolderService.HasIllegalChars(downloadPath))
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_DownloadFailed_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_InvalidFolderNameOrPath"), downloadPath));
                return false;
            }

            bool pathExists = true; //Suppose that the download path exists
            try { await StorageFolder.GetFolderFromPathAsync(downloadPath); }
            catch (FileNotFoundException) { pathExists = false; }
            catch (UnauthorizedAccessException)
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_DowloadPathUnauthorizedAccess_Title"),
                    ResourceService.AppMessages.GetString("AM_DowloadPathUnauthorizedAccess"));
                return false;
            }
            catch (Exception e)
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_DownloadFailed_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_DownloadPathUnknownError"),
                    e.GetType().Name + " - " + e.Message));
                return false;
            }

            // Create the download path if not exists
            if (!pathExists)
                return await CreateExternalDownloadPathAsync(downloadPath);

            return true;
        }

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
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_DownloadFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_CreateDownloadPathError"),
                        e.GetType().Name + " - " + e.Message));
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Cancel all the pending offline transfer of a node and wait until all transfers are canceled.
        /// </summary>
        /// <param name="nodePath">Path of the node.</param>
        /// <param name="isFolder">Boolean value which indicates if the node is a folder or not.</param>
        //public static void CancelPendingNodeOfflineTransfers(string nodePath, bool isFolder)
        //{
        //    var megaTransfers = SdkService.MegaSdk.getTransfers(MTransferType.TYPE_DOWNLOAD);
        //    var numMegaTransfers = megaTransfers.size();

        //    for (int i = 0; i < numMegaTransfers; i++)
        //    {
        //        var transfer = megaTransfers.get(i);
        //        if (transfer == null) continue;

        //        string transferPathToCompare;
        //        if (isFolder)
        //            transferPathToCompare = transfer.getParentPath();
        //        else
        //            transferPathToCompare = transfer.getPath();

        //        WaitHandle waitEventRequestTransfer = new AutoResetEvent(false);
        //        if (string.Compare(nodePath, transferPathToCompare) == 0)
        //        {
        //            SdkService.MegaSdk.cancelTransfer(transfer, 
        //                new CancelTransferRequestListener((AutoResetEvent)waitEventRequestTransfer));
        //            waitEventRequestTransfer.WaitOne();
        //        }
        //    }
        //}
    }
}
