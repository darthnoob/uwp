using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using mega;
using MegaApp.UserControls;
using MegaApp.Services;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseMultiFactorAuthCodeInputDialog : ContentDialogEx<MultiFactorAuthCodeInputDialogViewModel> { }

    public sealed partial class MultiFactorAuthCodeInputDialog : BaseMultiFactorAuthCodeInputDialog
    {
        /// <summary>
        /// Creates an input dialog to type the MFA 6-digit code and also executes an action.
        /// </summary>
        /// <param name="dialogAction">Action to execute by the primary button.</param>
        /// <param name="title">Custom title of the input dialog.</param>
        /// <param name="message">Custom message of the input dialog.</param>
        public MultiFactorAuthCodeInputDialog(Func<string, bool> dialogAction,
            string title = null, string message = null)
        {
            this.InitializeComponent();

            this.ViewModel.DialogAction = dialogAction;
            this.ViewModel.TitleText = title ?? ResourceService.UiResources.GetString("UI_TwoFactorAuth");
            this.ViewModel.MessageText = message ?? ResourceService.AppMessages.GetString("AM_2FA_InputAppCodeDialogMessage");
        }

        /// <summary>
        /// Creates an input dialog to type the MFA 6-digit code and also executes an action.
        /// </summary>
        /// <param name="dialogActionAsync">Async action to execute by the primary button.</param>
        /// <param name="title">Custom title of the input dialog.</param>
        /// <param name="message">Custom message of the input dialog.</param>
        public MultiFactorAuthCodeInputDialog(Func<string, Task<bool>> dialogActionAsync,
            string title = null, string message = null)
        {
            this.InitializeComponent();

            this.ViewModel.DialogActionAsync = dialogActionAsync;
            this.ViewModel.TitleText = title ?? ResourceService.UiResources.GetString("UI_TwoFactorAuth");
            this.ViewModel.MessageText = message ?? ResourceService.AppMessages.GetString("AM_2FA_InputAppCodeDialogMessage");
        }

        #region Methods

        private void OnVerifyButtonTapped(object sender, EventArgs e)
        {
            this.DialogResult = true;
            this.Hide();
        }

        private void OnCloseButtonTapped(object sender, EventArgs e)
        {
            this.DialogResult = false;
            this.Hide();
        }

        private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.ViewModel.PrimaryButtonTapped += OnVerifyButtonTapped;
            this.ViewModel.CloseButtonTapped += OnCloseButtonTapped;
        }

        private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            this.ViewModel.PrimaryButtonTapped -= OnVerifyButtonTapped;
            this.ViewModel.CloseButtonTapped -= OnCloseButtonTapped;
        }

        private void OnInputTextBoxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = false;

            if (e.Key == VirtualKey.Tab) return;

            if ((e.Key >= VirtualKey.Number0 && e.Key <= VirtualKey.Number9) ||
                (e.Key >= VirtualKey.NumberPad0 && e.Key <= VirtualKey.NumberPad9))
            {
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
                return;
            }

            if (e.Key == VirtualKey.Back)
            {
                var textBox = sender as TextBox;
                if (!textBox.Equals(this.TextBoxDigit1))
                    FocusManager.TryMoveFocus(FocusNavigationDirection.Previous);
                return;
            }

            if (this.IsPrimaryButtonEnabled && e.Key == VirtualKey.Enter)
            {
                if (this.ViewModel?.PrimaryButtonCommand?.CanExecute(null) == true)
                    this.ViewModel.PrimaryButtonCommand.Execute(null);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Put each digit of the MFA code from the clipboard to the corresponding place
        /// </summary>
        /// <param name="sender"><see cref="TextBox"/> that sent the paste event</param>
        /// <param name="e">Event arguments</param>
        private async void OnInputTextBoxPaste(object sender, TextControlPasteEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null) return;

            // Mark the event as handled first. Otherwise, the
            // default paste action will happen, then the custom paste
            // action, and the user will see the text box content change.
            e.Handled = true;

            try
            {
                // Get content from the clipboard.
                var dataPackageView = Clipboard.GetContent();
                if (!dataPackageView.Contains(StandardDataFormats.Text))
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Invalid MFA code. Format is not correct");
                    DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();
                    return;
                }

                // Check if the code format is correct
                var text = await dataPackageView.GetTextAsync();
                if (string.IsNullOrWhiteSpace(text) || !ValidationService.IsDigitsOnly(text))
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Invalid MFA code. Format is not correct");
                    DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();
                    return;
                }                    

                // Check if the code length is correct
                if (text.Length != this.ViewModel.Settings.MinLength)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Invalid MFA code. Length is not correct");
                    DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();
                    return;
                }

                this.ViewModel.Digit1 = text[0].ToString();
                this.ViewModel.Digit2 = text[1].ToString();
                this.ViewModel.Digit3 = text[2].ToString();
                this.ViewModel.Digit4 = text[3].ToString();
                this.ViewModel.Digit5 = text[4].ToString();
                this.ViewModel.Digit6 = text[5].ToString();
            }
            catch (Exception ex)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error pasting MFA code", ex);
                DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();
            }
        }

        #endregion
    }
}
