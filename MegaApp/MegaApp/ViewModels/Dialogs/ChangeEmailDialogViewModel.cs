using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class ChangeEmailDialogViewModel : BaseViewModel
    {
        public ChangeEmailDialogViewModel()
        {
            this.SaveButtonCommand = new RelayCommand(Save);
            this.CancelButtonCommand = new RelayCommand(Cancel);
        }

        #region Commands

        public ICommand SaveButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the user saves the email change.
        /// </summary>
        public event EventHandler Saved;

        /// <summary>
        /// Event invocator method called when the user saves the email change.
        /// </summary>
        protected virtual void OnSaved()
        {
            this.CanClose = true;
            this.Saved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the user cancels the email change.
        /// </summary>
        public event EventHandler Canceled;

        /// <summary>
        /// Event invocator method called when the user cancels the email change.
        /// </summary>
        protected virtual void OnCanceled()
        {
            this.CanClose = true;
            this.Canceled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the email is wrong.
        /// </summary>
        public event EventHandler EmailError;

        /// <summary>
        /// Event invocator method called when the email is wrong.
        /// </summary>
        protected virtual void OnEmailError()
        {
            this.EmailError?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        private void Save()
        {
            if (!ValidationService.IsValidEmail(this.NewEmail))
            {
                OnEmailError();
                this.ErrorMessage = ResourceService.AppMessages.GetString("AM_IncorrectEmailFormat");
                return;
            }

            OnSaved();
        }

        private void Cancel()
        {
            OnCanceled();
        }

        #endregion

        #region Properties

        public bool CanClose = false;

        private string _newEmail;
        public string NewEmail
        {
            get { return _newEmail; }
            set { SetField(ref _newEmail, value); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetField(ref _errorMessage, value); }
        }

        #endregion

        #region UiResources

        public string TitleText => ResourceService.UiResources.GetString("UI_ChangeEmail");
        public string DescriptionText => ResourceService.UiResources.GetString("UI_ChangeEmailDescription");
        public string EnterNewEmailText => ResourceService.UiResources.GetString("UI_EnterNewEmail");
        public string SaveText => ResourceService.UiResources.GetString("UI_Save");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        
        #endregion
    }
}
