using System;
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
            this.MegaContacts = new ContactsListViewModel();
            this.MegaContacts.AddContactTapped += OnAddContactTapped;

            this.IncomingContactRequests = new ContactRequestsListViewModel(false);

            this.OutgoingContactRequests = new ContactRequestsListViewModel(true);
            this.OutgoingContactRequests.AddContactTapped += OnAddContactTapped;
        }

        #region Methods

        public void Initialize(GlobalListener globalListener)
        {
            this.MegaContacts.Initialize(globalListener);
            this.IncomingContactRequests.Initialize(globalListener);
            this.OutgoingContactRequests.Initialize(globalListener);
        }

        public void Deinitialize(GlobalListener globalListener)
        {
            this.MegaContacts.Deinitialize(globalListener);
            this.IncomingContactRequests.Deinitialize(globalListener);
            this.OutgoingContactRequests.Deinitialize(globalListener);
        }

        private async void OnAddContactTapped(object sender, EventArgs e)
        {
            var addContactDialog = new AddContactDialog();
            await addContactDialog.ShowAsync();

            if (addContactDialog.DialogResult)
            {
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
        }

        #endregion

        #region Properties

        private object _activeView;
        public object ActiveView
        {
            get { return _activeView; }
            set { SetField(ref _activeView, value); }
        }

        private ContactsListViewModel _megaContacts;
        public ContactsListViewModel MegaContacts
        {
            get { return _megaContacts; }
            set { SetField(ref _megaContacts, value); }
        }

        private ContactRequestsListViewModel _incomingContactRequests;
        public ContactRequestsListViewModel IncomingContactRequests
        {
            get { return _incomingContactRequests; }
            set { SetField(ref _incomingContactRequests, value); }
        }

        private ContactRequestsListViewModel _outgoingContactRequest;
        public ContactRequestsListViewModel OutgoingContactRequests
        {
            get { return _outgoingContactRequest; }
            set { SetField(ref _outgoingContactRequest, value); }
        }

        #endregion

        #region UiResources

        public string ContactsTitle => ResourceService.UiResources.GetString("UI_Contacts");
        public string IncomingTitle => ResourceService.UiResources.GetString("UI_Incoming");
        public string OutgoingTitle => ResourceService.UiResources.GetString("UI_Outgoing");
        
        #endregion
    }
}
