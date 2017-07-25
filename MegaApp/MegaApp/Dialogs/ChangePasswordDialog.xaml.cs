using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.Dialogs
{
    public sealed partial class ChangePasswordDialog : ContentDialog
    {
        public ChangePasswordDialog()
        {
            this.InitializeComponent();

            this.DefaultBoxBorderBrush = this.CurrentPasswordBox.BorderBrush;

            this.Title = ResourceService.UiResources.GetString("UI_ChangePassword");
            this.Description.Text = ResourceService.UiResources.GetString("UI_ChangePasswordDescription");
            this.CurrentPasswordBox.PlaceholderText = ResourceService.UiResources.GetString("UI_CurrentPassword");
            this.NewPasswordBox.PlaceholderText = ResourceService.UiResources.GetString("UI_NewPassword");
            this.ConfirmNewPasswordBox.PlaceholderText = ResourceService.UiResources.GetString("UI_ReEnterNewPassword");

            this.PrimaryButtonText = ResourceService.UiResources.GetString("UI_Save");
            this.SecondaryButtonText = ResourceService.UiResources.GetString("UI_Cancel");

            this.ErrorMessage.Text = string.Empty;
        }

        #region Properties

        private readonly Brush DefaultBoxBorderBrush;

        private string CurrentPassword => this.CurrentPasswordBox.Password;
        private string NewPassword => this.NewPasswordBox.Password;
        private string NewPasswordConfirmation => this.ConfirmNewPasswordBox.Password;

        #endregion

        #region Private Methods

        private async void SaveButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;

            if (string.IsNullOrWhiteSpace(this.CurrentPassword) || string.IsNullOrWhiteSpace(this.NewPassword) ||
                string.IsNullOrWhiteSpace(this.NewPasswordConfirmation))
            {
                if (string.IsNullOrWhiteSpace(this.CurrentPassword))
                    this.CurrentPasswordBox.BorderBrush = (Brush)Application.Current.Resources["MegaRedColorBrush"];

                if (string.IsNullOrWhiteSpace(this.NewPassword))
                    this.NewPasswordBox.BorderBrush = (Brush)Application.Current.Resources["MegaRedColorBrush"];

                if (string.IsNullOrWhiteSpace(this.NewPasswordConfirmation))
                    this.ConfirmNewPasswordBox.BorderBrush = (Brush)Application.Current.Resources["MegaRedColorBrush"];

                this.ErrorMessage.Text = ResourceService.AppMessages.GetString("AM_EmptyRequiredFields");
                return;
            }

            if (this.CurrentPassword.Equals(this.NewPassword) || !this.NewPassword.Equals(this.NewPasswordConfirmation))
            {
                if (this.CurrentPassword.Equals(this.NewPassword))
                    this.ErrorMessage.Text = ResourceService.AppMessages.GetString("AM_NewAndOldPasswordMatch");
                else if (!this.NewPassword.Equals(this.NewPasswordConfirmation))
                    this.ErrorMessage.Text = ResourceService.AppMessages.GetString("AM_PasswordsDoNotMatch");

                this.NewPasswordBox.BorderBrush = this.ConfirmNewPasswordBox.BorderBrush =
                    (Brush)Application.Current.Resources["MegaRedColorBrush"];
                return;
            }

            var changePassword = new ChangePasswordRequestListenerAsync();
            var result = await changePassword.ExecuteAsync(() =>
                SdkService.MegaSdk.changePassword(this.CurrentPassword, this.NewPassword, changePassword));

            if (!result)
            {
                this.ErrorMessage.Text = ResourceService.AppMessages.GetString("AM_WrongPassword");
                this.CurrentPasswordBox.BorderBrush = (Brush)Application.Current.Resources["MegaRedColorBrush"];
                return;
            }

            this.Hide();

            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_PasswordChanged_Title"),
                ResourceService.AppMessages.GetString("AM_PasswordChanged"));
        }

        private void CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private void OnCurrentPasswordChanged(object sender, RoutedEventArgs e)
        {
            this.CurrentPasswordBox.BorderBrush = DefaultBoxBorderBrush;
        }

        private void OnNewPasswordChanged(object sender, RoutedEventArgs e)
        {
            this.NewPasswordBox.BorderBrush = DefaultBoxBorderBrush;
        }

        private void OnConfirmNewPasswordChanged(object sender, RoutedEventArgs e)
        {
            this.ConfirmNewPasswordBox.BorderBrush = DefaultBoxBorderBrush;
        }

        #endregion
    }
}
