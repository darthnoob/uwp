using System;
using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels.Dialogs;
using MegaApp.UserControls;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseOkCancelAndWarningDialog : ContentDialogEx<OkCancelAndWarningDialogViewModel> { }

    public sealed partial class OkCancelAndWarningDialog : BaseOkCancelAndWarningDialog
    {
        public OkCancelAndWarningDialog(string title, string message, 
            string warning, string okButton, string cancelButton)
        {
            this.InitializeComponent();

            this.ViewModel.TitleText = title;
            this.ViewModel.MessageText = message;
            this.ViewModel.WarningText = warning;
            this.ViewModel.OkButtonLabelText = okButton;
            this.ViewModel.CancelButtonLabelText = cancelButton;
        }

        #region Properties

        public bool DialogResult;

        #endregion

        #region Private Methods

        private void OnOkButtonTapped(object sender, EventArgs e)
        {
            this.DialogResult = true;
            this.Hide();
        }

        private void OnCancelButtonTapped(object sender, EventArgs e)
        {
            this.DialogResult = false;
            this.Hide();
        }

        private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.ViewModel.OkButtonTapped += OnOkButtonTapped;
            this.ViewModel.CancelButtonTapped += OnCancelButtonTapped;
        }

        private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            this.ViewModel.OkButtonTapped -= OnOkButtonTapped;
            this.ViewModel.CancelButtonTapped -= OnCancelButtonTapped;
        }

        #endregion
    }
}
