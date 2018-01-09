using System.Collections.Generic;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels.Login
{
    public class ConfirmParkAccountViewModel : BasePageViewModel
    {
        public ConfirmParkAccountViewModel()
        {
            this.ControlState = true;
            this.StartNewAccountCommand = new RelayCommand(StartNewAccount);
        }

        #region Methods

        private async void StartNewAccount()
        {
            SetWarning(false, string.Empty);
            this.PasswordInputState = InputState.Normal;
            this.ConfirmPasswordInputState = InputState.Normal;

            if (!NetworkService.IsNetworkAvailable(true)) return;

            if (!CheckInputParameters()) return;

            if (CheckPassword())
            {
                if (CheckPasswordStrenght())
                {
                    this.IsBusy = true;
                    this.ControlState = false;
                    this.StartNewAccountButtonState = false;

                    var reset = new ConfirmResetPasswordRequestListenerAsync();
                    var result = await reset.ExecuteAsync(() =>
                    {
                        SdkService.MegaSdk.confirmResetPasswordWithoutMasterKey(
                            LinkInformationService.ActiveLink,
                            this.Password,
                            reset);
                    });

                    this.ControlState = true;
                    this.StartNewAccountButtonState = true;
                    this.IsBusy = false;

                    if (result)
                    {
                        await DialogService.ShowAlertAsync(ResourceService.AppMessages.GetString("AM_AccountParked_Title"),
                            ResourceService.AppMessages.GetString("AM_AccountParked"));

                        NavigateService.Instance.Navigate(
                            typeof(LoginAndCreateAccountPage),
                            true,
                            new NavigationObject
                            {
                                Action = NavigationActionType.Recovery,
                                Parameters = new Dictionary<NavigationParamType, object>
                                {
                                    { NavigationParamType.Email, reset.EmailAddress },
                                    { NavigationParamType.Password, this.Password }
                                }
                            });

                    }
                    else
                    {
                        await DialogService.ShowAlertAsync(ResourceService.AppMessages.GetString("AM_PasswordResetFailed_Title"),
                            ResourceService.AppMessages.GetString("AM_PasswordResetFailed"));
                    }
                }
                else
                {
                    this.PasswordInputState = InputState.Warning;
                    this.ConfirmPasswordInputState = InputState.Warning;
                    SetWarning(true, ResourceService.AppMessages.GetString("AM_VeryWeakPassword"));
                }
            }
            else
            {
                this.PasswordInputState = InputState.Warning;
                this.ConfirmPasswordInputState = InputState.Warning;
                SetWarning(true, ResourceService.AppMessages.GetString("AM_PasswordsDoNotMatch"));
            }
        }

        private bool CheckInputParameters()
        {
            if (string.IsNullOrWhiteSpace(this.Password))
            {
                this.PasswordInputState = InputState.Warning;
                SetWarning(true, ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
                return false;
            }

            if (!string.IsNullOrWhiteSpace(this.ConfirmPassword)) return true;

            this.ConfirmPasswordInputState = InputState.Warning;
            SetWarning(true, ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
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


        private bool CheckPassword()
        {
            return this.Password.Equals(this.ConfirmPassword);
        }

        private void CalculatePasswordStrength(string value)
        {
            this.PasswordStrength = ValidationService.CalculatePasswordStrength(value);
        }

        private bool CheckPasswordStrenght()
        {
            return PasswordStrength != MPasswordStrength.PASSWORD_STRENGTH_VERYWEAK;
        }

        private void SetButtonState()
        {
            this.StartNewAccountButtonState = !string.IsNullOrWhiteSpace(this.Password) &&
                                              !string.IsNullOrWhiteSpace(this.ConfirmPassword);
        }

        #endregion

        #region Commands

        public ICommand StartNewAccountCommand { get; }

        #endregion

        #region Properties

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if (SetField(ref _password, value))
                    CalculatePasswordStrength(value);
                SetButtonState();
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                SetField(ref _confirmPassword, value);
                SetButtonState();
            }
        }

        private MPasswordStrength _passwordStrength;
        public MPasswordStrength PasswordStrength
        {
            get { return _passwordStrength; }
            set { SetField(ref _passwordStrength, value); }
        }
        
        private bool _startNewAccountButtonState;
        public bool StartNewAccountButtonState
        {
            get { return _startNewAccountButtonState; }
            set { SetField(ref _startNewAccountButtonState, value); }
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

        private InputState _passwordInputState;
        public InputState PasswordInputState
        {
            get { return _passwordInputState; }
            set { SetField(ref _passwordInputState, value); }
        }

        private InputState _confirmPasswordInputState;
        public InputState ConfirmPasswordInputState
        {
            get { return _confirmPasswordInputState; }
            set { SetField(ref _confirmPasswordInputState, value); }
        }

        #endregion

        #region UiResources

        public string StartNewAccountText => ResourceService.UiResources.GetString("UI_ValidatePassword");
        public string ParkAccountHeaderText => ResourceService.UiResources.GetString("UI_ParkAccount");
        public string ParkAccountDescriptionText => ResourceService.UiResources.GetString("UI_ParkAccountDescription");
        public string PasswordWatermarkText => ResourceService.UiResources.GetString("UI_PasswordWatermark");
        public string ConfirmPasswordWatermarkText => ResourceService.UiResources.GetString("UI_ConfirmPasswordWatermark");

        #endregion

        #region Visual Resources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }
}
