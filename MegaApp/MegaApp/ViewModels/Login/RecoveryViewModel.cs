using System.Threading.Tasks;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Login
{
    public class RecoveryViewModel : BasePageViewModel
    {
        public RecoveryViewModel()
        {
            this.ControlState = true;
            this.SendCommand = new RelayCommand(Send);
        }

        #region Methods

        private async void Send()
        {
            SetWarning(false, string.Empty);
            this.EmailInputState = InputState.Normal;

            if (!NetworkService.IsNetworkAvailable(true)) return;

            if (!CheckInputParameters()) return;

            this.IsBusy = true;
            this.ControlState = false;
            this.SendButtonState = false;

            var reset = new ResetPasswordRequestListener();
            var resetted = await reset.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.resetPassword(this.Email, true, reset);
            });

            this.ControlState = true;
            this.SendButtonState = true;
            this.IsBusy = false;

            if (resetted)
                await DialogService.ShowAlertAsync(ResourceService.AppMessages.GetString("AM_CheckEmail_Title"),
                    ResourceService.AppMessages.GetString("AM_CheckEmail"));
            else
                await DialogService.ShowAlertAsync(ResourceService.AppMessages.GetString("AM_InvalidEmail_Title"),
                    ResourceService.AppMessages.GetString("AM_InvalidEmail"));
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

        public ICommand SendCommand { get; }

        #endregion

        #region Properties

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                SetField(ref _email, value);
                this.SendButtonState = !string.IsNullOrWhiteSpace(_email);
            }
        }

        private InputState _emailInputState;
        public InputState EmailInputState
        {
            get { return _emailInputState; }
            set { SetField(ref _emailInputState, value); }
        }

        private bool _sendButtonState;
        public bool SendButtonState
        {
            get { return _sendButtonState; }
            set { SetField(ref _sendButtonState, value); }
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

        public string SendText => ResourceService.UiResources.GetString("UI_Send");
        public string RecoveryHeaderText => ResourceService.UiResources.GetString("UI_RecoveryKeyHeader");
        public string RecoveryDescriptionText => ResourceService.UiResources.GetString("UI_RecoveryDescription");
        public string EmailWatermarkText => ResourceService.UiResources.GetString("UI_EmailWatermark");

        #endregion

        #region Visual Resources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }
}
