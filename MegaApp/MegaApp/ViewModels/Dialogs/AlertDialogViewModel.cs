using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;

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

        private string _titleText;
        /// <summary>
        /// Title of the alert dialog
        /// </summary>
        public string TitleText
        {
            get { return _titleText; }
            set { SetField(ref _titleText, value); }
        }

        private string _messageText;
        /// <summary>
        /// Message of the alert dialog
        /// </summary>
        public string MessageText
        {
            get { return _messageText; }
            set { SetField(ref _messageText, value); }
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

        #region UiResources

        public string OkText => ResourceService.UiResources.GetString("UI_Ok");
        
        #endregion
    }
}
