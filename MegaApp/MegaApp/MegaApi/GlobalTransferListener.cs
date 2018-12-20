using System;
using System.IO;
using mega;
using MegaApp.Database;
using MegaApp.Extensions;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    public class GlobalTransferListener : MTransferListenerInterface
    {
        /// <summary>
        /// This function is called when a folder transfer has finished
        /// </summary>
        /// <param name="api">MegaApi object that started the transfer</param>
        /// <param name="transfer">Information about the transfer</param>
        /// <param name="e">Error information</param>
        private async void FolderTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            // In this case the transfer is not included in the transfers list.
            // We need to create a new 'TransferObjectModel' to work with it.
            var megaTransfer = TransferService.CreateTransferObjectModel(transfer);
            if (megaTransfer == null) return;

            megaTransfer.Transfer = transfer;
            UiService.OnUiThread(() =>
            {
                megaTransfer.TransferState = transfer.getState();
                megaTransfer.TransferPriority = transfer.getPriority();
            });

            switch (e.getErrorCode())
            {
                case MErrorType.API_OK:
                    if (transfer.getType() == MTransferType.TYPE_DOWNLOAD)
                    {
                        if (megaTransfer.IsSaveForOfflineTransfer)
                        {
                            this.AddOfflineNodeFromTransfer(megaTransfer);
                            return;
                        }

                        if (!await megaTransfer.FinishDownload(megaTransfer.TransferPath, megaTransfer.SelectedNode.Name))
                            UiService.OnUiThread(() => megaTransfer.TransferState = MTransferState.STATE_FAILED);
                    }
                    break;

                case MErrorType.API_EGOINGOVERQUOTA: // Not enough storage quota	
                case MErrorType.API_EOVERQUOTA: // Storage overquota error	
                    ProcessOverquotaError(api, e);
                    break;

                case MErrorType.API_EINCOMPLETE:
                    if (megaTransfer.IsSaveForOfflineTransfer)
                        this.RemoveOfflineNodeFromTransfer(megaTransfer);
                    break;

                default:
                    ProcessDefaultError(transfer);
                    break;
            }
        }

        /// <summary>
        /// This function is called when a file transfer has finished
        /// </summary>
        /// <param name="api">MegaApi object that started the transfer</param>
        /// <param name="transfer">Information about the transfer</param>
        /// <param name="e">Error information</param>
        private async void FileTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            // Search the corresponding transfer in the transfers list
            var megaTransfer = TransferService.SearchTransfer(TransferService.MegaTransfers.SelectAll(), transfer);
            if (megaTransfer == null) return;

            UiService.OnUiThread(() =>
            {
                megaTransfer.Transfer = transfer;
                megaTransfer.TransferState = transfer.getState();
                megaTransfer.TransferPriority = transfer.getPriority();

                TransferService.GetTransferAppData(transfer, megaTransfer);

                megaTransfer.TotalBytes = transfer.getTotalBytes();
                megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                megaTransfer.TransferSpeed = string.Empty;
                megaTransfer.IsBusy = false;
            });

            switch (e.getErrorCode())
            {
                case MErrorType.API_OK:
                    UiService.OnUiThread(() => megaTransfer.TransferedBytes = megaTransfer.TotalBytes);
                    if (megaTransfer.Type == MTransferType.TYPE_DOWNLOAD)
                    {
                        //If is download transfer of an image file 
                        if (megaTransfer.SelectedNode is ImageNodeViewModel)
                        {
                            var imageNode = megaTransfer.SelectedNode as ImageNodeViewModel;

                            UiService.OnUiThread(() => imageNode.ImageUri = new Uri(megaTransfer.TransferPath));

                            if (megaTransfer.AutoLoadImageOnFinish)
                            {
                                UiService.OnUiThread(() =>
                                {
                                    if (imageNode.OriginalMNode.hasPreview()) return;
                                    imageNode.PreviewImageUri = new Uri(imageNode.PreviewPath);
                                    imageNode.IsBusy = false;
                                });
                            }
                        }

                        if (megaTransfer.IsSaveForOfflineTransfer)
                        {
                            this.AddOfflineNodeFromTransfer(megaTransfer);
                        }
                        else if (!await megaTransfer.FinishDownload(megaTransfer.TransferPath, megaTransfer.SelectedNode.Name))
                        {
                            UiService.OnUiThread(() => megaTransfer.TransferState = MTransferState.STATE_FAILED);
                            return;
                        }
                    }

                    UiService.OnUiThread(() => TransferService.MoveMegaTransferToCompleted(TransferService.MegaTransfers, megaTransfer));
                    break;

                case MErrorType.API_EGOINGOVERQUOTA: // Not enough storage quota	
                case MErrorType.API_EOVERQUOTA: // Storage overquota error	
                    ProcessOverquotaError(api, e);
                    break;

                case MErrorType.API_EINCOMPLETE:
                    if (megaTransfer.IsSaveForOfflineTransfer)
                        this.RemoveOfflineNodeFromTransfer(megaTransfer);
                    break;

                default:
                    ProcessDefaultError(transfer);
                    break;
            }
        }

        private void AddOfflineNodeFromTransfer(TransferObjectModel megaTransfer)
        {
            var parentNode = SdkService.MegaSdk.getParentNode(megaTransfer.SelectedNode.OriginalMNode);

            // Need get the path on the transfer finish because the file name can be changed if already exists in the destiny path.
            var offlineNodePath = Path.Combine(
                OfflineService.GetOfflineParentNodePath(megaTransfer.SelectedNode.OriginalMNode),
                megaTransfer.Transfer.getFileName());

            var sfoNode = new SavedForOfflineDB
            {
                Fingerprint = SdkService.MegaSdk.getNodeFingerprint(megaTransfer.SelectedNode.OriginalMNode),
                Base64Handle = megaTransfer.SelectedNode.OriginalMNode.getBase64Handle(),
                LocalPath = offlineNodePath,
                ParentBase64Handle = parentNode != null ? 
                    parentNode.getBase64Handle() : string.Empty
            };

            if (SavedForOfflineDB.ExistsNodeByLocalPath(sfoNode.LocalPath))
                SavedForOfflineDB.UpdateNode(sfoNode);
            else
                SavedForOfflineDB.InsertNode(sfoNode);

            UiService.OnUiThread(() => megaTransfer.SelectedNode.IsSavedForOffline = true);

            OfflineService.CheckOfflineNodePath(megaTransfer.SelectedNode.OriginalMNode);
        }

        private void RemoveOfflineNodeFromTransfer(TransferObjectModel megaTransfer)
        {
            if (SavedForOfflineDB.ExistsNodeByLocalPath(megaTransfer.TransferPath))
                SavedForOfflineDB.DeleteNodeByLocalPath(megaTransfer.TransferPath);
        }

        /// <summary>
        /// This function is called when a transfer fails by an over quota error.
        /// It does the needed actions to process this kind of error.
        /// </summary>
        /// <param name="api">MegaApi object that started the transfer</param>
        /// <param name="e">Error information</param>
        private void ProcessOverquotaError(MegaSDK api, MError e)
        {
            // TRANSFER OVERQUOTA ERROR
            if (e.getValue() != 0)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                    string.Format("Transfer quota exceeded ({0})", e.getErrorCode().ToString()));
                AccountService.AccountDetails.IsInTransferOverquota = true;
                UiService.OnUiThread(DialogService.ShowTransferOverquotaWarning);

                return;
            }

            // STORAGE OVERQUOTA ERROR
            switch (e.getErrorCode())
            {
                case MErrorType.API_EGOINGOVERQUOTA: // Not enough storage quota
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                        string.Format("Not enough storage quota ({0})", e.getErrorCode().ToString()));
                    UiService.OnUiThread(() => DialogService.ShowStorageOverquotaAlert(true));
                    break;

                case MErrorType.API_EOVERQUOTA: // Storage overquota error
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                        string.Format("Storage quota exceeded ({0})", e.getErrorCode().ToString()));
                    UiService.OnUiThread(() =>
                    {
                        AccountService.AccountDetails.IsInStorageOverquota = true;
                        DialogService.ShowStorageOverquotaAlert(false);
                    });
                    break;
            }
        }

        /// <summary>
        /// This function is called when a transfer fails by a default error.
        /// It does the needed actions to process this kind of error.
        /// </summary>
        /// <param name="transfer"></param>
        private void ProcessDefaultError(MTransfer transfer)
        {
            string message, title = string.Empty;
            switch (transfer.getType())
            {
                case MTransferType.TYPE_DOWNLOAD:
                    title = ResourceService.AppMessages.GetString("AM_DownloadFailed_Title");
                    if (transfer.isFolderTransfer())
                        message = ResourceService.AppMessages.GetString("AM_DownloadFolderFailed");
                    else
                        message = ResourceService.AppMessages.GetString("AM_DownloadFileFailed");
                    break;

                case MTransferType.TYPE_UPLOAD:
                    title = ResourceService.AppMessages.GetString("AM_UploadFailed_Title");
                    if (transfer.isFolderTransfer())
                        message = ResourceService.AppMessages.GetString("AM_UploadFolderFailed");
                    else
                        message = ResourceService.AppMessages.GetString("AM_UploadFileFailed");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            UiService.OnUiThread(async () =>
            {
                await DialogService.ShowAlertAsync(title,
                    string.Format(message, transfer.getFileName()));
            });
        }

        #region MTransferListenerInterface

        //Will be called only for transfers started by startStreaming
        //Return true to continue getting data, false to stop the streaming
        public bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }

        public void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            // Extra checking to avoid NullReferenceException
            if (transfer == null) return;

            if (transfer.isFolderTransfer())
                FolderTransferFinish(api, transfer, e);
            else
                FileTransferFinish(api, transfer, e);
        }

        public void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            // Extra checking to avoid NullReferenceException
            if (transfer == null) return;

            UiService.OnUiThread(() =>
            {
                var megaTransfer = TransferService.AddTransferToList(TransferService.MegaTransfers, transfer);
                if (megaTransfer != null)
                {
                    megaTransfer.Transfer = transfer;
                    megaTransfer.IsBusy = api.areTransfersPaused((int)transfer.getType()) ? false : true;
                    megaTransfer.TransferState = api.areTransfersPaused((int)transfer.getType()) ? MTransferState.STATE_QUEUED : transfer.getState();
                    megaTransfer.TotalBytes = transfer.getTotalBytes();
                    megaTransfer.TransferPriority = transfer.getPriority();
                }
            });
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            switch(e.getErrorCode())
            {
                case MErrorType.API_EGOINGOVERQUOTA: // Not enough quota
                case MErrorType.API_EOVERQUOTA: // Overquota error
                    ProcessOverquotaError(api, e);
                    break;
            }

            // Extra checking to avoid NullReferenceException
            if (transfer == null) return;

            // Search the corresponding transfer in the transfers list
            var megaTransfer = TransferService.SearchTransfer(TransferService.MegaTransfers.SelectAll(), transfer);
            if (megaTransfer == null) return;

            var isBusy = api.areTransfersPaused((int)transfer.getType()) ? false : true;
            var transferState = api.areTransfersPaused((int)transfer.getType()) ? MTransferState.STATE_QUEUED : transfer.getState();
            var transferPriority = transfer.getPriority();

            UiService.OnUiThread(() =>
            {
                // Only update the values if they have changed to improve the UI performance
                if (megaTransfer.Transfer != transfer) megaTransfer.Transfer = transfer;
                if (megaTransfer.IsBusy != isBusy) megaTransfer.IsBusy = isBusy;
                if (megaTransfer.TransferState != transferState) megaTransfer.TransferState = transferState;
                if (megaTransfer.TransferPriority != transferPriority) megaTransfer.TransferPriority = transferPriority;
            });
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            // Extra checking to avoid NullReferenceException
            if (transfer == null) return;

            // Search the corresponding transfer in the transfers list
            var megaTransfer = TransferService.SearchTransfer(TransferService.MegaTransfers.SelectAll(), transfer);
            if (megaTransfer == null) return;

            var isBusy = api.areTransfersPaused((int)transfer.getType()) ? false : true;
            var transferState = api.areTransfersPaused((int)transfer.getType()) ? MTransferState.STATE_QUEUED : transfer.getState();
            var totalBytes = transfer.getTotalBytes();
            var transferedBytes = transfer.getTransferredBytes();
            var transferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();
            var transferMeanSpeed = transfer.getMeanSpeed();
            var transferPriority = transfer.getPriority();

            UiService.OnUiThread(() =>
            {
                // Only update the values if they have changed to improve the UI performance
                if (megaTransfer.Transfer != transfer) megaTransfer.Transfer = transfer;
                if (megaTransfer.IsBusy != isBusy) megaTransfer.IsBusy = isBusy;
                if (megaTransfer.TransferState != transferState) megaTransfer.TransferState = transferState;
                if (megaTransfer.TotalBytes != totalBytes) megaTransfer.TotalBytes = totalBytes;
                if (megaTransfer.TransferedBytes != transferedBytes) megaTransfer.TransferedBytes = transferedBytes;
                if (megaTransfer.TransferSpeed != transferSpeed) megaTransfer.TransferSpeed = transferSpeed;
                if (megaTransfer.TransferMeanSpeed != transferMeanSpeed) megaTransfer.TransferMeanSpeed = transferMeanSpeed;
                if (megaTransfer.TransferPriority != transferPriority) megaTransfer.TransferPriority = transferPriority;
            });
        }

        #endregion
    }
}