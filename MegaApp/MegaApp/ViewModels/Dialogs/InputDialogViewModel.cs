using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class InputDialogViewModel : BaseViewModel
    {
        public InputDialogViewModel()
        {
            this.PrimaryButtonCommand = new RelayCommand(PrimaryButtonAction);
            this.SecondaryButtonCommand = new RelayCommand(SecondaryButtonAction);
        }

        #region Commands

        public virtual ICommand PrimaryButtonCommand { get; }
        public virtual ICommand SecondaryButtonCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Action to do by the primary button of the input dialog
        /// </summary>
        protected virtual void PrimaryButtonAction()
        {

        }

        /// <summary>
        /// Action to do by the secondary button of the input dialog
        /// </summary>
        protected virtual void SecondaryButtonAction()
        {

        }

        #endregion

        #region Properties

        public InputDialogSettings Settings;

        private string _titleText;
        /// <summary>
        /// Title of the input dialog
        /// </summary>
        public string TitleText
        {
            get { return _titleText; }
            set { SetField(ref _titleText, value); }
        }

        private string _title;
        /// <summary>
        /// Title of the input dialog
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { SetField(ref _title, value); }
        }

        private string _messageText;
        /// <summary>
        /// Message of the input dialog
        /// </summary>
        public string MessageText
        {
            get { return _messageText; }
            set { SetField(ref _messageText, value); }
        }

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

        private string _primaryButtonLabel;
        /// <summary>
        /// Label of the primary button of the input dialog
        /// </summary>
        public string PrimaryButtonLabel
        {
            get { return _primaryButtonLabel; }
            set { SetField(ref _primaryButtonLabel, value); }
        }

        /// <summary>
        /// State of the primary button of the input dialog
        /// </summary>
        public bool PrimaryButtonState => !string.IsNullOrWhiteSpace(this.InputText);

        private string _secondaryButtonLabel;
        /// <summary>
        /// Label of the secondary button of the input dialog
        /// </summary>
        public string SecondaryButtonLabel
        {
            get { return _secondaryButtonLabel; }
            set { SetField(ref _secondaryButtonLabel, value); }
        }

        #endregion

        #region UiResources

        public string OkText => ResourceService.UiResources.GetString("UI_Ok");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");

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
    }
}
