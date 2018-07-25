using System;
using System.Windows.Input;
using Windows.UI.Xaml;
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
            this.HowDoesItWorkCommand = new RelayCommand(HowDoesItWork);

            this.TitleText = ResourceService.AppMessages.GetString("AM_2FA_SetupDialogTitle");
            this.MessageText = ResourceService.AppMessages.GetString("AM_2FA_SetupDialogDescription");
            this.HowDoesItWorkLinkVisibility = Visibility.Visible;
        }

        #region Commands

        public ICommand SetupTwoFactorAuthCommand { get; }
        public ICommand HowDoesItWorkCommand { get; }

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

        private Visibility _howDoesItWorkLinkVisibility;
        /// <summary>
        /// Visibility of the "How does it work" link
        /// </summary>
        public Visibility HowDoesItWorkLinkVisibility
        {
            get { return _howDoesItWorkLinkVisibility; }
            set { SetField(ref _howDoesItWorkLinkVisibility, value); }
        }

        #endregion

        #region Methods

        private void SetupTwoFactorAuth()
        {
            this.DialogResult = true;
            this.OnHideDialog();
            NavigateService.Instance.Navigate(typeof(MultiFactorAuthAppSetupPage));
        }

        private void HowDoesItWork()
        {
            this.TitleText = ResourceService.AppMessages.GetString("AM_2FA_HowDoesItWorkTitle");
            this.MessageText = ResourceService.AppMessages.GetString("AM_2FA_HowDoesItWorkDescription");
            this.HowDoesItWorkLinkVisibility = Visibility.Collapsed;
        }

        #endregion

        #region UiResources

        public string HowDoesItWorkText => ResourceService.UiResources.GetString("UI_HowDoesItWork");
        public string SetupTwoFactorAuthText => ResourceService.UiResources.GetString("UI_Setup2FA");
        
        #endregion
    }
}
