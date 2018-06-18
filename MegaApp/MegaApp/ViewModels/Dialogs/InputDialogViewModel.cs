using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using MegaApp.Enums;

namespace MegaApp.ViewModels.Dialogs
{
    public class InputDialogViewModel : TwoButtonsDialogViewModel
    {
        #region Properties

        /// <summary>
        /// The action to execute by the primary button.
        /// </summary>
        public Func<string, bool> DialogAction;

        /// <summary>
        /// The async action to execute by the primary button.
        /// </summary>
        public Func<string, Task<bool>> DialogActionAsync;

        /// <summary>
        /// The settings of the dialog.
        /// </summary>
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
                this.InputState = InputState.Normal;
                this.WarningText = string.Empty;
            }
        }

        /// <summary>
        /// State of the primary button of the input dialog
        /// </summary>
        public bool PrimaryButtonState => this.ControlState &&
            !string.IsNullOrWhiteSpace(this.InputText) &&
            this.InputText.Length >= this.Settings?.MinLength;

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

        private InputState _inputState;
        /// <summary>
        /// State of the input field of the dialog.
        /// </summary>
        public InputState InputState
        {
            get { return _inputState; }
            set { SetField(ref _inputState, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Action to do by the primary button of the dialog
        /// </summary>
        protected override async void PrimaryButtonAction()
        {
            if (this.DialogAction != null || this.DialogActionAsync != null)
            {
                this.ControlState = false;
                OnPropertyChanged(nameof(this.PrimaryButtonState));
                this.IsBusy = true;

                bool result = false;
                if (this.DialogAction != null)
                    result = this.DialogAction.Invoke(this.InputText);

                if (this.DialogActionAsync != null)
                    result = await this.DialogActionAsync.Invoke(this.InputText);

                this.ControlState = true;
                OnPropertyChanged(nameof(this.PrimaryButtonState));
                this.IsBusy = false;

                if (!result) return;
            }

            base.PrimaryButtonAction();
        }

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
        /// Max length for the <see cref="InputText"/>
        /// </summary>
        public int MaxLength { get; set; } = int.MaxValue;

        /// <summary>
        /// Min length for the <see cref="InputText"/>
        /// </summary>
        public int MinLength { get; set; } = -1;

        /// <summary>
        /// Placeholder text for the input dialog
        /// </summary>
        public string PlaceholderText { get; set; } = string.Empty;
    }
}
