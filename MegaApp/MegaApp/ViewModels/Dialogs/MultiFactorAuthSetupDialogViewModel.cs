using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.Views.MultiFactorAuth;

namespace MegaApp.ViewModels.Dialogs
{
    public class MultiFactorAuthSetupDialogViewModel : BaseContentDialogViewModel
    {
        public MultiFactorAuthSetupDialogViewModel() : base()
        {
            this.SetupTwoFactorAuthCommand = new RelayCommand(SetupTwoFactorAuth);
        }

        #region Commands

        public ICommand SetupTwoFactorAuthCommand { get; }

        #endregion

        #region Properties

        /// <summary>
        /// Result value of the dialog.
        /// TRUE if the user continues with the setup process or FALSE in other case.
        /// </summary>
        public bool DialogResult = false;

        /// <summary>
        /// Uri image to display in the dialog
        /// </summary>
        public Uri MultiFactorAuthImageUri => 
            new Uri("ms-appx:///Assets/MultiFactorAuth/multiFactorAuth.png");

        #endregion

        #region Methods

        private void SetupTwoFactorAuth()
        {
            this.DialogResult = true;

            if (!this.CloseCommand.CanExecute(null)) return;
            this.CloseCommand.Execute(null);

            NavigateService.Instance.Navigate(typeof(MultiFactorAuthAppSetupPage));
        }

        #endregion

        #region AppMessageResources

        public string TitleText => ResourceService.AppMessages.GetString("AM_2FA_SetupDialogTitle");
        public string DescriptionText => ResourceService.AppMessages.GetString("AM_2FA_SetupDialogDescription");

        #endregion

        #region UiResources

        public string HowDoesItWorkText => ResourceService.UiResources.GetString("UI_HowDoesItWork");
        public string SetupTwoFactorAuthText => ResourceService.UiResources.GetString("UI_Setup2FA");
        
        #endregion
    }
}
