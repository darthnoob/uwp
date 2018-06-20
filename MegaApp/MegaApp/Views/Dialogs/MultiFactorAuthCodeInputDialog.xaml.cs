using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using MegaApp.UserControls;
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
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="dialogAction">Action to execute by the primary button.</param>
        public MultiFactorAuthCodeInputDialog(string title, string message, Func<string, bool> dialogAction)
        {
            this.InitializeComponent();

            this.ViewModel.TitleText = title;
            this.ViewModel.MessageText = message;
            this.ViewModel.DialogAction = dialogAction;
        }

        /// <summary>
        /// Creates an input dialog to type the MFA 6-digit code and also executes an action.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="dialogAction">Async action to execute by the primary button.</param>
        public MultiFactorAuthCodeInputDialog(string title, string message, Func<string, Task<bool>> dialogActionAsync)
        {
            this.InitializeComponent();

            this.ViewModel.TitleText = title;
            this.ViewModel.MessageText = message;
            this.ViewModel.DialogActionAsync = dialogActionAsync;
        }

        #region Methods

        private void OnPrimaryButtonTapped(object sender, EventArgs e)
        {
            this.DialogResult = true;
            this.Hide();
        }

        private void OnSecondaryButtonTapped(object sender, EventArgs e)
        {
            this.DialogResult = false;
            this.Hide();
        }

        private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.ViewModel.PrimaryButtonTapped += OnPrimaryButtonTapped;
            this.ViewModel.SecondaryButtonTapped += OnSecondaryButtonTapped;
        }

        private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            this.ViewModel.PrimaryButtonTapped -= OnPrimaryButtonTapped;
            this.ViewModel.SecondaryButtonTapped -= OnSecondaryButtonTapped;
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

        #endregion
    }
}
