using System.ComponentModel;
using MegaApp.Services;
using MegaApp.ViewModels.Contacts;

namespace MegaApp.ViewModels
{
    public class ContactsManagerViewModel : BaseSdkViewModel
    {
        public ContactsManagerViewModel() : base(SdkService.MegaSdk)
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

        public string SectionNameText => ResourceService.UiResources.GetString("UI_Contacts");
        public string ContactsTitle => ResourceService.UiResources.GetString("UI_Contacts");
        public string IncomingTitle => ResourceService.UiResources.GetString("UI_Incoming");
        public string OutgoingTitle => ResourceService.UiResources.GetString("UI_Outgoing");
        
        #endregion
    }
}
