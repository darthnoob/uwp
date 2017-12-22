using System;
using System.ComponentModel;
using mega;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.Contacts;
using MegaApp.Views.Dialogs;

namespace MegaApp.ViewModels
{
    public class ContactsManagerViewModel : BaseSdkViewModel
    {
        public ContactsManagerViewModel()
        {
            
        }

        #region Methods

        public void Initialize()
        {
            this.MegaContacts.PropertyChanged += this.OnMegaContactsPropertyChanged;
            this.IncomingContactRequests.PropertyChanged += this.OnIncomingContactRequestsPropertyChanged;
            this.OutgoingContactRequests.PropertyChanged += this.OnOutgoingContactRequestsPropertyChanged;
        }

        public void Deinitialize()
        {
            this.MegaContacts.PropertyChanged -= this.OnMegaContactsPropertyChanged;
            this.IncomingContactRequests.PropertyChanged -= this.OnIncomingContactRequestsPropertyChanged;
            this.OutgoingContactRequests.PropertyChanged -= this.OnOutgoingContactRequestsPropertyChanged;
        }

        private void OnMegaContactsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(this.MegaContacts));
        }

        private void OnIncomingContactRequestsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(this.IncomingContactRequests));
        }

        private void OnOutgoingContactRequestsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(this.OutgoingContactRequests));
        }

        private async void OnAddContactTapped(object sender, EventArgs e)
        {
            var addContactDialog = new AddContactDialog();
            await addContactDialog.ShowAsync();

            if (!addContactDialog.DialogResult) return;

            var inviteContact = new InviteContactRequestListenerAsync();
            var result = await inviteContact.ExecuteAsync(() =>
                SdkService.MegaSdk.inviteContact(addContactDialog.ContactEmail, addContactDialog.EmailContent,
                    MContactRequestInviteActionType.INVITE_ACTION_ADD, inviteContact));

            switch (result)
            {
                case Enums.InviteContactResult.Success:
                    await DialogService.ShowAlertAsync(ResourceService.UiResources.GetString("UI_AddContact"),
                        string.Format(ResourceService.AppMessages.GetString("AM_InviteContactSuccessfully"),
                        addContactDialog.ContactEmail));
                    break;

                case Enums.InviteContactResult.AlreadyExists:
                    await DialogService.ShowAlertAsync(ResourceService.UiResources.GetString("UI_AddContact"),
                        ResourceService.AppMessages.GetString("AM_ContactAlreadyExists"));
                    break;

                case Enums.InviteContactResult.Unknown:
                    await DialogService.ShowAlertAsync(ResourceService.UiResources.GetString("UI_AddContact"),
                        ResourceService.AppMessages.GetString("AM_InviteContactFailed"));
                    break;
            }
        }

        #endregion

        #region Properties

        private object _activeView;
        public object ActiveView
        {
            get { return _activeView; }
            set { SetField(ref _activeView, value); }
        }

        public ContactsListViewModel MegaContacts => ContactsService.MegaContacts;
        public ContactRequestsListViewModel IncomingContactRequests => ContactsService.IncomingContactRequests;
        public ContactRequestsListViewModel OutgoingContactRequests => ContactsService.OutgoingContactRequests;

        #endregion

        #region UiResources

        public string ContactsTitle => ResourceService.UiResources.GetString("UI_Contacts");
        public string IncomingTitle => ResourceService.UiResources.GetString("UI_Incoming");
        public string OutgoingTitle => ResourceService.UiResources.GetString("UI_Outgoing");
        
        #endregion
    }
}
