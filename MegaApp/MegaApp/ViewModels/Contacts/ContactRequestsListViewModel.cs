using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactRequestsListViewModel : ContactsBaseViewModel<IMegaContactRequest>
    {
        /// <summary>
        /// View model to manage a contact requests list
        /// </summary>
        /// <param name="isOutgoing">Indicate the contact request list is for outgoing requests or not</param>
        public ContactRequestsListViewModel(bool isOutgoing) : base(isOutgoing)
        {
            this.isOutgoing = isOutgoing;
            this.ViewState = this.isOutgoing ? ContactsViewState.OutgoingRequests : ContactsViewState.IncomingRequests;
            this.List = new CollectionViewModel<IMegaContactRequest>();

            this.AddContactCommand = new RelayCommand(AddContact);
            this.AcceptContactRequestCommand = new RelayCommand(AcceptContactRequest);
            this.CancelContactRequestCommand = new RelayCommand(CancelContactRequest);
            this.DeclineContactRequestCommand = new RelayCommand(DeclineContactRequest);
            this.RemindContactRequestCommand = new RelayCommand(RemindContactRequest);            
        }

        #region Commands

        public ICommand AddContactCommand { get; }
        public ICommand AcceptContactRequestCommand { get; }
        public ICommand CancelContactRequestCommand { get; }
        public ICommand DeclineContactRequestCommand { get; }
        public ICommand RemindContactRequestCommand { get; }
        
        #endregion

        #region Methods

        public void Initialize(GlobalListener globalListener)
        {
            this.GetMegaContactRequests();

            if (globalListener == null) return;
            if (isOutgoing)
                globalListener.OutgoingContactRequestUpdated += (sender, args) => this.GetMegaContactRequests();
            else
                globalListener.IncomingContactRequestUpdated += (sender, args) => this.GetMegaContactRequests();
        }

        public void Deinitialize(GlobalListener globalListener)
        {
            if (globalListener == null) return;
            if (isOutgoing)
                globalListener.OutgoingContactRequestUpdated -= (sender, args) => this.GetMegaContactRequests();
            else
                globalListener.IncomingContactRequestUpdated -= (sender, args) => this.GetMegaContactRequests();
        }

        private void ListOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.List.Items))
            {
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems));
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfSelectedItems));
            }
        }

        public async void GetMegaContactRequests()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            await OnUiThreadAsync(() => this.List.Clear());

            var contactRequestsList = isOutgoing ?
                SdkService.MegaSdk.getOutgoingContactRequests() : 
                SdkService.MegaSdk.getIncomingContactRequests();

            var contactRequestsListSize = contactRequestsList.size();

            for (int i = 0; i < contactRequestsListSize; i++)
            {
                // To avoid null values
                if (contactRequestsList.get(i) == null) continue;

                var contactRequest = new ContactRequestViewModel(contactRequestsList.get(i), this);
                OnUiThread(() => this.List.Items.Add(contactRequest));
            }
        }

        private void AddContact()
        {
            this.OnAddContactTapped();
        }

        private void AcceptContactRequest()
        {
            if (!this.List.HasSelectedItems) return;

            // Use a temp variable to avoid InvalidOperationException
            var selectedContactRequests = this.List.SelectedItems.ToList();

            foreach (var contactRequest in selectedContactRequests)
                contactRequest.AcceptContactRequest();
        }

        private void DeclineContactRequest()
        {
            if (!this.List.HasSelectedItems) return;

            // Use a temp variable to avoid InvalidOperationException
            var selectedContactRequests = this.List.SelectedItems.ToList();

            foreach (var contactRequest in selectedContactRequests)
                contactRequest.DeclineContactRequest();
        }

        private void RemindContactRequest()
        {
            if (!this.List.HasSelectedItems) return;

            // Use a temp variable to avoid InvalidOperationException
            var selectedContactRequests = this.List.SelectedItems.ToList();

            foreach (var contactRequest in selectedContactRequests)
                contactRequest.RemindContactRequest();
        }

        private void CancelContactRequest()
        {
            if (!this.List.HasSelectedItems) return;

            // Use a temp variable to avoid InvalidOperationException
            var selectedContactRequests = this.List.SelectedItems.ToList();

            foreach (var contactRequest in selectedContactRequests)
                contactRequest.CancelContactRequest();
        }

        #endregion

        #region Properties

        private bool isOutgoing { get; set; }

        #endregion

        #region Ui_Resources

        public string AcceptContactText => ResourceService.UiResources.GetString("UI_AcceptContact");
        public string CancelInviteText => ResourceService.UiResources.GetString("UI_CancelInvite");
        public string DenyContactText => ResourceService.UiResources.GetString("UI_DenyContact");
        public string RemindContactText => ResourceService.UiResources.GetString("UI_RemindContact");

        #endregion

        #region VisualResources

        public string AcceptPathData => ResourceService.VisualResources.GetString("VR_ConfirmPathData");
        public string DeclinePathData => ResourceService.VisualResources.GetString("VR_CancelPathData");

        #endregion
    }
}
