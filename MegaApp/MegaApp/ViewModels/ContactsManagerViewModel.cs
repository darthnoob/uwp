using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.Contacts;

namespace MegaApp.ViewModels
{
    public class ContactsManagerViewModel : BaseSdkViewModel
    {
        public ContactsManagerViewModel()
        {
            this.MegaContacts = new ContactsListViewModel();
            this.IncomingContactRequests = new ContactRequestsListViewModel(false);
            this.OutgoingContactRequests = new ContactRequestsListViewModel(true);

            this.AddContactCommand = new RelayCommand(AddContact);
        }

        #region Commands

        public ICommand AddContactCommand { get; }

        #endregion

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

        private void AddContact()
        {
            this.MegaContacts.AddContact();
        }

        #endregion

        #region Properties

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

        public string AddContactText => ResourceService.UiResources.GetString("UI_AddContact");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");

        #endregion

        #region VisualResources

        public string AddContactPathData => ResourceService.VisualResources.GetString("VR_AddContactPathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");

        #endregion
    }
}
