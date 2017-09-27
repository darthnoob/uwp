using System.Windows.Input;
using MegaApp.Classes;

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

        #region Private Methods

        /// <summary>
        /// Action to do by the primary button of the alert dialog
        /// </summary>
        protected virtual void PrimaryButtonAction()
        {
            
        }

        /// <summary>
        /// Action to do by the secondary button of the alert dialog
        /// </summary>
        protected virtual void SecondaryButtonAction()
        {
            
        }

        #endregion

        #region Properties

        private string _title;
        /// <summary>
        /// Title of the alert dialog
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { SetField(ref _title, value); }
        }

        private string _message;
        /// <summary>
        /// Message of the alert dialog
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { SetField(ref _message, value); }
        }

        private string _primaryButtonLabel;
        /// <summary>
        /// Label of the primary button of the alert dialog
        /// </summary>
        public string PrimaryButtonLabel
        {
            get { return _primaryButtonLabel; }
            set { SetField(ref _primaryButtonLabel, value); }
        }

        private string _secondaryButtonLabel;
        /// <summary>
        /// Label of the secondary button of the alert dialog
        /// </summary>
        public string SecondaryButtonLabel
        {
            get { return _secondaryButtonLabel; }
            set { SetField(ref _secondaryButtonLabel, value); }
        }

        #endregion
    }
}
