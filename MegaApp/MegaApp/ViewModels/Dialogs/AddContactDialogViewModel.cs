using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class AddContactDialogViewModel : BaseViewModel
    {
        public AddContactDialogViewModel()
        {
            this.AddButtonCommand = new RelayCommand(Add);
            this.CancelButtonCommand = new RelayCommand(Cancel);

            this.EmailContent = ResourceService.UiResources.GetString("UI_AddContactEmailDefaultText");
        }

        #region Commands

        public ICommand AddButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the user invites a new contact.
        /// </summary>
        public event EventHandler ContactInvited;

        /// <summary>
        /// Event invocator method called when the user invites a new contact.
        /// </summary>
        protected virtual void OnContactInvited()
        {
            this.CanClose = true;
            this.ContactInvited?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the user cancels the invite contact process.
        /// </summary>
        public event EventHandler Canceled;

        /// <summary>
        /// Event invocator method called when the user cancels the invite contact process.
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

        private void Add()
        {
            if (!ValidationService.IsValidEmail(this.ContactEmail))
            {
                OnEmailError();
                this.ErrorMessage = ResourceService.AppMessages.GetString("AM_IncorrectEmailFormat");
                return;
            }

            if (this.ContactEmail.Equals(SdkService.MegaSdk.getMyEmail()))
            {
                OnEmailError();
                this.ErrorMessage = ResourceService.AppMessages.GetString("AM_InviteContactFailedOwnEmail");
                return;
            }

            OnContactInvited();
        }

        private void Cancel()
        {
            OnCanceled();
        }

        #endregion

        #region Properties

        public bool CanClose { get; set; }

        private string _contactEmail;
        public string ContactEmail
        {
            get { return _contactEmail; }
            set { SetField(ref _contactEmail, value); }
        }

        private string _emailContent;
        public string EmailContent
        {
            get { return _emailContent; }
            set { SetField(ref _emailContent, value); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetField(ref _errorMessage, value); }
        }

        #endregion

        #region UiResources

        public string TitleText => ResourceService.UiResources.GetString("UI_AddContact");

        public string AddText => ResourceService.UiResources.GetString("UI_Add");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string EmailText => ResourceService.UiResources.GetString("UI_Email");

        #endregion
    }
}
