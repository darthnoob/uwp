using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class MultiFactorAuthEnabledDialogViewModel : BaseContentDialogViewModel
    {
        public MultiFactorAuthEnabledDialogViewModel() : base()
        {
            this.SaveKeyButtonCommand = new RelayCommand(SaveKey);
        }

        #region Commands

        /// <summary>
        /// Command invoked when the user select the "Backup Recovery key" option
        /// </summary>
        public ICommand SaveKeyButtonCommand { get; }

        #endregion

        #region Methods

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

            // If the recovery key has been successfully saved close the dialog
            this.OnHideDialog();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Uri image to display in the dialog
        /// </summary>
        public Uri MultiFactorAuthImageUri =>
            new Uri("ms-appx:///Assets/MultiFactorAuth/multiFactorAuth.png");

        #endregion

        #region AppMessages

        public string TitleText => ResourceService.AppMessages.GetString("AM_2FA_EnabledDialogTitle");
        public string DescriptionText => ResourceService.AppMessages.GetString("AM_2FA_EnabledDialogDescription");
        public string RecommendationText => ResourceService.AppMessages.GetString("AM_2FA_EnabledDialogRecommendation");

        #endregion

        #region UiResources

        public string ExportText => ResourceService.UiResources.GetString("UI_Export");

        #endregion
    }
}
