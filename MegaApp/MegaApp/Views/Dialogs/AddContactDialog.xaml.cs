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
    public class BaseAddContactDialog : ContentDialogEx<AddContactDialogViewModel> { }

    public sealed partial class AddContactDialog : BaseAddContactDialog
    {
        public AddContactDialog()
        {
            this.InitializeComponent();

            this.defaultBoxBorderBrush = this.ContactEmailTextBox.BorderBrush;
        }

        #region Properties

        private Brush defaultBoxBorderBrush;

        public bool DialogResult;

        public string ContactEmail => ViewModel.ContactEmail;
        public string EmailContent => ViewModel.EmailContent;

        #endregion

        #region Private Methods

        private void OnContactInvited(object sender, EventArgs e)
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
            this.ContactEmailTextBox.BorderBrush = (Brush)Application.Current.Resources["MegaRedColorBrush"];
        }

        private void OnContactEmailChanged(object sender, TextChangedEventArgs e)
        {
            this.ContactEmailTextBox.BorderBrush = defaultBoxBorderBrush;
            this.ErrorMessage.Text = string.Empty;
        }

        private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.ViewModel.ContactInvited += OnContactInvited;
            this.ViewModel.Canceled += OnCanceled;
            this.ViewModel.EmailError += OnEmailError;
        }

        private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            this.ViewModel.ContactInvited -= OnContactInvited;
            this.ViewModel.Canceled -= OnCanceled;
            this.ViewModel.EmailError -= OnEmailError;
        }

        #endregion
    }
}
