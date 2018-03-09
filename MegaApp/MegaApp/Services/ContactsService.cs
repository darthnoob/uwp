using System.Threading.Tasks;
using mega;
using MegaApp.MegaApi;
using MegaApp.ViewModels.Contacts;

namespace MegaApp.Services
{
    public static class ContactsService
    {
        private static ContactsListViewModel _megaContacts;
        public static ContactsListViewModel MegaContacts
        {
            get
            {
                if (_megaContacts != null) return _megaContacts;

                _megaContacts = new ContactsListViewModel();
                _megaContacts.Initialize();
                _megaContacts.GetMegaContacts();
                return _megaContacts;
            }
        }

        private static ContactRequestsListViewModel _incomingContactRequests;
        public static ContactRequestsListViewModel IncomingContactRequests
        {
            get
            {
                if (_incomingContactRequests != null) return _incomingContactRequests;

                _incomingContactRequests = new ContactRequestsListViewModel(false);
                _incomingContactRequests.Initialize();
                _incomingContactRequests.GetMegaContactRequests();
                return _incomingContactRequests;
            }
        }

        private static ContactRequestsListViewModel _outgoingContactRequest;
        public static ContactRequestsListViewModel OutgoingContactRequests
        {
            get
            {
                if (_outgoingContactRequest != null) return _outgoingContactRequest;

                _outgoingContactRequest = new ContactRequestsListViewModel(true);
                _outgoingContactRequest.Initialize();
                _outgoingContactRequest.GetMegaContactRequests();
                return _outgoingContactRequest;
            }
        }

        public static void Clear()
        {
            MegaContacts.ItemCollection.Clear();
            IncomingContactRequests.ItemCollection.Clear();
            OutgoingContactRequests.ItemCollection.Clear();
        }

        public static async Task<string> GetContactFirstName(MUser contact)
        {
            var contactAttributeRequestListener = new GetUserAttributeRequestListenerAsync();
            var firstName = await contactAttributeRequestListener.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.getUserAttribute(
                    contact, (int) MUserAttrType.USER_ATTR_FIRSTNAME, contactAttributeRequestListener);
            });
            return firstName;
        }

        public static async Task<string> GetContactLastName(MUser contact)
        {
            var contactAttributeRequestListener = new GetUserAttributeRequestListenerAsync();
            var lastName = await contactAttributeRequestListener.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.getUserAttribute(
                    contact, (int) MUserAttrType.USER_ATTR_LASTNAME, contactAttributeRequestListener);
            });
            return lastName;
        }
    }
}
