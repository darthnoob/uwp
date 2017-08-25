using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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
            this.ViewState = ContactsViewState.Contacts;
            this.List = new CollectionViewModel<IMegaContact>();

            this.AddContactCommand = new RelayCommand(AddContact);
            this.RemoveContactCommand = new RelayCommand(RemoveContact);

            this.CurrentOrder = ContactsSortOptions.NameAscending;
        }

        #region Commands

        public ICommand AddContactCommand { get; }
        public ICommand RemoveContactCommand { get; }
        
        #endregion

        #region Methods

        public void Initialize(GlobalListener globalListener)
        {
            this.GetMegaContacts();

            if (App.GlobalListener == null) return;
            globalListener.ContactUpdated += this.OnContactUpdated;
        }

        public void Deinitialize(GlobalListener globalListener)
        {
            if (globalListener == null) return;
            globalListener.ContactUpdated -= this.OnContactUpdated;
        }

        private void AddContact()
        {
            this.OnAddContactTapped();
        }

        private async void RemoveContact()
        {
            if (!this.List.HasSelectedItems) return;

            int count = this.List.SelectedItems.Count;

            var dialogResult = await DialogService.ShowOkCancelAndWarningAsync(
                this.RemoveContactText,
                string.Format(ResourceService.AppMessages.GetString("AM_RemoveMultipleContactsQuestion"), count),
                ResourceService.AppMessages.GetString("AM_RemoveContactWarning"),
                this.RemoveText, this.CancelText);

            if (!dialogResult) return;

            // Use a temp variable to avoid InvalidOperationException
            RemoveMultipleContacts(this.List.SelectedItems.ToList());
        }

        private async void RemoveMultipleContacts(ICollection<IMegaContact> contacts)
        {
            if (contacts?.Count < 1) return;

            bool result = true;
            foreach (var contact in contacts)
            {
                result = result & (await contact.RemoveContactAsync());
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

            await OnUiThreadAsync(() => this.List.Clear());
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

                        if ((contactsList.get(i).getVisibility() == MUserVisibility.VISIBILITY_VISIBLE))
                        {
                            var megaContact = new ContactViewModel(contactsList.get(i), this);

                            OnUiThread(() => this.List.Items.Add(megaContact));

                            this.GetContactFirstname(megaContact);
                            this.GetContactLastname(megaContact);
                            this.GetContactAvatarColor(megaContact);
                            this.GetContactAvatar(megaContact);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just exit this background process because a cancellation exception has been thrown
                }

            }, LoadingCancelToken, TaskCreationOptions.PreferFairness, TaskScheduler.Current);

            this.SortBy(this.CurrentOrder);
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
            var existingContact = this.List.Items.FirstOrDefault(
                contact => contact.Handle.Equals(user.getHandle()));

            // If the contact exists in the contact list
            if (existingContact != null)
            {
                //If the contact is no longer a contact(REMOVE CONTACT SCENARIO)
                if (!existingContact.Visibility.Equals(user.getVisibility()) &&
                    !(user.getVisibility().Equals(MUserVisibility.VISIBILITY_VISIBLE)))
                {
                    OnUiThread(() => this.List.Items.Remove(existingContact));
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

                OnUiThread(() => this.List.Items.Add(megaContact));

                this.GetContactFirstname(megaContact);
                this.GetContactLastname(megaContact);
                this.GetContactAvatarColor(megaContact);
                this.GetContactAvatar(megaContact);
            }
        }

        public void SortBy(ContactsSortOptions sortOption)
        {
            switch (sortOption)
            {
                case ContactsSortOptions.NameAscending:
                    OnUiThread(() =>
                    {
                        this.List.Items = new ObservableCollection<IMegaContact>(
                            this.List.Items.OrderBy(item => item.FullName ?? item.Email));
                    });
                    break;

                case ContactsSortOptions.NameDescending:
                    OnUiThread(() =>
                    {
                        this.List.Items = new ObservableCollection<IMegaContact>(
                            this.List.Items.OrderByDescending(item => item.FullName ?? item.Email));
                    });
                    break;

                default:
                    return;
            }
        }

        #endregion

        #region Properties

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        private CancellationToken LoadingCancelToken { get; set; }

        #endregion

        #region UiResources

        public string RemoveContactText => ResourceService.UiResources.GetString("UI_RemoveContact");
        public string RemoveMultipleContactsText => ResourceService.UiResources.GetString("UI_RemoveMultipleContacts");
        
        private string RemoveText => ResourceService.UiResources.GetString("UI_Remove");

        public string EmptyContactsHeaderText => ResourceService.EmptyStates.GetString("ES_ContactsHeader");
        public string EmptyContactsSubHeaderText => ResourceService.EmptyStates.GetString("ES_ContactsSubHeader");

        #endregion

        #region VisualResources

        public string RemovePathData => ResourceService.VisualResources.GetString("VR_RemovePathData");

        #endregion
    }
}
