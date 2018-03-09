using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
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
        public TransferObjectModel(MegaSDK megaSdk, IMegaNode selectedNode, MTransferType transferType,
            string transferPath, string externalDownloadPath = null) : base(megaSdk)
        {
            Initialize(selectedNode, transferType, transferPath, externalDownloadPath);
        }

        private void Initialize(IMegaNode selectedNode, MTransferType transferType,
            string transferPath, string externalDownloadPath = null)
        {
            this.TypeAndState = new object[2];

            switch (transferType)
            {
                case MTransferType.TYPE_DOWNLOAD:
                    this.DisplayName = selectedNode.Name;
                    this.TotalBytes = selectedNode.Size;
                    break;

                case MTransferType.TYPE_UPLOAD:
                    this.DisplayName = Path.GetFileName(transferPath);
                    if (FileService.FileExists(transferPath))
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                var srcFile = await StorageFile.GetFileFromPathAsync(transferPath);
                                if (srcFile != null)
                                {
                                    var fileProperties = await srcFile.GetBasicPropertiesAsync();
                                    this.TotalBytes = fileProperties.Size;
                                }
                            }
                            catch (Exception e)
                            {
                                var message = (this.DisplayName == null) ? "Error getting transfer size" :
                                    string.Format("Error getting transfer size. File: '{0}'", this.DisplayName);
                                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, message, e);
                            }
                        });
                    }
                    break;
            }

            this.IsBusy = false;
            this.Type = transferType;
            this.TransferPath = transferPath;
            this.ExternalDownloadPath = externalDownloadPath;
            this.TransferState = MTransferState.STATE_NONE;
            this.TransferedBytes = 0;
            this.TransferSpeed = string.Empty;
            this.SelectedNode = selectedNode;
            this.AutoLoadImageOnFinish = false;
            this.PauseOrResumeTransferCommand = new RelayCommand(PauseOrResumeTransfer);
            this.CancelTransferCommand = new RelayCommand(CancelTransfer);
            this.RetryTransferCommand = new RelayCommand(RetryTransfer);
            this.RemoveTransferCommand = new RelayCommand(RemoveTransfer);
            SetThumbnail();
        }

        #region Commands

        public ICommand PauseOrResumeTransferCommand { get; set; }
        public ICommand CancelTransferCommand { get; set; }
        public ICommand RetryTransferCommand { get; set; }
        public ICommand RemoveTransferCommand { get; set; }

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
                case MTransferType.TYPE_DOWNLOAD:
                    // Download all nodes with the App instance of the SDK and authorize nodes to be downloaded with this SDK instance.
                    // Needed to allow transfers resumption of folder link nodes.
                    SdkService.MegaSdk.startDownloadWithAppData(this.MegaSdk.authorizeNode(this.SelectedNode.OriginalMNode),
                        this.TransferPath, TransferService.CreateTransferAppDataString(isSaveForOffline, this.ExternalDownloadPath));
                    this.IsSaveForOfflineTransfer = isSaveForOffline;
                    break;

                case MTransferType.TYPE_UPLOAD:
                    // Start uploads with the flag of temporary source activated to always automatically delete the 
                    // uploaded file from the upload temporary folder in the sandbox of the app
                    SdkService.MegaSdk.startUploadWithDataTempSource(this.TransferPath,
                        this.SelectedNode.OriginalMNode, string.Empty, true);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async void PauseOrResumeTransfer()
        {
            bool pause = this.TransferState == MTransferState.STATE_PAUSED ? false : true;

            var pauseTransfer = new PauseTransferRequestListenerAsync();
            var result = await pauseTransfer.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.pauseTransfer(this.Transfer, pause, pauseTransfer);
            });

            if (!result) return;
        }

        /// <summary>
        /// Cancels a transfer
        /// </summary>
        public void CancelTransfer()
        {
            // If the transfer is an upload and is being prepared (copying file to the upload temporary folder)
            if (this.Type == MTransferType.TYPE_UPLOAD && this.PreparingUploadCancelToken != null)
            {
                this.PreparingUploadCancelToken.Cancel();
                return;
            }

            // If the transfer is ready but not started for some reason
            if (!this.IsBusy && this.TransferState == MTransferState.STATE_NONE)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, string.Format("Transfer ({0}) canceled: {1}",
                    this.Type == MTransferType.TYPE_UPLOAD ? "UPLOAD" : "DOWNLOAD", this.DisplayName));
                this.TransferState = MTransferState.STATE_CANCELLED;
                return;
            }

            SdkService.MegaSdk.cancelTransfer(this.Transfer);
        }

        public void RetryTransfer()
        {
            SdkService.MegaSdk.retryTransfer(this.Transfer);
        }

        public void RemoveTransfer()
        {
            TransferService.MegaTransfers.Remove(this);
        }

        /// <summary>
        /// Sets the transfer thumbnail
        /// </summary>
        private void SetThumbnail()
        {
            switch (this.Type)
            {
                case MTransferType.TYPE_DOWNLOAD:
                    this.FileTypePathData = ImageService.GetDefaultFileTypePathData(this.SelectedNode.Name);
                    if (FileService.FileExists(this.SelectedNode.ThumbnailPath))
                        this.ThumbnailUri = new Uri(this.SelectedNode.ThumbnailPath);
                    break;

                case MTransferType.TYPE_UPLOAD:
                    if (ImageService.IsImage(this.TransferPath))
                        this.ThumbnailUri = new Uri(this.TransferPath);
                    else
                        this.FileTypePathData = ImageService.GetDefaultFileTypePathData(this.TransferPath);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Moves a downloaded file/folder to the final destination.
        /// Shows an error message if something went wrong.
        /// </summary>
        /// <param name="srcPath">Path of the source file/folder</param>
        /// <param name="destName">New name for the file/folder</param>
        /// <returns>Result of the action. TRUE if all went well or FALSE in other case.</returns>
        public async Task<bool> FinishDownload(string srcPath, string destName)
        {
            try
            {
                // If transfer is child of a folder transfer, doesn't need to do anything 
                // because the parent folder transfer will do the final required action.
                if (this.Transfer.getFolderTransferTag() > 0) return true;

                string defaultDownloadLocation = ResourceService.SettingsResources.GetString("SR_DefaultDownloadLocation");
                this.ExternalDownloadPath = this.ExternalDownloadPath ?? await SettingsService.LoadSettingAsync<string>(defaultDownloadLocation, null);
                if (string.IsNullOrWhiteSpace(this.ExternalDownloadPath)) return false;
                
                if (this.Transfer.isFolderTransfer())
                    await FolderService.MoveFolderAsync(srcPath, this.ExternalDownloadPath, destName);
                else
                    await FileService.MoveFileAsync(srcPath, this.ExternalDownloadPath, destName);

                return true;
            }
            catch (Exception e)
            {
                string alertMessage, logMessage;
                if(this.Transfer == null || string.IsNullOrWhiteSpace(destName))
                {
                    logMessage = "Error finishing a download to an external location";
                    alertMessage = ResourceService.AppMessages.GetString("AM_DownloadFailed");
                }
                else
                {
                    if (this.Transfer.isFolderTransfer())
                    {
                        logMessage = string.Format("Error finishing download folder '{0}'", destName);
                        alertMessage = string.Format(ResourceService.AppMessages.GetString("AM_DownloadFolderFailed"), destName);
                    }
                    else
                    {
                        logMessage = string.Format("Error finishing download file '{0}'", destName);
                        alertMessage = string.Format(ResourceService.AppMessages.GetString("AM_DownloadFileFailed"), destName);
                    }
                }

                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, logMessage, e);
                UiService.OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_DownloadFailed_Title"), alertMessage);
                });

                return false;
            }
        }

        #endregion

        #region Properties

        public string DisplayName { get; set; }
        public string TransferPath { get; private set; }
        public string ExternalDownloadPath { get; set; }
        public IMegaNode SelectedNode { get; private set; }

        private MTransferType _type;
        public MTransferType Type
        {
            get { return _type; }
            set
            {
                SetField(ref _type, value);
                this.TypeAndState[0] = value;
                OnPropertyChanged("TypeAndState");
            }
        }

        public CancellationTokenSource PreparingUploadCancelToken;

        public object[] TypeAndState { get; set; }

        private MTransfer _transfer;
        public MTransfer Transfer
        {
            get { return _transfer; }
            set { SetField(ref _transfer, value); }
        }

        public bool IsFolderTransfer => (this.Transfer != null) ? 
            this.Transfer.isFolderTransfer() : !Path.HasExtension(this.TransferPath);

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

        private MTransferState _transferState;
        public MTransferState TransferState
        {
            get { return _transferState; }
            set
            {
                SetField(ref _transferState, value);

                this.IsBusy = (value == MTransferState.STATE_ACTIVE) ? true : false;
                this.TypeAndState[1] = value;

                OnPropertyChanged("TypeAndState");
                OnPropertyChanged("IsAliveTransfer");
            }
        }

        private ulong _transferPriority;
        public ulong TransferPriority
        {
            get { return _transferPriority; }
            set { SetField(ref _transferPriority, value); }
        }

        private ulong _totalBytes;
        public ulong TotalBytes
        {
            get { return _totalBytes; }
            set { SetField(ref _totalBytes, value); }
        }

        public string TotalBytesText => this.TotalBytes.ToStringAndSuffix(1);

        private ulong _transferedBytes;
        public ulong TransferedBytes
        {
            get { return _transferedBytes; }
            set
            {
                SetField(ref _transferedBytes, value);
                OnPropertyChanged("TransferedPercentage");
                OnPropertyChanged("EstimatedTime");
                OnPropertyChanged("TransferedAndTotalBytes");
            }
        }

        public string TransferedAndTotalBytes => string.Format("{0:n2} / {1}",
            this.TransferedBytes.ToEqualSize(this.TotalBytes), this.TotalBytes.ToStringAndSuffix(1));

        public string TransferedPercentage => string.Format("{0}%", TransferedBytes * 100 / TotalBytes);

        private string _transferSpeed;
        public string TransferSpeed
        {
            get { return _transferSpeed; }
            set { SetField(ref _transferSpeed, value); }
        }

        private ulong _transferMeanSpeed;
        public ulong TransferMeanSpeed
        {
            get { return _transferMeanSpeed; }
            set
            {
                SetField(ref _transferMeanSpeed, value);
                OnPropertyChanged("EstimatedTime");
            }
        }

        public string EstimatedTime
        {
            get
            {
                if (TransferMeanSpeed == 0) return string.Empty;
                var t = TimeSpan.FromSeconds((TotalBytes - TransferedBytes) / TransferMeanSpeed);
                return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
            }
        }

        public bool IsAliveTransfer
        {
            get
            {
                switch (this.TransferState)
                {
                    case MTransferState.STATE_CANCELLED:
                    case MTransferState.STATE_COMPLETED:
                    case MTransferState.STATE_COMPLETING:
                    case MTransferState.STATE_FAILED:
                        return false;
                }

                return true;
            }
        }

        public bool IsActionAvailable
        {
            get
            {
                switch (this.TransferState)
                {
                    case MTransferState.STATE_COMPLETING:
                        return false;
                    default:
                        return true;
                }
            }
        }

        #endregion

        #region Ui_Resources

        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string PauseText => ResourceService.UiResources.GetString("UI_Pause");
        public string RemoveText => ResourceService.UiResources.GetString("UI_Remove");
        public string ResumeText => ResourceService.UiResources.GetString("UI_Resume");
        public string RetryText => ResourceService.UiResources.GetString("UI_Retry");

        #endregion

        #region VisualResources

        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string PausePathData => ResourceService.VisualResources.GetString("VR_PausePathData");
        public string ResumePathData => ResourceService.VisualResources.GetString("VR_PlayPathData");
        public string RemovePathData => ResourceService.VisualResources.GetString("VR_CancelPathData");

        #endregion
    }
}
