using Windows.UI.Xaml.Input;

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

        /// <summary>
        /// Input scope of the dialog
        /// </summary>
        public InputScope InputScope
        {
            get
            {
                var inputScope = new InputScope();
                inputScope.Names.Add(new InputScopeName(this.Settings.InputScopeValue));
                return inputScope;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the input scope of the dialog
        /// </summary>
        /// <param name="inputScopeValue">Value to set</param>
        public void SetInputScope(InputScopeNameValue inputScopeValue)
        {
            this.Settings.InputScopeValue = inputScopeValue;
            OnPropertyChanged(nameof(this.InputScope));
        }

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
        /// Input scope value for the dialog
        /// </summary>
        public InputScopeNameValue InputScopeValue { get; set; } = InputScopeNameValue.Text;

        /// <summary>
        /// Placeholder text for the input dialog
        /// </summary>
        public string PlaceholderText { get; set; } = string.Empty;
    }
}
