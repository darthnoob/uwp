using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;
//using MegaApp.Database;

namespace MegaApp.ViewModels
{
    public class TransferObjectModel : BaseSdkViewModel
    {
        /// <summary>
        /// Object which represents a transfer and all its properties
        /// </summary>
        /// <param name="selectedNode">
        /// The selected file/folder for the transfer.
        /// - Downloads: selected file/folder to be downloaded.
        /// - Uploads: selected destination folder to upload a new node.
        /// </param>
        /// <param name="transferType">
        /// Indicates the type of transfer (Download or Upload)
        /// </param>
        /// <param name="transferPath">
        /// The file/folder path for the transfer
        /// - Downloads: local download path for the selected file/folder
        /// - Uploads: local path of the selected file/folder to be uploaded
        /// </param>
        /// <param name="externalDownloadPath">
        /// Only for downloads. External download path to the application for the selected file / folder
        /// </param>
        public TransferObjectModel(IMegaNode selectedNode, TransferType transferType,
            string transferPath, string externalDownloadPath = null)
        {
            switch (transferType)
            {
                case TransferType.Download:
                    this.DisplayName = selectedNode.Name;
                    break;

                case TransferType.Upload:
                    this.DisplayName = Path.GetFileName(transferPath);
                    break;            
            }

            this.Type = transferType;
            this.TransferPath = transferPath;
            this.ExternalDownloadPath = externalDownloadPath;
            this.Status = TransferStatus.NotStarted;
            this.SelectedNode = selectedNode;
            this.AutoLoadImageOnFinish = false;
            this.CancelTransferCommand = new RelayCommand(CancelTransfer);
            SetThumbnail();
        }

        #region Commands

        public ICommand CancelTransferCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Starts a tranfer
        /// </summary>
        /// <param name="isSaveForOffline">
        /// Boolean value which indicates if is a "save for offline" transfer or not
        /// </param>
        public void StartTransfer(bool isSaveForOffline = false)
        {
            switch (this.Type)
            {
                case TransferType.Download:
                    // Download all nodes with the App instance of the SDK and authorize nodes to be downloaded with this SDK instance.
                    // Needed to allow transfers resumption of folder link nodes.
                    SdkService.MegaSdk.startDownloadWithAppData(this.MegaSdk.authorizeNode(this.SelectedNode.OriginalMNode), this.TransferPath, TransferService.CreateTransferAppDataString(isSaveForOffline, this.ExternalDownloadPath));
                    this.IsSaveForOfflineTransfer = isSaveForOffline;
                    break;

                case TransferType.Upload:
                    // Start uploads with the flag of temporary source activated to always automatically delete the 
                    // uploaded file from the upload temporary folder in the sandbox of the app
                    SdkService.MegaSdk.startUploadWithDataTempSource(this.TransferPath, this.SelectedNode.OriginalMNode, string.Empty, true);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Cancels a transfer
        /// </summary>
        public void CancelTransfer()
        {
            if (!this.IsBusy)
            {
                if (this.Status == TransferStatus.NotStarted)
                    this.Status = TransferStatus.Canceled;
                return;
            }
            this.Status = TransferStatus.Canceling;
            SdkService.MegaSdk.cancelTransfer(this.Transfer);
        }

        /// <summary>
        /// Sets the transfer thumbnail
        /// </summary>
        private void SetThumbnail()
        {
            switch (this.Type)
            {
                case TransferType.Download:
                    this.IsDefaultImage = true;
                    this.FileTypePathData = ImageService.GetDefaultFileTypePathData(this.SelectedNode.Name);
                    if (FileService.FileExists(this.SelectedNode.ThumbnailPath))
                    {
                        this.IsDefaultImage = false;
                        this.ThumbnailUri = new Uri(this.SelectedNode.ThumbnailPath);
                    }
                    break;

                case TransferType.Upload:
                    if (ImageService.IsImage(this.TransferPath))
                    {
                        this.IsDefaultImage = false;
                        this.ThumbnailUri = new Uri(this.TransferPath);
                    }
                    else
                    {
                        this.IsDefaultImage = true;
                        this.FileTypePathData = ImageService.GetDefaultFileTypePathData(this.TransferPath);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsAliveTransfer()
        {
            switch (this.Status)
            {
                case TransferStatus.Canceled:
                case TransferStatus.Downloaded:
                case TransferStatus.Uploaded:
                case TransferStatus.Error:
                    return false;
            }

            return true;
        }

        public async Task<bool> FinishDownload(string srcPath, string destName)
        {
            try
            {
                // If transfer is child of a folder transfer, doesn't need to do anything 
                // because the parent folder transfer will do the final required action.
                if (this.Transfer.getFolderTransferTag() > 0) return true;

                string defaultDownloadLocation = ResourceService.SettingsResources.GetString("SR_DefaultDownloadLocation");
                this.ExternalDownloadPath = this.ExternalDownloadPath ?? SettingsService.LoadSetting<string>(defaultDownloadLocation, null);
                if (this.ExternalDownloadPath == null) return false;
                
                if (this.Transfer.isFolderTransfer())
                    await FolderService.MoveFolder(srcPath, this.ExternalDownloadPath, destName);
                else
                    await FileService.MoveFile(srcPath, this.ExternalDownloadPath, destName);

                return true;
            }
            catch(Exception e)
            {
                OnUiThread(async() =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_DownloadNodeFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_DownloadNodeFailed"), e.Message));
                });                
                return false;
            }
        }

        #endregion

        #region Properties

        public string DisplayName { get; set; }
        public string TransferPath { get; private set; }
        public string ExternalDownloadPath { get; set; }
        public TransferType Type { get; set; }
        public IMegaNode SelectedNode { get; private set; }

        private MTransfer _transfer;
        public MTransfer Transfer
        {
            get { return _transfer; }
            set { SetField(ref _transfer, value); }
        }

        private bool _isDefaultImage;
        public bool IsDefaultImage
        {
            get { return _isDefaultImage; }
            set { SetField(ref _isDefaultImage, value); }
        }

        private Uri _thumbnailUri;
        public Uri ThumbnailUri
        {
            get { return _thumbnailUri; }
            set { SetField(ref _thumbnailUri, value); }
        }

        private string _fileTypePathData;
        public string FileTypePathData
        {
            get { return _fileTypePathData; }
            set { SetField(ref _fileTypePathData, value); }
        }

        public bool AutoLoadImageOnFinish { get; set; }

        public bool IsSaveForOfflineTransfer { get; set; }

        private TransferStatus _transferStatus;
        public TransferStatus Status
        {
            get { return _transferStatus; }
            set { SetField(ref _transferStatus, value); }
        }

        private ulong _totalBytes;
        public ulong TotalBytes
        {
            get { return _totalBytes; }
            set { SetField(ref _totalBytes, value); }
        }

        private ulong _transferedBytes;
        public ulong TransferedBytes
        {
            get { return _transferedBytes; }
            set { SetField(ref _transferedBytes, value); }
        }

        private string _transferSpeed;
        public string TransferSpeed
        {
            get { return _transferSpeed; }
            set { SetField(ref _transferSpeed, value); }
        }

        public bool IsActionAvailable
        {
            get
            {
                switch (this.Status)
                {
                    case TransferStatus.Error:
                    case TransferStatus.Canceled:
                    case TransferStatus.Canceling:
                    case TransferStatus.Downloaded:
                    case TransferStatus.Uploaded:
                        return false;
                    default:
                        return true;
                }
            }
        }

        #endregion

        #region Ui_Resources

        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");

        #endregion
    }
}
