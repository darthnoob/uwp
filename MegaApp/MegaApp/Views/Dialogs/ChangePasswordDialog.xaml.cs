using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseChangePasswordDialog : ContentDialogEx<ChangePasswordDialogViewModel> { }

    public sealed partial class ChangePasswordDialog : BaseChangePasswordDialog
    {
        public ChangePasswordDialog()
        {
            this.InitializeComponent();

            this.DefaultBoxBorderBrush = this.CurrentPasswordBox.BorderBrush;

            this.ViewModel.PasswordChanged += OnPasswordChanged;
            this.ViewModel.Canceled += OnCanceled;
            this.ViewModel.CurrentPasswordError += OnCurrentPasswordError;
            this.ViewModel.NewPasswordError += OnNewPasswordError;
            this.ViewModel.NewPasswordConfirmationError += OnNewPasswordConfirmationError;
        }

        #region Properties

        private readonly Brush DefaultBoxBorderBrush;

        #endregion

        #region Private Methods

        private void OnPasswordChanged(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void OnCanceled(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void OnCurrentPasswordError(object sender, EventArgs e)
        {
            this.CurrentPasswordBox.BorderBrush = (Brush)Application.Current.Resources["MegaRedColorBrush"];
        }

        private void OnNewPasswordError(object sender, EventArgs e)
        {
            this.NewPasswordBox.BorderBrush = (Brush)Application.Current.Resources["MegaRedColorBrush"];
        }

        private void OnNewPasswordConfirmationError(object sender, EventArgs e)
        {
            this.ConfirmNewPasswordBox.BorderBrush = (Brush)Application.Current.Resources["MegaRedColorBrush"];
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

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!this.ViewModel.CanClose)
                args.Cancel = true;
        }

        #endregion        
    }
}
