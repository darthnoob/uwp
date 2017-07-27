using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class ChangePasswordDialogViewModel : BaseViewModel
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

        #region Events

        /// <summary>
        /// Event triggered when the user changes the password successfully.
        /// </summary>
        public event EventHandler PasswordChanged;

        /// <summary>
        /// Event invocator method called when the user changes the password successfully.
        /// </summary>
        protected virtual void OnPasswordChanged()
        {
            this.CanClose = true;
            this.PasswordChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the user cancels the password change.
        /// </summary>
        public event EventHandler Canceled;

        /// <summary>
        /// Event invocator method called when the user cancels the password change.
        /// </summary>
        protected virtual void OnCanceled()
        {
            this.CanClose = true;
            this.Canceled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the current password is wrong.
        /// </summary>
        public event EventHandler CurrentPasswordError;

        /// <summary>
        /// Event invocator method called when the current password is wrong.
        /// </summary>
        protected virtual void OnCurrentPasswordError()
        {
            this.CurrentPasswordError?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the new password is wrong.
        /// </summary>
        public event EventHandler NewPasswordError;

        /// <summary>
        /// Event invocator method called when the new password is wrong.
        /// </summary>
        protected virtual void OnNewPasswordError()
        {
            this.NewPasswordError?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the confirmation of the new password is wrong.
        /// </summary>
        public event EventHandler NewPasswordConfirmationError;

        /// <summary>
        /// Event invocator method called when the confirmation of the new password is wrong.
        /// </summary>
        protected virtual void OnNewPasswordConfirmationError()
        {
            this.NewPasswordConfirmationError?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Changes the password
        /// </summary>
        private async void Save()
        {
            // If there are empty fields
            if (string.IsNullOrWhiteSpace(this.CurrentPassword) || string.IsNullOrWhiteSpace(this.NewPassword) ||
                string.IsNullOrWhiteSpace(this.NewPasswordConfirmation))
            {
                if (string.IsNullOrWhiteSpace(this.CurrentPassword))
                    OnCurrentPasswordError();

                if (string.IsNullOrWhiteSpace(this.NewPassword))
                    OnNewPasswordError();

                if (string.IsNullOrWhiteSpace(this.NewPasswordConfirmation))
                    OnNewPasswordConfirmationError();

                this.ErrorMessage = ResourceService.AppMessages.GetString("AM_EmptyRequiredFields");
                return;
            }

            // If the new password and the old password are the same, 
            // or the new password and the confirmation password don't match
            if (this.CurrentPassword.Equals(this.NewPassword) || !this.NewPassword.Equals(this.NewPasswordConfirmation))
            {
                if (this.CurrentPassword.Equals(this.NewPassword))
                    this.ErrorMessage = ResourceService.AppMessages.GetString("AM_NewAndOldPasswordMatch");
                else if (!this.NewPassword.Equals(this.NewPasswordConfirmation))
                    this.ErrorMessage = ResourceService.AppMessages.GetString("AM_PasswordsDoNotMatch");

                OnNewPasswordError();
                OnNewPasswordConfirmationError();
                return;
            }

            var changePassword = new ChangePasswordRequestListenerAsync();
            var result = await changePassword.ExecuteAsync(() =>
                SdkService.MegaSdk.changePassword(this.CurrentPassword, this.NewPassword, changePassword));

            // If something went wrong, probably the current password is wrong
            if (!result)
            {
                this.ErrorMessage = ResourceService.AppMessages.GetString("AM_WrongPassword");
                OnCurrentPasswordError();
                return;
            }

            OnPasswordChanged();

            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_PasswordChanged_Title"),
                ResourceService.AppMessages.GetString("AM_PasswordChanged"));
        }

        /// <summary>
        /// Cancels the change password process
        /// </summary>
        private void Cancel()
        {
            OnCanceled();
        }

        #endregion

        #region Properties

        public bool CanClose = false;

        private string _currentPassword;
        public string CurrentPassword
        {
            get { return _currentPassword; }
            set { SetField(ref _currentPassword, value); }
        }

        private string _newPassword;
        public string NewPassword
        {
            get { return _newPassword; }
            set { SetField(ref _newPassword, value); }
        }

        private string _newPasswordConfirmation;
        public string NewPasswordConfirmation
        {
            get { return _newPasswordConfirmation; }
            set { SetField(ref _newPasswordConfirmation, value); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetField(ref _errorMessage, value); }
        }

        #endregion

        #region UiResources

        public string TitleText => ResourceService.UiResources.GetString("UI_ChangePassword");
        public string DescriptionText => ResourceService.UiResources.GetString("UI_ChangePasswordDescription");
        public string CurrentPasswordText => ResourceService.UiResources.GetString("UI_CurrentPassword");
        public string NewPasswordText => ResourceService.UiResources.GetString("UI_NewPassword");
        public string ReEnterNewPasswordText => ResourceService.UiResources.GetString("UI_ReEnterNewPassword");
        public string SaveText => ResourceService.UiResources.GetString("UI_Save");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");

        #endregion
    }
}
