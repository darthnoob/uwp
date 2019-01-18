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
    public class ConfirmRecoveryViewModel : BasePageViewModel
    {
        public ConfirmRecoveryViewModel()
        {
            this.ControlState = true;
            this.ValidatePasswordCommand = new RelayCommand(ValidatePassword);
        }

        #region Methods

        private async void ValidatePassword()
        {
            SetWarning(false, string.Empty);
            this.PasswordInputState = InputState.Normal;
            this.ConfirmPasswordInputState = InputState.Normal;

            if (!NetworkService.HasInternetAccess(true)) return;

            if (!CheckInputParameters()) return;

            if (CheckPassword())
            {
                if (CheckPasswordStrenght())
                {
                    this.IsBusy = true;
                    this.ControlState = false;
                    this.ValidateButtonState = false;

                    var reset = new ConfirmResetPasswordRequestListenerAsync();
                    var result = await reset.ExecuteAsync(() =>
                    {
                        SdkService.MegaSdk.confirmResetPassword(
                            LinkInformationService.ActiveLink,
                            this.Password,
                            RecoveryKey,
                            reset);
                    });

                    this.ControlState = true;
                    this.ValidateButtonState = true;
                    this.IsBusy = false;

                    if (result)
                    {
                        await DialogService.ShowAlertAsync(ResourceService.AppMessages.GetString("AM_PasswordResetSuccess_Title"),
                            ResourceService.AppMessages.GetString("AM_PasswordResetSuccess"));

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

        private bool CheckPasswordStrenght()
        {
            return PasswordStrength != MPasswordStrength.PASSWORD_STRENGTH_VERYWEAK;
        }

        private void CalculatePasswordStrength(string value)
        {
            this.PasswordStrength = ValidationService.CalculatePasswordStrength(value);
        }

        private void SetButtonState()
        {
            this.ValidateButtonState = !string.IsNullOrWhiteSpace(this.Password) &&
                                       !string.IsNullOrWhiteSpace(this.ConfirmPassword);
        }

        #endregion

        #region Commands

        public ICommand ValidatePasswordCommand { get; }

        #endregion

        #region Properties

        public string RecoveryKey { get; set; }

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
        
        private bool _validateButtonState;
        public bool ValidateButtonState
        {
            get { return _validateButtonState; }
            set { SetField(ref _validateButtonState, value); }
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

        public string ValidatePasswordText => ResourceService.UiResources.GetString("UI_ValidatePassword");
        public string ValidatePasswordtHeaderText => ResourceService.UiResources.GetString("UI_ValidatePasswordHeader");
        public string ValidatePasswordDescriptionText => ResourceService.UiResources.GetString("UI_ValidatePasswordDescription");
        public string PasswordWatermarkText => ResourceService.UiResources.GetString("UI_PasswordWatermark");
        public string ConfirmPasswordWatermarkText => ResourceService.UiResources.GetString("UI_ConfirmPasswordWatermark");

        #endregion

        #region Visual Resources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }
}
