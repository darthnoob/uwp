using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.CreateAccount
{
    public class ConfirmEmailViewModel : BasePageViewModel
    {
        public ConfirmEmailViewModel()
        {
            this.ControlState = true;
            this.ResendCommand = new RelayCommand(Resend);
        }

        #region Methods

        private async void Resend()
        {
            SetWarning(false, string.Empty);
            this.EmailInputState = InputState.Normal;

            if (!NetworkService.HasInternetAccess(true)) return;

            if (!CheckInputParameters()) return;

            this.IsBusy = true;
            this.ControlState = false;
            this.ResendButtonState = false;

            var create = new CreateAccountRequestListenerAsync();
            var result = await create.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.createAccount(
                    this.Email, this.Password, this.FirstName, this.LastName, create);
            });

            this.ControlState = true;
            this.ResendButtonState = true;
            this.IsBusy = false;

            string messageContent;
            switch (result)
            {
                case CreateAccountResult.Success:
                    {
                        messageContent = string.Format(ResourceService.AppMessages.GetString("AM_ConfirmEmail"), this.Email);
                        break;
                    }
                case CreateAccountResult.AlreadyExists:
                    {
                        messageContent = ResourceService.AppMessages.GetString("AM_EmailAlreadyRegistered");
                        break;
                    }
                case CreateAccountResult.Unknown:
                    {
                        messageContent = ResourceService.AppMessages.GetString("AM_CreateAccountFailed");
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_ConfirmEmail_Title"),
                messageContent);
        }

        private bool CheckInputParameters()
        {
            if (string.IsNullOrWhiteSpace(this.Email))
            {
                SetWarning(true, ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
                EmailInputState = InputState.Warning;
                return false;
            }

            if (ValidationService.IsValidEmail(this.Email)) return true;

            SetWarning(true, ResourceService.AppMessages.GetString("AM_IncorrectEmailFormat"));
            EmailInputState = InputState.Warning;
            return false;
        }

        private void SetWarning(bool isVisible, string warningText)
        {
            if (isVisible)
            {
                // First text and then display
                this.WarningText = warningText;
                this.IsWarningVisible = true;
            }
            else
            {
                // First remove and than clean text
                this.IsWarningVisible = false;
                this.WarningText = warningText;
            }
        }

        #endregion

        #region Commands

        public ICommand ResendCommand { get; }

        #endregion

        #region Properties

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                SetField(ref _email, value);
                this.ResendButtonState = !string.IsNullOrWhiteSpace(_email);
            }
        }

        public string Password { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        private InputState _emailInputState;
        public InputState EmailInputState
        {
            get { return _emailInputState; }
            set { SetField(ref _emailInputState, value); }
        }

        private bool _resendButtonState;
        public bool ResendButtonState
        {
            get { return _resendButtonState; }
            set { SetField(ref _resendButtonState, value); }
        }

        private string _warningText;
        public string WarningText
        {
            get { return _warningText; }
            set { SetField(ref _warningText, value); }
        }

        private bool _isWarningVisible;
        public bool IsWarningVisible
        {
            get { return _isWarningVisible; }
            set { SetField(ref _isWarningVisible, value); }
        }

        #endregion

        #region UiResources

        public string ResendText => ResourceService.UiResources.GetString("UI_Resend");
        public string ConfirmEmailHeaderText => ResourceService.UiResources.GetString("UI_ConfirmEmail");
        public string ConfirmEmailDescriptionText => ResourceService.UiResources.GetString("UI_ConfirmEmailDescription");
        public string ConfirmEmailInfoText => ResourceService.UiResources.GetString("UI_ConfirmEmailInfo");
        public string EmailWatermarkText => ResourceService.UiResources.GetString("UI_EmailWatermark");

        #endregion

        #region Visual Resources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }
}
