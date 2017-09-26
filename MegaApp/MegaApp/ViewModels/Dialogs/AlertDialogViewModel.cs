using System.Windows.Input;
using MegaApp.Classes;

namespace MegaApp.ViewModels.Dialogs
{
    public class AlertDialogViewModel : BaseViewModel
    {
        public AlertDialogViewModel()
        {
            this.ButtonCommand = new RelayCommand(ButtonAction);
        }

        #region Commands

        public virtual ICommand ButtonCommand { get; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Action to do by the button of the alert dialog
        /// </summary>
        protected virtual void ButtonAction()
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

        private string _buttonLabel;
        /// <summary>
        /// Label of the button of the alert dialog
        /// </summary>
        public string ButtonLabel
        {
            get { return _buttonLabel; }
            set { SetField(ref _buttonLabel, value); }
        }

        #endregion
    }
}
