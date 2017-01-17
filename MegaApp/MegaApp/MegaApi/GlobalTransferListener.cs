using System;
using System.Linq;
using mega;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Services;
using MegaApp.ViewModels;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;

namespace MegaApp.MegaApi
{
    public class GlobalTransferListener: MTransferListenerInterface
    {
        #region MTransferListenerInterface

        //Will be called only for transfers started by startStreaming
        //Return true to continue getting data, false to stop the streaming
        public bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }
        
        public async void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)        
        {
            // Extra checking to avoid NullReferenceException
            if (transfer == null) return;

            // Use a temp variable to avoid InvalidOperationException
            var transfersList = TransferService.MegaTransfers.SelectAll();

            // Extra checking during finding to avoid NullReferenceException
            var megaTransfer = transfersList.FirstOrDefault(t => 
                t.Transfer != null && t.Transfer.getTag() == transfer.getTag() ||
                t.TransferPath.Equals(transfer.getPath()));
            
            if(megaTransfer != null)
            {
                UiService.OnUiThread(() =>
                {
                    TransferService.GetTransferAppData(transfer, megaTransfer);

                    megaTransfer.TotalBytes = transfer.getTotalBytes();
                    megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                    megaTransfer.TransferSpeed = string.Empty;
                    megaTransfer.IsBusy = false;
                });

                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK:
                        {
                            UiService.OnUiThread(() => megaTransfer.TransferedBytes = megaTransfer.TotalBytes);

                            switch (megaTransfer.Type)
                            {
                                case TransferType.Download:

                                    bool result = true;

                                    // If is a folder transfer
                                    if(transfer.isFolderTransfer())
                                    {
                                        var folderNode = megaTransfer.SelectedNode as FolderNodeViewModel;
                                        if (folderNode != null)
                                            result = await megaTransfer.FinishDownload(megaTransfer.TransferPath, folderNode.Name);
                                    }
                                    else
                                    {
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
                                    }

                                    if(!result)
                                        UiService.OnUiThread(() => megaTransfer.Status = TransferStatus.Error);
                                    else
                                        UiService.OnUiThread(() => megaTransfer.Status = TransferStatus.Downloaded);
                                    break;

                                case TransferType.Upload:
                                    UiService.OnUiThread(() => megaTransfer.Status = TransferStatus.Uploaded);
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                    case MErrorType.API_EOVERQUOTA:
                        {
                            // Stop all upload transfers
                            api.cancelTransfers((int)MTransferType.TYPE_UPLOAD);

                            // Disable the "camera upload" service
                            //MediaService.SetAutoCameraUpload(false);
                            SettingsService.SaveSetting(ResourceService.SettingsResources.GetString("SR_CameraUploadsIsEnabled"), false);

                            UiService.OnUiThread(() => DialogService.ShowOverquotaAlert());
                            break;
                        }
                    case MErrorType.API_EINCOMPLETE:
                        {
                            UiService.OnUiThread(() => megaTransfer.Status = TransferStatus.Canceled);
                            break;
                        }
                    default:
                        {
                            UiService.OnUiThread(() => megaTransfer.Status = TransferStatus.Error);
                            switch (megaTransfer.Type)
                            {
                                case TransferType.Download:
                                    UiService.OnUiThread(async() =>
                                    {
                                        await DialogService.ShowAlertAsync(
                                            ResourceService.AppMessages.GetString("AM_DownloadNodeFailed_Title"),
                                            string.Format(ResourceService.AppMessages.GetString("AM_DownloadNodeFailed"), e.getErrorString()));
                                    });
                                    break;

                                case TransferType.Upload:
                                    UiService.OnUiThread(async() =>
                                    {
                                        await DialogService.ShowAlertAsync(
                                            ResourceService.AppMessages.GetString("AM_UploadNodeFailed_Title"),
                                            string.Format(ResourceService.AppMessages.GetString("AM_UploadNodeFailed"), e.getErrorString()));
                                    });
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                }
            }
        }

        public void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            TransferService.AddTransferToList(TransferService.MegaTransfers, transfer);
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            // Extra checking to avoid NullReferenceException
            if (transfer == null) return;

            // Use a temp variable to avoid InvalidOperationException
            var transfersList = TransferService.MegaTransfers.SelectAll();

            // Extra checking during finding to avoid NullReferenceException
            var megaTransfer = transfersList.FirstOrDefault(t => 
                t.Transfer != null && t.Transfer.getTag() == transfer.getTag() ||
                t.TransferPath.Equals(transfer.getPath()));

            if(megaTransfer != null)
            {
                UiService.OnUiThread(() =>
                {
                    megaTransfer.IsBusy = true;
                    megaTransfer.TotalBytes = transfer.getTotalBytes();
                    megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                    megaTransfer.TransferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();

                    if (megaTransfer.TransferedBytes > 0)
                    {
                        switch (megaTransfer.Type)
                        {
                            case TransferType.Download:
                                megaTransfer.Status = TransferStatus.Downloading;
                                break;
                            case TransferType.Upload:
                                megaTransfer.Status = TransferStatus.Uploading;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                });
            }
        }

        #endregion
    }
}
