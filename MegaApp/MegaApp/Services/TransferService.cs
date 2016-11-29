using System;
using System.Linq;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.ViewModels;
using MegaApp.Enums;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using MegaApp.Extensions;

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

        /// <summary>
        /// Update the transfers list/queue.
        /// </summary>
        /// <param name="MegaTransfers">Transfers list/queue to update.</param>
        public static void UpdateMegaTransfersList(TransferQueue MegaTransfers)
        {
            UiService.OnUiThread(() =>
            {
                MegaTransfers.Clear();
                MegaTransfers.Downloads.Clear();
                MegaTransfers.Uploads.Clear();
            });

            // Get transfers and fill the transfers list again.
            var transfers = SdkService.MegaSdk.getTransfers();
            var numTransfers = transfers.size();
            for (int i = 0; i < numTransfers; i++)
                AddTransferToList(transfers.get(i));
        }

        public static void AddTransferToList(MTransfer transfer)
        {
            TransferObjectModel megaTransfer = null;
            if (transfer.getType() == MTransferType.TYPE_DOWNLOAD)
            {
                // If is a public node
                MNode node = transfer.getPublicMegaNode();
                if (node == null) // If not
                    node = SdkService.MegaSdk.getNodeByHandle(transfer.getNodeHandle());

                if (node == null) return;

                megaTransfer = new TransferObjectModel(
                    NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, node, ContainerType.CloudDrive),
                    TransferType.Download, transfer.getPath());
            }
            else
            {
                var parentNode = SdkService.MegaSdk.getNodeByPath(transfer.getParentPath());
                megaTransfer = new TransferObjectModel(
                    NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, parentNode, ContainerType.CloudDrive),
                    TransferType.Upload, transfer.getPath());
            }

            if (megaTransfer != null)
            {
                UiService.OnUiThread(() =>
                {
                    GetTransferAppData(transfer, megaTransfer);

                    megaTransfer.Transfer = transfer;
                    megaTransfer.Status = TransferStatus.Queued;
                    megaTransfer.CancelButtonState = true;
                    megaTransfer.TransferButtonIcon = new Uri("/Assets/Images/cancel transfers.Screen-WXGA.png", UriKind.Relative);
                    megaTransfer.TransferButtonForegroundColor = (SolidColorBrush)Application.Current.Resources["MegaAppForegroundBrush"];
                    megaTransfer.IsBusy = true;
                    megaTransfer.TotalBytes = transfer.getTotalBytes();
                    megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                    megaTransfer.TransferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();

                    MegaTransfers.Add(megaTransfer);
                });
            }
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
