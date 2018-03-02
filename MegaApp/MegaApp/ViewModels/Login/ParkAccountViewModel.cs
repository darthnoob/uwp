using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Login
{
    public class ParkAccountViewModel : BasePageViewModel
    {
        public ParkAccountViewModel()
        {
            this.ControlState = true;
            this.ParkAccountCommand = new RelayCommand(ParkAccount);
        }

        #region Methods

        private async void ParkAccount()
        {
            SetWarning(false, string.Empty);
            this.EmailInputState = InputState.Normal;

            if (!await NetworkService.IsNetworkAvailableAsync(true)) return;

            if (!CheckInputParameters()) return;

            this.IsBusy = true;
            this.ControlState = false;
            this.ParkButtonState = false;

            var reset = new ResetPasswordRequestListener();
            var resetted = await reset.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.resetPassword(this.Email, false, reset);
            });

            this.ControlState = true;
            this.ParkButtonState = true;
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

        public ICommand ParkAccountCommand { get; }

        #endregion

        #region Properties

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                SetField(ref _email, value);
                this.ParkButtonState = !string.IsNullOrWhiteSpace(_email);
            }
        }

        private InputState _emailInputState;
        public InputState EmailInputState
        {
            get { return _emailInputState; }
            set { SetField(ref _emailInputState, value); }
        }

        private bool _parkButtonState;
        public bool ParkButtonState
        {
            get { return _parkButtonState; }
            set { SetField(ref _parkButtonState, value); }
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

        public string ParkAccountText => ResourceService.UiResources.GetString("UI_ParkAccount");
        public string ParkAccountHeaderText => ResourceService.UiResources.GetString("UI_NoRecoveryKeyHeader");
        public string ParkAccountDescriptionText => UiService.ConcatStringsToParagraph(
            new[]
            {
                ResourceService.UiResources.GetString("UI_NoRecoveryKeyDescription_Part_1"),
                ResourceService.UiResources.GetString("UI_NoRecoveryKeyDescription_Part_2")
            });
       
        public string EmailWatermarkText => ResourceService.UiResources.GetString("UI_EmailWatermark");

        #endregion

        #region Visual Resources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }
}
