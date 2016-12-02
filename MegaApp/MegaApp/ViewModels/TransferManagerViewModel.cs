using MegaApp.Classes;
using MegaApp.Services;
using mega;

namespace MegaApp.ViewModels
{
    public class TransferManagerViewModel : BasePageViewModel
    {
        # region Methods

        /// <summary>
        /// Pause or resume all transfers.
        /// </summary>
        /// <param name="pause">
        /// Boolean value to indicate if pause or resume transfers.
        /// - TRUE: pause.
        /// - FALSE: resume.
        /// </param>
        public void PauseAll(bool pause)
        {
            SdkService.MegaSdk.pauseTransfers(pause);
        }

        /// <summary>
        /// Pause or resume only download transfers.
        /// </summary>
        /// <param name="pause">
        /// Boolean value to indicate if pause or resume transfers.
        /// - TRUE: pause.
        /// - FALSE: resume.
        /// </param>
        public void PauseDownloads(bool pause)
        {
            SdkService.MegaSdk.pauseTransfersDirection(pause, (int)MTransferType.TYPE_DOWNLOAD);
        }

        /// <summary>
        /// Pause or resume only upload transfers.
        /// </summary>
        /// <param name="pause">
        /// Boolean value to indicate if pause or resume transfers.
        /// - TRUE: pause.
        /// - FALSE: resume.
        /// </param>
        public void PauseUploads(bool pause)
        {
            SdkService.MegaSdk.pauseTransfersDirection(pause, (int)MTransferType.TYPE_UPLOAD);
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

        #endregion Properties

        public TransferQueue MegaTransfers => TransferService.MegaTransfers;

        #region

        #endregion

        #region Ui_Resources

        public string DownloadsText => ResourceService.UiResources.GetString("UI_Downloads");
        public string PauseText => ResourceService.UiResources.GetString("UI_Pause");
        public string ResumeText => ResourceService.UiResources.GetString("UI_Resume");
        public string UploadsText => ResourceService.UiResources.GetString("UI_Uploads");

        #endregion
    }
}
