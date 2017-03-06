using System;
using mega;
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
                    switch (transfer.getType())
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            var folderNode = megaTransfer.SelectedNode as FolderNodeViewModel;
                            if (folderNode != null)
                            {
                                if (!await megaTransfer.FinishDownload(megaTransfer.TransferPath, folderNode.Name))
                                    UiService.OnUiThread(() => megaTransfer.TransferState = MTransferState.STATE_FAILED);
                            }
                            break;

                        case MTransferType.TYPE_UPLOAD:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case MErrorType.API_EOVERQUOTA:
                    ProcessOverquotaError(api);
                    break;

                case MErrorType.API_EINCOMPLETE:
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
                    switch (megaTransfer.Type)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            bool result = true;

                            //If is download transfer of an image file 
                            var imageNode = megaTransfer.SelectedNode as ImageNodeViewModel;
                            if (imageNode != null)
                            {
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

                                result = await megaTransfer.FinishDownload(megaTransfer.TransferPath, imageNode.Name);
                            }
                            else //If is a download transfer of other file type
                            {
                                var node = megaTransfer.SelectedNode as FileNodeViewModel;
                                if (node != null)
                                    result = await megaTransfer.FinishDownload(megaTransfer.TransferPath, node.Name);
                            }

                            UiService.OnUiThread(() =>
                            {
                                if (!result)
                                    megaTransfer.TransferState = MTransferState.STATE_FAILED;
                                else
                                    TransferService.MoveMegaTransferToCompleted(TransferService.MegaTransfers, megaTransfer);
                            });
                            break;

                        case MTransferType.TYPE_UPLOAD:
                            UiService.OnUiThread(() =>
                                TransferService.MoveMegaTransferToCompleted(TransferService.MegaTransfers, megaTransfer));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case MErrorType.API_EOVERQUOTA:
                    ProcessOverquotaError(api);
                    break;

                case MErrorType.API_EINCOMPLETE:
                    break;

                default:
                    ProcessDefaultError(transfer);
                    break;
            }
        }

        /// <summary>
        /// This function is called when a transfer fails by an over quota error.
        /// It does the needed actions to process this kind of error.
        /// </summary>
        /// <param name="api">MegaApi object that started the transfer</param>
        private void ProcessOverquotaError(MegaSDK api)
        {
            // Stop all upload transfers
            api.cancelTransfers((int)MTransferType.TYPE_UPLOAD);

            // Disable the "camera upload" service
            //MediaService.SetAutoCameraUpload(false);
            SettingsService.Save(ResourceService.SettingsResources.GetString("SR_CameraUploadsIsEnabled"), false);

            UiService.OnUiThread(DialogService.ShowOverquotaAlert);
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
                    megaTransfer.TransferPriority = transfer.getPriority();
                }
            });
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            // Extra checking to avoid NullReferenceException
            if (transfer == null) return;

            // Search the corresponding transfer in the transfers list
            var megaTransfer = TransferService.SearchTransfer(TransferService.MegaTransfers.SelectAll(), transfer);
            if (megaTransfer == null) return;

            UiService.OnUiThread(() =>
            {
                megaTransfer.Transfer = transfer;
                megaTransfer.IsBusy = api.areTransfersPaused((int)transfer.getType()) ? false : true;
                megaTransfer.TransferState = api.areTransfersPaused((int)transfer.getType()) ? MTransferState.STATE_QUEUED : transfer.getState();
                megaTransfer.TransferPriority = transfer.getPriority();
            });
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            // Extra checking to avoid NullReferenceException
            if (transfer == null) return;

            // Search the corresponding transfer in the transfers list
            var megaTransfer = TransferService.SearchTransfer(TransferService.MegaTransfers.SelectAll(), transfer);
            if (megaTransfer == null) return;

            UiService.OnUiThread(() =>
            {
                megaTransfer.Transfer = transfer;
                megaTransfer.IsBusy = api.areTransfersPaused((int)transfer.getType()) ? false : true;
                megaTransfer.TransferState = api.areTransfersPaused((int)transfer.getType()) ? MTransferState.STATE_QUEUED : transfer.getState();
                megaTransfer.TotalBytes = transfer.getTotalBytes();
                megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                megaTransfer.TransferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();
                megaTransfer.TransferMeanSpeed = transfer.getMeanSpeed();
                megaTransfer.TransferPriority = transfer.getPriority();
            });
        }

        #endregion
    }
}