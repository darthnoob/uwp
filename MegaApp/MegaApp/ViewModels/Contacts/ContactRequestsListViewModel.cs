using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using mega;
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
            this.ContentType = this.isOutgoing ? ContactsContentType.OutgoingRequests : ContactsContentType.IncomingRequests;
            this.ItemCollection = new CollectionViewModel<IMegaContactRequest>();

            this.AddContactCommand = new RelayCommand(AddContact);
            this.AcceptContactRequestCommand = new RelayCommand(AcceptContactRequest);
            this.IgnoreContactRequestCommand = new RelayCommand(IgnoreContactRequest);
            this.CancelContactRequestCommand = new RelayCommand(CancelContactRequest);
            this.DeclineContactRequestCommand = new RelayCommand(DeclineContactRequest);
            this.RemindContactRequestCommand = new RelayCommand(RemindContactRequest);
            this.InvertOrderCommand = new RelayCommand(InvertOrder);

            this.CurrentOrder = MSortOrderType.ORDER_ALPHABETICAL_ASC;
        }

        #region Commands

        public override ICommand AddContactCommand { get; }
        
        public override ICommand AcceptContactRequestCommand { get; }
        public override ICommand IgnoreContactRequestCommand { get; }
        public override ICommand CancelContactRequestCommand { get; }
        public override ICommand DeclineContactRequestCommand { get; }
        public override ICommand RemindContactRequestCommand { get; }

        public override ICommand InvertOrderCommand { get; }

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
            if (e.PropertyName == nameof(this.ItemCollection.Items))
            {
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems));
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfSelectedItems));
            }
        }

        public async void GetMegaContactRequests()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            await OnUiThreadAsync(() => this.ItemCollection.Clear());

            var contactRequestsList = isOutgoing ?
                SdkService.MegaSdk.getOutgoingContactRequests() : 
                SdkService.MegaSdk.getIncomingContactRequests();

            var contactRequestsListSize = contactRequestsList.size();

            for (int i = 0; i < contactRequestsListSize; i++)
            {
                // To avoid null values
                if (contactRequestsList.get(i) == null) continue;

                var contactRequest = new ContactRequestViewModel(contactRequestsList.get(i), this);
                await OnUiThreadAsync(() => this.ItemCollection.Items.Add(contactRequest));
            }

            this.SortBy(this.CurrentOrder);
        }

        private void AddContact()
        {
            this.OnAddContactTapped();
        }

        private void AcceptContactRequest()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            // Use a temp variable to avoid InvalidOperationException
            var selectedContactRequests = this.ItemCollection.SelectedItems.ToList();

            foreach (var contactRequest in selectedContactRequests)
                contactRequest.AcceptContactRequest();
        }

        private void IgnoreContactRequest()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            // Use a temp variable to avoid InvalidOperationException
            var selectedContactRequests = this.ItemCollection.SelectedItems.ToList();

            foreach (var contactRequest in selectedContactRequests)
                contactRequest.IgnoreContactRequest();
        }

        private void DeclineContactRequest()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            // Use a temp variable to avoid InvalidOperationException
            var selectedContactRequests = this.ItemCollection.SelectedItems.ToList();

            foreach (var contactRequest in selectedContactRequests)
                contactRequest.DeclineContactRequest();
        }

        private void RemindContactRequest()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            // Use a temp variable to avoid InvalidOperationException
            var selectedContactRequests = this.ItemCollection.SelectedItems.ToList();

            foreach (var contactRequest in selectedContactRequests)
                contactRequest.RemindContactRequest();
        }

        private void CancelContactRequest()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            // Use a temp variable to avoid InvalidOperationException
            var selectedContactRequests = this.ItemCollection.SelectedItems.ToList();

            foreach (var contactRequest in selectedContactRequests)
                contactRequest.CancelContactRequest();
        }

        public void SortBy(MSortOrderType sortOption)
        {
            switch (sortOption)
            {
                case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaContactRequest>(
                            this.ItemCollection.Items.OrderBy(item => this.isOutgoing ?
                            item.TargetEmail : item.SourceEmail));
                    });
                    break;

                case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaContactRequest>(
                            this.ItemCollection.Items.OrderByDescending(item => this.isOutgoing ?
                            item.TargetEmail : item.SourceEmail));
                    });
                    break;

                default:
                    return;
            }
        }

        private void InvertOrder()
        {
            switch (this.CurrentOrder)
            {
                case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    this.CurrentOrder = MSortOrderType.ORDER_ALPHABETICAL_DESC;
                    break;
                case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                    this.CurrentOrder = MSortOrderType.ORDER_ALPHABETICAL_ASC;
                    break;
                default:
                    return;
            }

            this.SortBy(this.CurrentOrder);
        }

        #endregion

        #region Properties

        private bool isOutgoing { get; set; }

        #endregion
    }
}
