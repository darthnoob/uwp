using System.IO;
using Windows.UI.Xaml;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseInputDialog : ContentDialogEx<InputDialogViewModel> { }

    public sealed partial class InputDialog : BaseInputDialog
    {
        /// <summary>
        /// Creates an standard input dialog
        /// </summary>
        /// <param name="title">Title of the input dialog</param>
        /// <param name="message">Message of the input dialog</param>
        /// <param name="primaryButton">Label of the primary button of the input dialog</param>
        /// <param name="secondaryButton">Label of the secondary button of the input dialog</param>
        /// <param name="settings">Input dialog behavior/option settings</param>
        public InputDialog(string title, string message,
            string primaryButton = null, string secondaryButton = null,
            InputDialogSettings settings = null)
        {
            this.InitializeComponent();

            this.ViewModel.TitleText = title;
            this.ViewModel.MessageText = message;
            this.ViewModel.PrimaryButtonLabel = primaryButton ?? ViewModel.OkText;
            this.ViewModel.SecondaryButtonLabel = secondaryButton ?? ViewModel.CancelText;

            // Create default input settings if null
            this.ViewModel.Settings = settings ?? new InputDialogSettings();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.InputTextBox.Text = this.ViewModel.Settings.InputText;

            if (!this.ViewModel.Settings.IsTextSelected) return;

            this.InputTextBox.SelectionStart = 0;
            if (this.ViewModel.Settings.IgnoreExtensionInSelection)
            {
                var fileName = Path.GetFileNameWithoutExtension(this.ViewModel.Settings.InputText);
                this.InputTextBox.SelectionLength = fileName.Length;
                return;
            }

            this.InputTextBox.SelectionLength = this.ViewModel.Settings.InputText.Length;
        }
    }
}
