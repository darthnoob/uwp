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
    public class BaseChangeEmailDialog : ContentDialogEx<ChangeEmailDialogViewModel> { }

    public sealed partial class ChangeEmailDialog : BaseChangeEmailDialog
    {
        public ChangeEmailDialog()
        {
            this.InitializeComponent();

            this.DefaultBoxBorderBrush = this.NewEmailTextBox.BorderBrush;

            this.ViewModel.Saved += OnSaved;
            this.ViewModel.Canceled += OnCanceled;
            this.ViewModel.EmailError += OnEmailError;
        }

        #region Properties

        private Brush DefaultBoxBorderBrush;

        public bool DialogResult;

        public string NewEmail => ViewModel.NewEmail;

        #endregion

        #region Private Methods

        private void OnSaved(object sender, EventArgs e)
        {
            this.DialogResult = true;
            this.Hide();
        }

        private void OnCanceled(object sender, EventArgs e)
        {
            this.DialogResult = false;
            this.Hide();
        }

        private void OnEmailError(object sender, EventArgs e)
        {
            this.NewEmailTextBox.BorderBrush = (Brush)Application.Current.Resources["MegaRedColorBrush"];
        }

        private void OnNewEmailChanged(object sender, RoutedEventArgs e)
        {
            this.NewEmailTextBox.BorderBrush = DefaultBoxBorderBrush;
            this.ErrorMessage.Text = string.Empty;
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!this.ViewModel.CanClose)
                args.Cancel = true;
        }

        #endregion
    }
}
