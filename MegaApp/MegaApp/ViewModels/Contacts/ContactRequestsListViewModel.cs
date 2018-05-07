using System;
using System.Collections.ObjectModel;
using System.Linq;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactRequestsListViewModel : ContactsBaseViewModel<IMegaContactRequest>
    {
        /// <summary>
        /// View model to manage a contact requests list
        /// </summary>
        /// <param name="isOutgoing">Indicate the contact request list is for outgoing requests or not</param>
        public ContactRequestsListViewModel(bool isOutgoing) : 
            base(SdkService.MegaSdk, isOutgoing)
        {
            this._isOutgoing = isOutgoing;
            this.ContentType = this._isOutgoing ? ContactsContentType.OutgoingRequests : ContactsContentType.IncomingRequests;
            this.ItemCollection = new CollectionViewModel<IMegaContactRequest>(this.MegaSdk);

            this.AddContactCommand = new RelayCommand(AddContact);
            this.AcceptContactRequestCommand = new RelayCommand(AcceptContactRequest);
            this.IgnoreContactRequestCommand = new RelayCommand(IgnoreContactRequest);
            this.CancelContactRequestCommand = new RelayCommand(CancelContactRequest);
            this.DeclineContactRequestCommand = new RelayCommand(DeclineContactRequest);
            this.RemindContactRequestCommand = new RelayCommand(RemindContactRequest);

            this.CurrentOrder = ContactRerquestsSortOrderType.ORDER_NAME;
        }

        #region Methods

        public void Initialize()
        {
            this.ItemCollection.OrderInverted += OnOrderInverted;
            this.ItemCollection.SelectedItemsCollectionChanged += OnSelectedItemsCollectionChanged;

            if (App.GlobalListener == null) return;
            if (_isOutgoing)
                App.GlobalListener.OutgoingContactRequestUpdated += OnOutgoingContactRequestUpdated;
            else
                App.GlobalListener.IncomingContactRequestUpdated += OnIncomingContactRequestUpdated;
        }

        public void Deinitialize()
        {
            this.ItemCollection.OrderInverted -= OnOrderInverted;
            this.ItemCollection.SelectedItemsCollectionChanged -= OnSelectedItemsCollectionChanged;

            if (App.GlobalListener == null) return;
            if (_isOutgoing)
                App.GlobalListener.OutgoingContactRequestUpdated -= OnOutgoingContactRequestUpdated;
            else
                App.GlobalListener.IncomingContactRequestUpdated -= OnIncomingContactRequestUpdated;
        }

        private void OnIncomingContactRequestUpdated(object sender, EventArgs args) => this.GetMegaContactRequests();
        private void OnOutgoingContactRequestUpdated(object sender, EventArgs args) => this.GetMegaContactRequests();

        public async void GetMegaContactRequests()
        {
            // User must be online to perform this operation
            if (!await IsUserOnlineAsync()) return;

            await OnUiThreadAsync(() => this.ItemCollection.Clear());

            var contactRequestsList = _isOutgoing ?
                this.MegaSdk.getOutgoingContactRequests() : 
                this.MegaSdk.getIncomingContactRequests();

            var contactRequestsListSize = contactRequestsList.size();

            for (int i = 0; i < contactRequestsListSize; i++)
            {
                // To avoid null values
                if (contactRequestsList.get(i) == null) continue;

                var contactRequest = new ContactRequestViewModel(contactRequestsList.get(i), this);
                await OnUiThreadAsync(() => this.ItemCollection.Items.Add(contactRequest));
            }

            this.SortBy(this.CurrentOrder, this.ItemCollection.CurrentOrderDirection);
        }

        private void AddContact()
        {
            if (ContactsService.MegaContacts.AddContactCommand.CanExecute(null))
                ContactsService.MegaContacts.AddContactCommand.Execute(null);
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

        public void SortBy(ContactRerquestsSortOrderType sortOption, SortOrderDirection sortDirection)
        {
            OnUiThread(() => this.ItemCollection.DisableCollectionChangedDetection());

            switch (sortOption)
            {
                case ContactRerquestsSortOrderType.ORDER_NAME:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaContactRequest>(this.ItemCollection.IsCurrentOrderAscending ?
                            this.ItemCollection.Items.OrderBy(item => this._isOutgoing ? item.TargetEmail : item.SourceEmail) :
                            this.ItemCollection.Items.OrderByDescending(item => this._isOutgoing ? item.TargetEmail : item.SourceEmail));
                    });
                    break;
            }

            OnUiThread(() => this.ItemCollection.EnableCollectionChangedDetection());
        }

        private void OnItemCollectionChanged(object sender, EventArgs args) =>
            OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems), nameof(this.OrderTypeAndNumberOfSelectedItems));

        private void OnSelectedItemsCollectionChanged(object sender, EventArgs args) =>
            OnPropertyChanged(nameof(this.OrderTypeAndNumberOfSelectedItems),
                nameof(this.IsRemindAllowed));

        private void OnOrderInverted(object sender, EventArgs args) =>
            this.SortBy(this.CurrentOrder, this.ItemCollection.CurrentOrderDirection);

        public override void UpdateNetworkStatus()
        {
            foreach (var item in this.ItemCollection.Items)
            {
                var contactRequest = item as ContactRequestViewModel;
                if (contactRequest == null) continue;
                contactRequest.UpdateNetworkStatus();
            }
        }

        #endregion

        #region Properties

        private bool _isOutgoing { get; set; }

        public string OrderTypeAndNumberOfItems
        {
            get
            {
                switch (this.CurrentOrder)
                {
                    case ContactRerquestsSortOrderType.ORDER_NAME:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByName"),
                            this.ItemCollection.Items.Count);

                    default:
                        return string.Empty;
                }
            }
        }

        public string OrderTypeAndNumberOfSelectedItems
        {
            get
            {
                switch (this.CurrentOrder)
                {
                    case ContactRerquestsSortOrderType.ORDER_NAME:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByNameMultiSelect"),
                            this.ItemCollection.SelectedItems.Count, this.ItemCollection.Items.Count);

                    default:
                        return string.Empty;
                }
            }
        }

        private ContactRerquestsSortOrderType _currentOrder;
        public ContactRerquestsSortOrderType CurrentOrder
        {
            get { return _currentOrder; }
            set
            {
                SetField(ref _currentOrder, value);

                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems),
                    nameof(this.OrderTypeAndNumberOfSelectedItems));
            }
        }

        /// <summary>
        /// Indicate if the selected outgoing contact requests can be reminded
        /// (two week period since started or last reminder)
        /// </summary>
        public bool IsRemindAllowed
        {
            get
            {
                if (!this._isOutgoing || !this.ItemCollection.HasSelectedItems) return false;
                foreach (var request in this.ItemCollection.SelectedItems)
                {
                    if (!(request as ContactRequestViewModel).IsRemindAllowed)
                        return false;
                }
                return true;
            }
        }

        #endregion
    }
}
