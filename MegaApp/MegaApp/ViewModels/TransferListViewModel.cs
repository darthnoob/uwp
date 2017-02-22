using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    /// <summary>
    /// Viewmodel to display transfers in a list
    /// </summary>
    public class TransferListViewModel: BaseSdkViewModel
    {
        public TransferListViewModel(MTransferType type)
        {
            this.Type = type;
            switch (this.Type)
            {
                case MTransferType.TYPE_DOWNLOAD:
                    this.Description = ResourceService.UiResources.GetString("UI_Downloads");
                    this.CancelTransfersTitleText = ResourceService.UiResources.GetString("UI_CancelDownloads");
                    this.CancelTransfersDescriptionText = ResourceService.AppMessages.GetString("AM_CancelDownloadsQuestion");
                    this.Items = TransferService.MegaTransfers.Downloads;
                    break;
                case MTransferType.TYPE_UPLOAD:
                    this.Description = ResourceService.UiResources.GetString("UI_Uploads");
                    this.CancelTransfersTitleText = ResourceService.UiResources.GetString("UI_CancelUploads");
                    this.CancelTransfersDescriptionText = ResourceService.AppMessages.GetString("AM_CancelUploadsQuestion");
                    this.Items = TransferService.MegaTransfers.Uploads;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.IsPauseEnabled = this.MegaSdk.areTransfersPaused((int) this.Type);
            this.IsCompletedTransfersList = false;
            this.PauseOrResumeCommand = new RelayCommand(PauseOrResumeTransfers);
            this.CancelCommand = new RelayCommand(CancelTransfers);
            this.CleanCommand = new RelayCommand<bool>(UpdateTransfers);
        }

        public TransferListViewModel()
        {
            this.Description = ResourceService.UiResources.GetString("UI_Completed");
            this.IsCompletedTransfersList = true;
            this.Items = TransferService.MegaTransfers.Completed;
            this.IsPauseEnabled = false;            
            this.CleanCommand = new RelayCommand(CleanCompletedTransfers);
        }

        public async void PauseOrResumeTransfers()
        {
            await PauseOrResumeTransfersAsync();
        }

        public void UpdateTransfers(bool cleanTransfers = false)
        {
            TransferService.UpdateMegaTransferList(TransferService.MegaTransfers, this.Type, cleanTransfers);
        }

        private void CleanCompletedTransfers()
        {
            TransferService.MegaTransfers.Completed.Clear();
        }

        private async Task PauseOrResumeTransfersAsync()
        {
            var playPauseStatus = !IsPauseEnabled;
            OnUiThread(() => this.IsPauseEnabled = playPauseStatus);

            var pauseTransfers = new PauseTransfersRequestListenerAsync();
            var result = await pauseTransfers.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.pauseTransfersDirection(playPauseStatus,
                    (int)this.Type, pauseTransfers);
            });

            if (!result) return;

            // Use a temp variable to avoid InvalidOperationException
            SetStatus(this.Items.ToList(), playPauseStatus);
        }

        private void SetStatus(ICollection<TransferObjectModel> items, bool playPauseStatus)
        {
            foreach (var transferObjectModel in items)
            {
                if (transferObjectModel.TransferedBytes < transferObjectModel.TotalBytes ||
                    transferObjectModel.TransferedBytes == 0)
                {
                    switch (transferObjectModel.Status)
                    {
                        case TransferStatus.Downloading:
                        case TransferStatus.Uploading:
                        case TransferStatus.Queued:
                        {
                            if (playPauseStatus)
                            {
                                OnUiThread(() =>
                                {
                                    transferObjectModel.Status = TransferStatus.Paused;
                                    transferObjectModel.TransferSpeed = string.Empty;
                                });
                            }
                            break;
                        }
                        case TransferStatus.Paused:
                        {
                            if (!playPauseStatus)
                            {
                                OnUiThread(() => transferObjectModel.Status = TransferStatus.Queued);
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cancel all transfers of the current type.        
        /// </summary>
        public async void CancelTransfers()
        {
            var result = await DialogService.ShowOkCancelAsync(
                this.CancelTransfersTitleText,
                this.CancelTransfersDescriptionText);

            if (!result) return;

            // Use a temp list to avoid InvalidOperationException
            var transfers = Items.ToList();
            foreach (var transfer in transfers)
            {
                // If the transfer is an upload and is being prepared (copying file to the upload temporary folder)
                if (this.Type == MTransferType.TYPE_UPLOAD && transfer?.PreparingUploadCancelToken != null)
                {
                    transfer.Status = TransferStatus.Canceling;
                    transfer.PreparingUploadCancelToken.Cancel();
                }
                // If the transfer is ready but not started for some reason
                else if (transfer?.IsBusy == false && transfer?.Status == TransferStatus.NotStarted)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO, string.Format("Transfer ({0}) canceled: {1}",
                        this.Type == MTransferType.TYPE_UPLOAD? "UPLOAD" : "DOWNLOAD", transfer.DisplayName));                    
                    transfer.Status = TransferStatus.Canceled;
                }
                else
                {
                    transfer.Status = TransferStatus.Canceling;
                }
            }

            SdkService.MegaSdk.cancelTransfers((int)this.Type);
        }

        #region Commands

        public ICommand PauseOrResumeCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CleanCommand { get; }

        #endregion

        #region Properties

        public string Description { get; }

        public MTransferType Type { get; set; }

        public ObservableCollection<TransferObjectModel> Items { get; }

        private bool _isCompletedTransfersList;
        public bool IsCompletedTransfersList
        {
            get { return _isCompletedTransfersList; }
            set { SetField(ref _isCompletedTransfersList, value); }
        }

        private bool _isPauseEnabled;
        public bool IsPauseEnabled
        {
            get { return _isPauseEnabled; }
            set
            {
                SetField(ref _isPauseEnabled, value);
                OnPropertyChanged("PauseOrResumeText");
                OnPropertyChanged("PauseOrResumeIcon");
            }
        }

        public string PauseOrResumeText => IsPauseEnabled ? ResumeText : PauseText;
        public SymbolIcon PauseOrResumeIcon => IsPauseEnabled ? new SymbolIcon(Symbol.Play) : new SymbolIcon(Symbol.Pause);

        #endregion

        #region Ui_Resources

        public string PauseText => ResourceService.UiResources.GetString("UI_Pause");
        public string CancelAllText => ResourceService.UiResources.GetString("UI_CancelAll");
        public string ResumeText => ResourceService.UiResources.GetString("UI_Resume");
        public string CleanUpTransfersText => ResourceService.UiResources.GetString("UI_CleanUpTransfers");
        public string CancelTransfersTitleText { get; }
        public string CancelTransfersDescriptionText { get; }

        #endregion
    }
}
