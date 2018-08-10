using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class MultiFactorAuthEnabledDialogViewModel : BaseContentDialogViewModel
    {
        public MultiFactorAuthEnabledDialogViewModel() : base()
        {
            this.SaveKeyButtonCommand = new RelayCommand(SaveKey);

            this.TitleText = ResourceService.AppMessages.GetString("AM_2FA_EnabledDialogTitle");
            this.MessageText = ResourceService.AppMessages.GetString("AM_2FA_EnabledDialogDescription");

            this.Initialize();
        }

        #region Commands

        /// <summary>
        /// Command invoked when the user select the "Backup Recovery key" option
        /// </summary>
        public ICommand SaveKeyButtonCommand { get; }

        #endregion

        #region Methods

        private async void Initialize()
        {
            var isRecoveryKeyExported = new IsMasterKeyExportedRequestListenerAsync();
            this.IsRecoveryKeyExported = await isRecoveryKeyExported.ExecuteAsync(() =>
                SdkService.MegaSdk.isMasterKeyExported(isRecoveryKeyExported));
        }

        /// <summary>
        /// Backup the Recovery key
        /// </summary>
        private async void SaveKey()
        {
            var saveKeyCommand = SettingsService.RecoveryKeySetting.SaveKeyCommand as RelayCommandAsync<bool>;
            if (saveKeyCommand == null) return;

            if (!saveKeyCommand.CanExecute(null)) return;
            var recoveryKeySaved = await saveKeyCommand.ExecuteAsync(null);
            if (!recoveryKeySaved) return;

            this.IsRecoveryKeyExported = recoveryKeySaved;

            // If the recovery key has been successfully saved close the dialog
            this.OnHideDialog();
        }

        #endregion

        #region Properties

        private bool _isRecoveryKeyExported;
        public bool IsRecoveryKeyExported
        {
            get { return _isRecoveryKeyExported; }
            set { SetField(ref _isRecoveryKeyExported, value); }
        }

        /// <summary>
        /// Uri image to display in the dialog
        /// </summary>
        public Uri MultiFactorAuthImageUri =>
            new Uri("ms-appx:///Assets/MultiFactorAuth/multiFactorAuth.png");

        #endregion

        #region AppMessages

        public string RecommendationText => ResourceService.AppMessages.GetString("AM_2FA_EnabledDialogRecommendation");

        #endregion

        #region UiResources

        public string ExportText => ResourceService.UiResources.GetString("UI_Export");

        #endregion
    }
}
