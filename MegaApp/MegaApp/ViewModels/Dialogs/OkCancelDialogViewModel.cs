using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class OkCancelDialogViewModel : BaseViewModel
    {
        public OkCancelDialogViewModel()
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
        /// Action to do by the primary button of the dialog
        /// </summary>
        protected virtual void PrimaryButtonAction()
        {
            
        }

        /// <summary>
        /// Action to do by the secondary button of the dialog
        /// </summary>
        protected virtual void SecondaryButtonAction()
        {
            
        }

        #endregion

        #region Properties

        private string _titleText;
        /// <summary>
        /// Title of the dialog
        /// </summary>
        public string TitleText
        {
            get { return _titleText; }
            set { SetField(ref _titleText, value); }
        }

        private string _messageText;
        /// <summary>
        /// Message of the dialog
        /// </summary>
        public string MessageText
        {
            get { return _messageText; }
            set { SetField(ref _messageText, value); }
        }

        private string _warningText;
        /// <summary>
        /// Warning to display in the dialog
        /// </summary>
        public string WarningText
        {
            get { return _warningText; }
            set { SetField(ref _warningText, value); }
        }

        private string _primaryButtonLabel;
        /// <summary>
        /// Label of the primary button of the dialog
        /// </summary>
        public string PrimaryButtonLabel
        {
            get { return _primaryButtonLabel; }
            set { SetField(ref _primaryButtonLabel, value); }
        }

        private string _secondaryButtonLabel;
        /// <summary>
        /// Label of the secondary button of the dialog
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

        #region VisualResources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }
}
