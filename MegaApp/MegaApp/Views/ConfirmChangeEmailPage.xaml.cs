using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseConfirmChangeEmailPage : PageEx<ConfirmChangeEmailViewModel> { }

    public sealed partial class ConfirmChangeEmailPage : BaseConfirmChangeEmailPage
    {
        public ConfirmChangeEmailPage()
        {
            this.InitializeComponent();

            this.DefaultBoxBorderBrush = this.PasswordBox.BorderBrush;

            this.ViewModel.HeaderText = ResourceService.UiResources.GetString("UI_VerifyEmailHeader");
            this.ViewModel.SubHeaderText = ResourceService.UiResources.GetString("UI_VerifyEmailSubHeader");

            this.EmailChangedText.Visibility = Visibility.Collapsed;
            this.OkButton.Visibility = Visibility.Collapsed;

            this.ViewModel.EmailChanged += OnEmailChanged;
            this.ViewModel.PasswordError += OnPasswordError;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DialogService.CloseAwaitEmailConfirmationDialog();

            this.ViewModel.ProcessVerifyEmailLink();
        }

        #region Properties

        private readonly Brush DefaultBoxBorderBrush;

        #endregion

        #region Private Methods

        private void OnEmailChanged(object sender, EventArgs e)
        {
            this.ViewModel.HeaderText = ResourceService.ProgressMessages.GetString("PM_LoginHeader");
            this.ViewModel.SubHeaderText = ResourceService.UiResources.GetString("UI_EmailChangedSuccessful");

            this.PasswordBox.Visibility = Visibility.Collapsed;
            this.ConfirmEmailButton.Visibility = Visibility.Collapsed;

            this.EmailChangedText.Visibility = Visibility.Visible;
            this.OkButton.Visibility = Visibility.Visible;
        }

        private void OnPasswordError(object sender, EventArgs e)
        {
            this.PasswordBox.BorderBrush = (Brush)Application.Current.Resources["MegaRedColorBrush"];
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            this.PasswordBox.BorderBrush = DefaultBoxBorderBrush;
            this.ViewModel.ErrorMessage = string.Empty;
        }

        #endregion
    }
}
