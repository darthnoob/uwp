namespace MegaApp.ViewModels.Dialogs
{
    public class InputDialogViewModel : TwoButtonsDialogViewModel
    {
        #region Properties

        public InputDialogSettings Settings;

        private string _inputText;
        /// <summary>
        /// Text introduced in the input dialog
        /// </summary>
        public string InputText
        {
            get { return _inputText; }
            set
            {
                SetField(ref _inputText, value);
                OnPropertyChanged(nameof(this.PrimaryButtonState));
            }
        }

        /// <summary>
        /// State of the primary button of the input dialog
        /// </summary>
        public bool PrimaryButtonState => !string.IsNullOrWhiteSpace(this.InputText);

        #endregion
    }

    public class InputDialogSettings
    {
        /// <summary>
        /// Default text for the input in the dialog
        /// </summary>
        public string InputText { get; set; } = string.Empty;

        /// <summary>
        /// Is the text in the input selected as default
        /// </summary>
        public bool IsTextSelected { get; set; }

        /// <summary>
        /// Ignore extensions when the text is default selected.
        /// </summary>
        public bool IgnoreExtensionInSelection { get; set; }

        /// <summary>
        /// Placeholder text for the input dialog
        /// </summary>
        public string PlaceholderText { get; set; } = string.Empty;
    }
}
