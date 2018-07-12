using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class ChangePasswordDialogViewModel : BaseContentDialogViewModel
    {
        public ChangePasswordDialogViewModel()
        {
            this.SaveButtonCommand = new RelayCommand(Save);
            this.CancelButtonCommand = new RelayCommand(Cancel);
        }

        #region Commands

        public ICommand SaveButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Cancels the change password process
        /// </summary>
        private void Cancel() => this.OnHideDialog();

        /// <summary>
        /// Changes the password
        /// </summary>
        private async void Save()
        {
            SetWarning(false, string.Empty);
            SetInputState();
            
            if (!this.CheckInputParameters() || !this.CheckNewPassword()) return;
            
            this.IsBusy = true;
            this.ControlState = false;
            this.SaveButtonState = false;

            ChangePasswordResult result = ChangePasswordResult.Unknown;
            var changePassword = new ChangePasswordRequestListenerAsync();

            var mfaStatus = await AccountService.CheckMultiFactorAuthStatusAsync();
            if (mfaStatus == MultiFactorAuthStatus.Enabled)
            {
                this.OnHideDialog();
                await DialogService.ShowAsyncMultiFactorAuthCodeInputDialogAsync(async (string code) =>
                {
                    result = await changePassword.ExecuteAsync(() =>
                    {
                        SdkService.MegaSdk.multiFactorAuthChangePasswordWithoutOld(
                            this.NewPassword, code, changePassword);
                    });

                    if (result == ChangePasswordResult.MultiFactorAuthInvalidCode)
                    {
                        DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();
                        return false;
                    }

                    return true;
                });

                this.OnShowDialog();
            }
            else
            {
                result = await changePassword.ExecuteAsync(() =>
                SdkService.MegaSdk.changePasswordWithoutOld(this.NewPassword, changePassword));
            }

            this.IsBusy = false;
            this.ControlState = true;
            this.SaveButtonState = true;

            if (result != ChangePasswordResult.Success)
            {
                SetWarning(true, ResourceService.AppMessages.GetString("AM_PasswordChangeFailed"));                
                return;
            }

            OnHideDialog();

            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_PasswordChanged_Title"),
                ResourceService.AppMessages.GetString("AM_PasswordChanged"));
        }

        /// <summary>
        /// Checks that the user has introduced all the required input parameters.
        /// </summary>
        /// <returns>TRUE if is all right or FALSE in other case.</returns>
        private bool CheckInputParameters()
        {
            // If there are empty fields
            if (string.IsNullOrWhiteSpace(this.NewPassword) || string.IsNullOrWhiteSpace(this.ConfirmPassword))
            {
                this.SetWarning(true, ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
                SetInputState(
                    newPassword: string.IsNullOrWhiteSpace(this.NewPassword) ?
                        InputState.Warning : InputState.Normal,
                    confirmPassword: string.IsNullOrWhiteSpace(this.ConfirmPassword) ?
                        InputState.Warning : InputState.Normal);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks that the new password is valid. New password requisites:
        /// 1.- New password and confirm password should be equal.
        /// 2.- The password strength should have a minimum value.
        /// </summary>
        /// <returns>TRUE if is all right or FALSE in other case.</returns>
        private bool CheckNewPassword()
        {
            // If the new password and the confirmation password don't match
            if (!this.NewPassword.Equals(this.ConfirmPassword))
            {
                this.SetWarning(true, ResourceService.AppMessages.GetString("AM_PasswordsDoNotMatch"));
                SetInputState(newPassword: InputState.Warning, confirmPassword: InputState.Warning);
                return false;
            }

            if (!this.CheckPasswordStrenght())
            {
                SetInputState(newPassword: InputState.Warning, confirmPassword: InputState.Warning);
                SetWarning(true, ResourceService.AppMessages.GetString("AM_VeryWeakPassword"));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks the new password strenght.
        /// </summary>
        /// <returns>TRUE if is all right or FALSE in other case.</returns>
        private bool CheckPasswordStrenght()
        {
            return PasswordStrength != MPasswordStrength.PASSWORD_STRENGTH_VERYWEAK;
        }

        /// <summary>
        /// Calculate the password strenght.
        /// </summary>
        /// <param name="value">Password string</param>
        private void CalculatePasswordStrength(string value)
        {
            this.PasswordStrength = ValidationService.CalculatePasswordStrength(value);
        }

        /// <summary>
        /// Sets the input fields state.
        /// </summary>
        /// <param name="newPassword">State of the new password field.</param>
        /// <param name="confirmPassword">State of the confirm password field.</param>
        private void SetInputState(
            InputState newPassword = InputState.Normal,
            InputState confirmPassword = InputState.Normal)
        {
            OnUiThread(() =>
            {
                this.NewPasswordInputState = newPassword;
                this.ConfirmPasswordInputState = confirmPassword;
            });
        }

        /// <summary>
        /// Sets the save button state.
        /// </summary>
        private void SetButtonState()
        {
            var enabled = !string.IsNullOrWhiteSpace(this.NewPassword) &&
                          !string.IsNullOrWhiteSpace(this.ConfirmPassword);

            OnUiThread(() => this.SaveButtonState = enabled);
        }

        /// <summary>
        /// Set the warning text status.
        /// </summary>
        /// <param name="isVisible">Set if is visible or not.</param>
        /// <param name="warningText">Warning text to display.</param>
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

        #region Properties

        private string _newPassword;
        public string NewPassword
        {
            get { return _newPassword; }
            set
            {
                if(SetField(ref _newPassword, value))
                    CalculatePasswordStrength(value);
                SetButtonState();
            }
        }

        private MPasswordStrength _passwordStrength;
        public MPasswordStrength PasswordStrength
        {
            get { return _passwordStrength; }
            set { SetField(ref _passwordStrength, value); }
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

        private bool _saveButtonState;
        public bool SaveButtonState
        {
            get { return _saveButtonState; }
            set { SetField(ref _saveButtonState, value); }
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

        private InputState _newPasswordInputState;
        public InputState NewPasswordInputState
        {
            get { return _newPasswordInputState; }
            set { SetField(ref _newPasswordInputState, value); }
        }

        private InputState _confirmPasswordInputState;
        public InputState ConfirmPasswordInputState
        {
            get { return _confirmPasswordInputState; }
            set { SetField(ref _confirmPasswordInputState, value); }
        }

        #endregion

        #region UiResources

        public string TitleText => ResourceService.UiResources.GetString("UI_ChangePassword");
        public string DescriptionText => ResourceService.UiResources.GetString("UI_ChangePasswordDescription");
        public string NewPasswordText => ResourceService.UiResources.GetString("UI_NewPassword");
        public string ReEnterNewPasswordText => ResourceService.UiResources.GetString("UI_ReEnterNewPassword");
        public string SaveText => ResourceService.UiResources.GetString("UI_Save");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");

        #endregion
    }
}
