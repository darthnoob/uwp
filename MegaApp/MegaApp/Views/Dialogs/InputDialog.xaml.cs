using System;
using System.IO;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
        /// Creates an standard input dialog.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="primaryButton">Label of the primary button of the input dialog. Default value "Ok".</param>
        /// <param name="secondaryButton">Label of the secondary button of the input dialog. Default value "Cancel".</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        public InputDialog(string title, string message, string primaryButton = null, 
            string secondaryButton = null, InputDialogSettings settings = null)
        {
            this.InitializeComponent();
            this.Initialize(title, message, primaryButton, secondaryButton, settings);
        }

        /// <summary>
        /// Creates an standard input dialog which also executes an action.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="dialogAction">Action to execute by the primary button.</param>
        /// <param name="primaryButton">Label of the primary button of the input dialog. Default value "Ok".</param>
        /// <param name="secondaryButton">Label of the secondary button of the input dialog. Default value "Cancel".</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        public InputDialog(string title, string message, Func<string, bool> dialogAction,
            string primaryButton = null, string secondaryButton = null,
            InputDialogSettings settings = null)
        {
            this.InitializeComponent();
            this.Initialize(title, message, primaryButton, secondaryButton, settings);
            this.ViewModel.DialogAction = dialogAction;
        }

        /// <summary>
        /// Creates an standard input dialog which also executes an async action.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="dialogActionAsync">Async action to execute by the primary button.</param>
        /// <param name="primaryButton">Label of the primary button of the input dialog. Default value "Ok".</param>
        /// <param name="secondaryButton">Label of the secondary button of the input dialog. Default value "Cancel".</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        public InputDialog(string title, string message, Func<string, Task<bool>> dialogActionAsync,
            string primaryButton = null, string secondaryButton = null, InputDialogSettings settings = null)
        {
            this.InitializeComponent();
            this.Initialize(title, message, primaryButton, secondaryButton, settings);
            this.ViewModel.DialogActionAsync = dialogActionAsync;
        }

        /// <summary>
        /// Initialize the input dialog
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="primaryButton">Label of the primary button of the input dialog. Default value "Ok".</param>
        /// <param name="secondaryButton">Label of the secondary button of the input dialog. Default value "Cancel".</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        private void Initialize(string title, string message, string primaryButton = null,
            string secondaryButton = null, InputDialogSettings settings = null)
        {
            this.ViewModel.TitleText = title;
            this.ViewModel.MessageText = message;
            this.ViewModel.PrimaryButtonLabel = primaryButton ?? this.ViewModel.OkText;
            this.ViewModel.SecondaryButtonLabel = secondaryButton ?? this.ViewModel.CancelText;

            // Create default input settings if null
            this.ViewModel.Settings = settings ?? new InputDialogSettings();
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
            if (this.ViewModel.Settings.InputScopeValue == InputScopeNameValue.NumericPin)
            {
                if ((e.Key >= VirtualKey.Number0 && e.Key <= VirtualKey.Number9) ||
                (e.Key >= VirtualKey.NumberPad0 && e.Key <= VirtualKey.NumberPad9))
                {
                    e.Handled = false;
                    return;
                }

                e.Handled = true;
            }

            if (this.IsPrimaryButtonEnabled && e.Key == VirtualKey.Enter)
            {
                if (this.ViewModel?.PrimaryButtonCommand?.CanExecute(null) == true)
                    this.ViewModel.PrimaryButtonCommand.Execute(null);

                e.Handled = true;
            }
        }

        #endregion
    }
}
