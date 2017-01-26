using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class ConfirmAccountViewModel : BaseSdkViewModel
    {
        public ConfirmAccountViewModel()
        {
            this.ControlState = true;
            this.ConfirmAccountCommand = new RelayCommand<object>(this.ConfirmAccount);
        }

        #region Methods

        private void ConfirmAccount(object obj)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            if (String.IsNullOrEmpty(ConfirmLink))
                return;
            else
            {
                if (String.IsNullOrEmpty(Password))
                {
                    new CustomMessageDialog(
                        ConfirmAccountText,
                        ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                }
                else
                {
                    SdkService.MegaSdk.confirmAccount(ConfirmLink, Password,
                        new ConfirmAccountRequestListener(this));
                }
            }
        }

        #endregion

        #region Commands

        public ICommand ConfirmAccountCommand { get; set; }

        #endregion

        #region Properties

        public string ConfirmLink { get; set; }
        public string Password { get; set; }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { SetField(ref _email, value); }
        }

        #endregion

        #region UiResources

        public string EmailWatermarkText => ResourceService.UiResources.GetString("UI_EmailWatermark");
        public string ConfirmAccountText => ResourceService.UiResources.GetString("UI_ConfirmAccount");
        public string ConfirmYourAccountTitleText => ResourceService.UiResources.GetString("UI_ConfirmYourAccountTitle");
        public string ConfirmYourAccountText => ResourceService.UiResources.GetString("UI_ConfirmYourAccount");
        public string PasswordWatermarkText => ResourceService.UiResources.GetString("UI_PasswordWatermark");

        #endregion

        #region VisualResources

        public string MegaIconPathData { get { return ResourceService.VisualResources.GetString("VR_MegaIconPathData"); } }

        #endregion
    }
}
