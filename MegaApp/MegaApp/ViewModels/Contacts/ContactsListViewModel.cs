using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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
            this.InvertOrderCommand = new RelayCommand(InvertOrder);
            this.OpenContactProfileCommand = new RelayCommand(OpenContactProfile);
            this.CloseContactProfileCommand = new RelayCommand(CloseContactProfile);

            this.CurrentOrder = MSortOrderType.ORDER_ALPHABETICAL_ASC;
        }

        #region Commands

        public override ICommand AddContactCommand { get; }
        public override ICommand RemoveContactCommand { get; }
        public override ICommand OpenContactProfileCommand { get; }
        public override ICommand CloseContactProfileCommand { get; }

        public override ICommand InvertOrderCommand { get; }

        #endregion

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
            foreach (var contact in contacts)
            {
                result = result & (await contact.RemoveContactAsync(true));
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

                        if ((contactsList.get(i).getVisibility() == MUserVisibility.VISIBILITY_VISIBLE))
                        {
                            var megaContact = new ContactViewModel(contactsList.get(i), this);

                            OnUiThread(() => this.ItemCollection.Items.Add(megaContact));

                            megaContact.GetContactFirstname();
                            megaContact.GetContactLastname();
                            megaContact.GetContactAvatarColor();
                            megaContact.GetContactAvatar();
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

        protected void OnContactUpdated(object sender, MUser user)
        {
            var existingContact = (ContactViewModel)this.ItemCollection.Items.FirstOrDefault(
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
                        existingContact.GetContactAvatar();
                    }

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_EMAIL))
                        OnUiThread(() => existingContact.Email = user.getEmail());

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_FIRSTNAME))
                        existingContact.GetContactFirstname();

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_LASTNAME))
                        existingContact.GetContactLastname();
                }
            }
            // If is a new contact (ADD CONTACT SCENARIO - REQUEST ACCEPTED)
            else if (user.getVisibility().Equals(MUserVisibility.VISIBILITY_VISIBLE))
            {
                var megaContact = new ContactViewModel(user, this);

                OnUiThread(() => this.ItemCollection.Items.Add(megaContact));

                megaContact.GetContactFirstname();
                megaContact.GetContactLastname();
                megaContact.GetContactAvatarColor();
                megaContact.GetContactAvatar();
            }
        }

        public void SortBy(MSortOrderType sortOption)
        {
            switch (sortOption)
            {
                case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaContact>(
                            this.ItemCollection.Items.OrderBy(item => item.FullName ?? item.Email));
                    });
                    break;

                case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaContact>(
                            this.ItemCollection.Items.OrderByDescending(item => item.FullName ?? item.Email));
                    });
                    break;

                default:
                    return;
            }

            this.CloseContactProfile();
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

        #endregion

        #region UiResources

        public string RemoveMultipleContactsText => ResourceService.UiResources.GetString("UI_RemoveMultipleContacts");
        
        private string RemoveText => ResourceService.UiResources.GetString("UI_Remove");

        public string EmptyContactsHeaderText => ResourceService.EmptyStates.GetString("ES_ContactsHeader");
        public string EmptyContactsSubHeaderText => ResourceService.EmptyStates.GetString("ES_ContactsSubHeader");

        #endregion
    }
}
