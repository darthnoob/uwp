using MegaApp.Services;
using System;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MegaApp.Classes
{
    /// <summary>
    /// Class that provides functionality to show the user a MEGA styled input dialog
    /// </summary>
    public class CustomInputDialog
    {
        #region Events

        public event EventHandler<CustomInputDialogOkButtonArgs> OkButtonTapped;
        public event EventHandler CancelButtonTapped;

        #endregion

        #region Controls

        protected TextBox InputControl { get; set; }
        protected ContentDialog DialogWindow { get; set; }

        #endregion

        #region Private Properties

        private readonly string _title;
        private readonly string _message;
        private readonly CustomInputDialogSettings _settings;
        private readonly AppInformation _appInformation;

        #endregion

        /// <summary>
        /// Create a CustomInputDialog
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Message above the input control</param>
        /// <param name="appInformation">App information for restricting number of dialogs</param>
        /// <param name="settings">Dialog settings to manipulate the dialog look and behavior</param>
        public CustomInputDialog(string title, string message, AppInformation appInformation,
            CustomInputDialogSettings settings = null)
        {
            _title = title;
            _message = message;
            _appInformation = appInformation;

            // Set the settings. If none specfied, create a default set
            _settings = settings ?? new CustomInputDialogSettings()
            {
                DefaultText = string.Empty,
                IgnoreExtensionInSelection = false,
                SelectDefaultText = false,
                OverrideCancelButtonText = string.Empty,
                OverrideOkButtonText = string.Empty
            };

            DialogWindow = new ContentDialog() { Title = _title.ToUpper() };

            // Setup Content
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = _message,
                Margin = new Thickness(0, 50, 0, 22),
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.Wrap
            });

            // Create input control
            InputControl = new TextBox()
            {
                Text = _settings.DefaultText, // The specified default text in the textbox control input area                
                HorizontalAlignment = HorizontalAlignment.Stretch,
                TextWrapping = TextWrapping.Wrap
            };

            panel.Children.Add(InputControl);
            DialogWindow.Content = panel;

            // Add Buttons
            DialogWindow.PrimaryButtonText = ResourceService.UiResources.GetString("UI_Ok");
            DialogWindow.PrimaryButtonClick += delegate
            {                
                if (string.IsNullOrWhiteSpace(InputControl.Text)) return;

                OkButtonTapped?.Invoke(this, new CustomInputDialogOkButtonArgs(InputControl.Text));
            };

            DialogWindow.SecondaryButtonText = ResourceService.UiResources.GetString("UI_Cancel");
            DialogWindow.SecondaryButtonClick += delegate
            {
                CancelButtonTapped?.Invoke(this, new EventArgs());
            };
        }

        #region Public Methods

        /// <summary>
        /// Display the CustomInputDialog on screen with the specified parameter from the constructor
        /// </summary>
        public async void ShowDialogAsync()
        {
            await DialogWindow.ShowAsync();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Select a text in the specified textbox control
        /// </summary>
        /// <param name="textBox">Textbox control to select a text</param>
        /// <param name="defaultText">The default text that is in the textbox input area</param>
        /// <param name="ignoreExtension">Specifies if any filename extensions should be ignored during selection</param>
        private void SetTextSelection(TextBox textBox, string defaultText, bool ignoreExtension)
        {
            // If no text is provided, no selection is possible
            if (string.IsNullOrEmpty(defaultText)) return;

            // Selection always start at zero (array)
            textBox.SelectionStart = 0;

            // Select the whole text, or just the filename without extension if an extension is available
            string extension = ignoreExtension ? Path.GetExtension(defaultText) : string.Empty;
            textBox.SelectionLength = string.IsNullOrEmpty(extension)
                ? defaultText.Length
                : defaultText.LastIndexOf(extension, StringComparison.CurrentCulture);
        }

        #endregion
    }

    /// <summary>
    /// Settings options to use in CustomInputDialog
    /// </summary>
    public class CustomInputDialogSettings
    {
        /// <summary>
        /// Populate textbox control with a default input text
        /// </summary>
        public string DefaultText { get; set; }

        /// <summary>
        /// Specifies if the default text has any text selection
        /// </summary>
        public bool SelectDefaultText { get; set; }

        /// <summary>
        /// Specifies to ignore file extensions in the text selection
        /// </summary>
        public bool IgnoreExtensionInSelection { get; set; }

        /// <summary>
        /// Overrides the default 'ok' label for the application bar button
        /// </summary>
        public string OverrideOkButtonText { get; set; }

        /// <summary>
        /// Overrides the default 'cancel' label for the application bar button
        /// </summary>
        public string OverrideCancelButtonText { get; set; }
    }

    /// <summary>
    /// Event arguments for the Ok button tapped event in the CustomInputDialog class
    /// </summary>
    public class CustomInputDialogOkButtonArgs : EventArgs
    {
        public CustomInputDialogOkButtonArgs(string inputText)
        {
            InputText = inputText;
        }

        /// <summary>
        /// The input text at the moment the user tapped the ok button
        /// </summary>
        public string InputText { get; private set; }
    }
}
