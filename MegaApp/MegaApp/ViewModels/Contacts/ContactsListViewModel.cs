using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactsListViewModel : ContactsBaseViewModel<IMegaContact>
    {
        public ContactsListViewModel()
        {
            this.ContentType = ContactsContentType.Contacts;
            this.ItemCollection = new CollectionViewModel<IMegaContact>();

            this.AddContactCommand = new RelayCommand(AddContact);
            this.RemoveContactCommand = new RelayCommand(RemoveContact);
            this.OpenContactProfileCommand = new RelayCommand(OpenContactProfile);
            this.CloseContactProfileCommand = new RelayCommand(CloseContactProfile);

            this.CurrentOrder = ContactsSortOrderType.ORDER_NAME;
        }

        #region Events

        /// <summary>
        /// Event triggered when the 'view profile' option is tapped
        /// </summary>
        public event EventHandler OpenContactProfileEvent;

        /// <summary>
        /// Event invocator method called when the 'view profile' option is tapped
        /// </summary>
        protected virtual void OnOpenContactProfile()
        {
            this.OpenContactProfileEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the 'close profile panel' option is tapped
        /// </summary>
        public event EventHandler CloseContactProfileEvent;

        /// <summary>
        /// Event invocator method called when the 'close profile panel' option is tapped
        /// </summary>
        protected virtual void OnCloseContactProfile()
        {
            this.CloseContactProfileEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        public void Initialize(GlobalListener globalListener)
        {
            this.ItemCollection.ItemCollectionChanged += OnItemCollectionChanged;
            this.ItemCollection.SelectedItemsCollectionChanged += OnSelectedItemsCollectionChanged;

            this.ItemCollection.OrderInverted += OnOrderInverted;

            this.GetMegaContacts();

            if (App.GlobalListener == null) return;
            globalListener.ContactUpdated += this.OnContactUpdated;
        }

        public void Deinitialize(GlobalListener globalListener)
        {
            this.ItemCollection.ItemCollectionChanged -= OnItemCollectionChanged;
            this.ItemCollection.SelectedItemsCollectionChanged -= OnSelectedItemsCollectionChanged;

            this.ItemCollection.OrderInverted -= OnOrderInverted;

            if (globalListener == null) return;
            globalListener.ContactUpdated -= this.OnContactUpdated;
        }

        private void AddContact()
        {
            this.OnAddContactTapped();
        }

        private async void RemoveContact()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            if (this.ItemCollection.OnlyOneSelectedItem)
            {
                var contact = this.ItemCollection.SelectedItems.First();
                await contact.RemoveContactAsync();
            }
            else
            {
                int count = this.ItemCollection.SelectedItems.Count;

                var dialogResult = await DialogService.ShowOkCancelAndWarningAsync(
                    this.RemoveContactText,
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveMultipleContactsQuestion"), count),
                    ResourceService.AppMessages.GetString("AM_RemoveContactWarning"),
                    this.RemoveText, this.CancelText);

                if (!dialogResult) return;

                // Use a temp variable to avoid InvalidOperationException
                RemoveMultipleContacts(this.ItemCollection.SelectedItems.ToList());
            }
        }

        private async void RemoveMultipleContacts(ICollection<IMegaContact> contacts)
        {
            if (contacts?.Count < 1) return;

            bool result = true;
            if (contacts != null)
            {
                foreach (var contact in contacts)
                {
                    result = result & await contact.RemoveContactAsync(true);
                }
            }

            if (!result)
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(this.RemoveContactText,
                        ResourceService.AppMessages.GetString("AM_RemoveMultipleContactsFailed"));
                });
            }
        }

        public async void GetMegaContacts()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // Create the option to cancel
            CreateLoadCancelOption();

            await OnUiThreadAsync(() => this.ItemCollection.Clear());
            MUserList contactsList = SdkService.MegaSdk.getContacts();

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    for (int i = 0; i < contactsList.size(); i++)
                    {
                        // If the task has been cancelled, stop processing
                        if (LoadingCancelToken.IsCancellationRequested)
                            LoadingCancelToken.ThrowIfCancellationRequested();

                        // To avoid null values
                        if (contactsList.get(i) == null) continue;

                        if (contactsList.get(i).getVisibility() != MUserVisibility.VISIBILITY_VISIBLE) continue;

                        var megaContact = new ContactViewModel(contactsList.get(i), this);

                        OnUiThread(() => this.ItemCollection.Items.Add(megaContact));

                        this.GetContactFirstname(megaContact);
                        this.GetContactLastname(megaContact);
                        this.GetContactAvatarColor(megaContact);
                        this.GetContactAvatar(megaContact);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just exit this background process because a cancellation exception has been thrown
                }

            }, LoadingCancelToken, TaskCreationOptions.PreferFairness, TaskScheduler.Current);

            this.SortBy(this.CurrentOrder, this.ItemCollection.CurrentOrderDirection);
        }

        /// <summary>
        /// Cancel any running load process of contacts
        /// </summary>
        public void CancelLoad()
        {
            if (this.LoadingCancelTokenSource != null && LoadingCancelToken.CanBeCanceled)
                LoadingCancelTokenSource.Cancel();
        }

        private void CreateLoadCancelOption()
        {
            if (this.LoadingCancelTokenSource != null)
            {
                this.LoadingCancelTokenSource.Dispose();
                this.LoadingCancelTokenSource = null;
            }
            this.LoadingCancelTokenSource = new CancellationTokenSource();
            this.LoadingCancelToken = LoadingCancelTokenSource.Token;
        }

        /// <summary>
        /// Gets the contact first name attribute
        /// </summary>
        private async void GetContactFirstname(IMegaContact contact)
        {
            var contactAttributeRequestListener = new GetUserAttributeRequestListenerAsync();
            var firstName = await contactAttributeRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getUserAttribute(contact.MegaUser,
                (int)MUserAttrType.USER_ATTR_FIRSTNAME, contactAttributeRequestListener));
            UiService.OnUiThread(() => contact.FirstName = firstName);
        }

        /// <summary>
        /// Gets the contact last name attribute
        /// </summary>
        private async void GetContactLastname(IMegaContact contact)
        {
            var contactAttributeRequestListener = new GetUserAttributeRequestListenerAsync();
            var lastName = await contactAttributeRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getUserAttribute(contact.MegaUser,
                (int)MUserAttrType.USER_ATTR_LASTNAME, contactAttributeRequestListener));
            UiService.OnUiThread(() => contact.LastName = lastName);
        }

        /// <summary>
        /// Gets the contact avatar color
        /// </summary>
        private void GetContactAvatarColor(IMegaContact contact)
        {
            var avatarColor = UiService.GetColorFromHex(SdkService.MegaSdk.getUserAvatarColor(contact.MegaUser));
            UiService.OnUiThread(() => contact.AvatarColor = avatarColor);
        }

        /// <summary>
        /// Gets the contact avatar
        /// </summary>
        private async void GetContactAvatar(IMegaContact contact)
        {
            var contactAvatarRequestListener = new GetUserAvatarRequestListenerAsync();
            var contactAvatarResult = await contactAvatarRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getUserAvatar(contact.MegaUser, contact.AvatarPath, contactAvatarRequestListener));

            if (contactAvatarResult)
            {
                UiService.OnUiThread(() =>
                {
                    var img = new BitmapImage()
                    {
                        CreateOptions = BitmapCreateOptions.IgnoreImageCache,
                        UriSource = new Uri(contact.AvatarPath)
                    };
                    contact.AvatarUri = img.UriSource;
                });
            }
            else
            {
                UiService.OnUiThread(() => contact.AvatarUri = null);
            }
        }

        private void OnContactUpdated(object sender, MUser user)
        {
            var existingContact = this.ItemCollection.Items.FirstOrDefault(
                contact => contact.Handle.Equals(user.getHandle()));

            // If the contact exists in the contact list
            if (existingContact != null)
            {
                //If the contact is no longer a contact(REMOVE CONTACT SCENARIO)
                if (!existingContact.Visibility.Equals(user.getVisibility()) &&
                    !(user.getVisibility().Equals(MUserVisibility.VISIBILITY_VISIBLE)))
                {
                    OnUiThread(() => this.ItemCollection.Items.Remove(existingContact));
                }
                // If the contact has been changed (UPDATE CONTACT SCENARIO) and is not an own change
                else if (!Convert.ToBoolean(user.isOwnChange()))
                {
                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_AVATAR) &&
                        !string.IsNullOrWhiteSpace(existingContact.AvatarPath))
                    {
                        this.GetContactAvatar(existingContact);
                    }

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_EMAIL))
                        OnUiThread(() => existingContact.Email = user.getEmail());

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_FIRSTNAME))
                        this.GetContactFirstname(existingContact);

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_LASTNAME))
                        this.GetContactLastname(existingContact);
                }
            }
            // If is a new contact (ADD CONTACT SCENARIO - REQUEST ACCEPTED)
            else if (user.getVisibility().Equals(MUserVisibility.VISIBILITY_VISIBLE))
            {
                var megaContact = new ContactViewModel(user, this);

                OnUiThread(() => this.ItemCollection.Items.Add(megaContact));

                this.GetContactFirstname(megaContact);
                this.GetContactLastname(megaContact);
                this.GetContactAvatarColor(megaContact);
                this.GetContactAvatar(megaContact);
            }
        }

        public void SortBy(ContactsSortOrderType sortOption, SortOrderDirection sortDirection)
        {
            OnUiThread(() => this.ItemCollection.DisableCollectionChangedDetection());

            switch (sortOption)
            {
                case ContactsSortOrderType.ORDER_NAME:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaContact>(this.ItemCollection.IsCurrentOrderAscending ?
                            this.ItemCollection.Items.OrderBy(item => item.FullName ?? item.Email) :
                            this.ItemCollection.Items.OrderByDescending(item => item.FullName ?? item.Email));
                    });
                    break;

                case ContactsSortOrderType.ORDER_EMAIL:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaContact>(this.ItemCollection.IsCurrentOrderAscending ?
                            this.ItemCollection.Items.OrderBy(item => item.Email) :
                            this.ItemCollection.Items.OrderByDescending(item => item.Email));
                    });
                    break;
            }

            OnUiThread(() => this.ItemCollection.EnableCollectionChangedDetection());

            this.CloseContactProfile();
        }

        private void OnItemCollectionChanged(object sender, EventArgs args) =>
            OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems), nameof(this.OrderTypeAndNumberOfSelectedItems));

        private void OnSelectedItemsCollectionChanged(object sender, EventArgs args) =>
            OnPropertyChanged(nameof(this.OrderTypeAndNumberOfSelectedItems));

        private void OnOrderInverted(object sender, EventArgs args) =>
            this.SortBy(this.CurrentOrder, this.ItemCollection.CurrentOrderDirection);

        private void OpenContactProfile()
        {
            this.OnOpenContactProfile();
        }

        private void CloseContactProfile()
        {
            this.OnCloseContactProfile();
        }

        #endregion

        #region Properties

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        private CancellationToken LoadingCancelToken { get; set; }

        public string OrderTypeAndNumberOfItems
        {
            get
            {
                switch (this.CurrentOrder)
                {
                    case ContactsSortOrderType.ORDER_NAME:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByName"),
                            this.ItemCollection.Items.Count);

                    case ContactsSortOrderType.ORDER_EMAIL:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByEmail"),
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
                    case ContactsSortOrderType.ORDER_NAME:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByNameMultiSelect"),
                            this.ItemCollection.SelectedItems.Count, this.ItemCollection.Items.Count);

                    case ContactsSortOrderType.ORDER_EMAIL:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByEmailMultiSelect"),
                            this.ItemCollection.SelectedItems.Count, this.ItemCollection.Items.Count);

                    default:
                        return string.Empty;
                }
            }
        }

        private ContactsSortOrderType _currentOrder;
        public ContactsSortOrderType CurrentOrder
        {
            get { return _currentOrder; }
            set
            {
                SetField(ref _currentOrder, value);

                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems),
                    nameof(this.OrderTypeAndNumberOfSelectedItems));
            }
        }

        #endregion

        #region UiResources

        public string RemoveMultipleContactsText => ResourceService.UiResources.GetString("UI_RemoveMultipleContacts");
        
        private string RemoveText => ResourceService.UiResources.GetString("UI_Remove");

        public string EmptyContactsHeaderText => ResourceService.EmptyStates.GetString("ES_ContactsHeader");
        public string EmptyContactsSubHeaderText => ResourceService.EmptyStates.GetString("ES_ContactsSubHeader");

        #endregion
    }
}
