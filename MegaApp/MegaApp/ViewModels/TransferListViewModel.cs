using System;
using System.Collections.Generic;
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
            this.ResumeCommand = new RelayCommand(ResumeTransfers);
            this.PauseCommand = new RelayCommand(PauseTransfers);
            this.CancelCommand = new RelayCommand(CancelTransfers);
            this.CleanCommand = new RelayCommand(UpdateTransfers);
        }

        public async void ResumeTransfers()
        {
            await ResumePauseTransfersAsync(false);
        }

        public async void PauseTransfers()
        {
            await ResumePauseTransfersAsync(true);
        }

        public void UpdateTransfers()
        {
            TransferService.UpdateMegaTransferList(TransferService.MegaTransfers, this.Type);
        }

        public async Task ResumePauseTransfersAsync(bool isPauseEnabled)
        {
            OnUiThread(() => this.IsPauseEnabled = isPauseEnabled);

            var pauseTransfers = new PauseTransfersRequestListenerAsync();
            var result = await pauseTransfers.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.pauseTransfersDirection(isPauseEnabled,
                    (int)this.Type, pauseTransfers);
            });

            if (!result) return;

            // Use a temp variable to avoid InvalidOperationException
            SetStatus(this.Items.ToList(), isPauseEnabled);
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

        public async void CancelTransfers()
        {
            var result = await DialogService.ShowOkCancelAsync(
                this.CancelTransfersTitleText,
                this.CancelTransfersDescriptionText);

            if (!result) return;

            SdkService.MegaSdk.cancelTransfers((int)this.Type);
        }

        #region Commands

        public ICommand ResumeCommand { get; }

        public ICommand PauseCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand CleanCommand { get; }

        #endregion

        #region Properties

        public string Description { get; }

        public MTransferType Type { get; set; }

        public ObservableCollection<TransferObjectModel> Items { get; }

        private bool _isPauseEnabled;
        public bool IsPauseEnabled
        {
            get { return _isPauseEnabled; }
            set { SetField(ref _isPauseEnabled, value); }
        }

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
