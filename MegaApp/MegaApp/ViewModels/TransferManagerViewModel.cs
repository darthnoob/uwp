using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class TransferManagerViewModel : BasePageViewModel
    {
        public TransferManagerViewModel()
        {
            this.CancelTransferCommand = new RelayCommand(CancelTransfer);
        }

        #region Commands

        public ICommand CancelTransferCommand { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Pause or resume all transfers.
        /// </summary>
        /// <param name="pause">
        /// Boolean value to indicate if pause or resume transfers.
        /// - TRUE: pause.
        /// - FALSE: resume.
        /// </param>
        public async Task PauseAll(bool pause)
        {
            var pauseTransfers = new PauseTransfersRequestListenerAsync();
            bool result = await pauseTransfers.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.pauseTransfers(pause, pauseTransfers);
            });

            if (result)
                SetTransfersListStatus(MegaTransfers, pause);
        }

        /// <summary>
        /// Pause or resume only download transfers.
        /// </summary>
        /// <param name="pause">
        /// Boolean value to indicate if pause or resume transfers.
        /// - TRUE: pause.
        /// - FALSE: resume.
        /// </param>
        public async Task PauseDownloads(bool pause)
        {
            var pauseDownloads = new PauseTransfersRequestListenerAsync();
            bool result = await pauseDownloads.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.pauseTransfersDirection(pause, 
                    (int)MTransferType.TYPE_DOWNLOAD, pauseDownloads);
            });

            if(result)
                SetTransfersListStatus(MegaTransfers.Downloads, pause);
        }

        /// <summary>
        /// Pause or resume only upload transfers.
        /// </summary>
        /// <param name="pause">
        /// Boolean value to indicate if pause or resume transfers.
        /// - TRUE: pause.
        /// - FALSE: resume.
        /// </param>
        public async Task PauseUploads(bool pause)
        {
            var pauseUploads = new PauseTransfersRequestListenerAsync();
            bool result = await pauseUploads.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.pauseTransfersDirection(pause,
                    (int)MTransferType.TYPE_UPLOAD, pauseUploads);
            });

            if (result)
                SetTransfersListStatus(MegaTransfers.Uploads, pause);
        }

        /// <summary>
        /// Cancel all transfers.
        /// </summary>        
        public void CancelAll()
        {
            CancelDownloads();
            CancelUploads();
        }

        /// <summary>
        /// Cancel only download transfers.
        /// </summary>
        public void CancelDownloads()
        {
            SdkService.MegaSdk.cancelTransfers((int)MTransferType.TYPE_DOWNLOAD);
        }

        /// <summary>
        /// Cancel only upload transfers.
        /// </summary>
        public void CancelUploads()
        {
            SdkService.MegaSdk.cancelTransfers((int)MTransferType.TYPE_UPLOAD);
        }

        /// <summary>
        /// Cancel the selected transfer of the list.
        /// </summary>
        private void CancelTransfer()
        {
            SdkService.MegaSdk.cancelTransfer(FocusedTransfer.Transfer);
        }

        /// <summary>
        /// Sets the status of the transfers of a transfers list
        /// </summary>
        /// <param name="transfersList">The transfers list to set the status.</param>
        /// <param name="paused">Boolean value which indicates is the queue of the list is paused or not.</param>
        private void SetTransfersListStatus(ObservableCollection<TransferObjectModel> transfersList, bool paused)
        {
            var numTransfers = transfersList.Count;
            for (int i = 0; i < numTransfers; i++)
            {
                var item = transfersList.ElementAt(i);
                if (item == null) continue;

                if (item.TransferedBytes < item.TotalBytes || item.TransferedBytes == 0)
                {
                    switch (item.Status)
                    {
                        case TransferStatus.Downloading:
                        case TransferStatus.Uploading:
                        case TransferStatus.Queued:
                            if (paused)
                                OnUiThread(() =>
                                {
                                    item.Status = TransferStatus.Paused;
                                    item.TransferSpeed = string.Empty;
                                });
                            break;

                        case TransferStatus.Paused:
                            if (!paused)
                                OnUiThread(() => item.Status = TransferStatus.Queued);
                            break;
                    }
                }
            }
        }

        #endregion

        #region Properties

        public bool AreDownloadsPaused => SdkService.MegaSdk.areTransfersPaused((int)MTransferType.TYPE_DOWNLOAD);
        public bool AreUploadsPaused => SdkService.MegaSdk.areTransfersPaused((int)MTransferType.TYPE_UPLOAD);
        public TransferQueue MegaTransfers => TransferService.MegaTransfers;
        public TransferObjectModel FocusedTransfer;

        #endregion

        #region Ui_Resources

        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string CancelDownloadsText => ResourceService.UiResources.GetString("UI_CancelDownloads");
        public string CancelUploadsText => ResourceService.UiResources.GetString("UI_CancelUploads");
        public string DownloadsText => ResourceService.UiResources.GetString("UI_Downloads");
        public string PauseText => ResourceService.UiResources.GetString("UI_Pause");
        public string ResumeText => ResourceService.UiResources.GetString("UI_Resume");
        public string UploadsText => ResourceService.UiResources.GetString("UI_Uploads");

        #endregion
    }
}
