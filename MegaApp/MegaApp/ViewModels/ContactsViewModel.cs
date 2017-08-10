using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views.Dialogs;

namespace MegaApp.ViewModels
{
    public class ContactsViewModel : BaseSdkViewModel
    {
        public ContactsViewModel()
        {
            this.AddContactCommand = new RelayCommand(AddContact);

            this.MegaContactsList = new CollectionViewModel<IMegaContact>();
            this.MegaContactsList.Items.CollectionChanged += (sender,args) => 
                OnPropertyChanged("MegaContactsList");

            this.IncomingContactRequestList = new CollectionViewModel<IMegaContactRequest>();
            this.IncomingContactRequestList.Items.CollectionChanged += (sender, args) => 
                OnPropertyChanged("IncomingContactRequestList");

            this.OutgoingContactRequestList = new CollectionViewModel<IMegaContactRequest>();
            this.OutgoingContactRequestList.Items.CollectionChanged += (sender, args) => 
                OnPropertyChanged("OutgoingContactRequestList");
        }

        #region Commands

        public ICommand AddContactCommand { get; }

        #endregion

        #region Public Methods

        public void Initialize(GlobalListener globalListener)
        {
            this.GetMegaContacts();
            this.GetIncomingContactRequests();
            this.GetOutgoingContactRequests();

            if (globalListener == null) return;
            globalListener.ContactUpdated += this.OnContactUpdated;
            globalListener.IncomingContactRequestUpdated += (sender, args) => this.GetIncomingContactRequests();
            globalListener.OutgoingContactRequestUpdated += (sender, args) => this.GetOutgoingContactRequests();
        }

        public void Deinitialize(GlobalListener globalListener)
        {
            if (globalListener == null) return;
            globalListener.ContactUpdated -= this.OnContactUpdated;
            globalListener.IncomingContactRequestUpdated -= (sender, args) => this.GetIncomingContactRequests();
            globalListener.OutgoingContactRequestUpdated -= (sender, args) => this.GetOutgoingContactRequests();
        }

        #endregion

        #region Private Methods

        private async void AddContact()
        {
            var addContactDialog = new AddContactDialog();
            await addContactDialog.ShowAsync();

            if(addContactDialog.DialogResult)
            {
                var inviteContact = new InviteContactRequestListenerAsync();
                var result = await inviteContact.ExecuteAsync(() =>
                    SdkService.MegaSdk.inviteContact(addContactDialog.ContactEmail, addContactDialog.EmailContent,
                        MContactRequestInviteActionType.INVITE_ACTION_ADD, inviteContact));

                switch(result)
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
            var existingContact = MegaContactsList.Items.FirstOrDefault(
                contact => contact.Handle.Equals(user.getHandle()));

            // If the contact exists in the contact list
            if (existingContact != null)
            {
                //If the contact is no longer a contact(REMOVE CONTACT SCENARIO)
                if (!existingContact.Visibility.Equals(user.getVisibility()) &&
                    !(user.getVisibility().Equals(MUserVisibility.VISIBILITY_VISIBLE)))
                {
                    OnUiThread(() => MegaContactsList.Items.Remove(existingContact));
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
                var megaContact = new ContactViewModel(user);

                OnUiThread(() => MegaContactsList.Items.Add(megaContact));

                this.GetContactFirstname(megaContact);
                this.GetContactLastname(megaContact);
                this.GetContactAvatarColor(megaContact);
                this.GetContactAvatar(megaContact);
            }
        }

        #endregion

        #region Public Methods

        public async void GetMegaContacts()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // Create the option to cancel
            CreateLoadCancelOption();

            await OnUiThreadAsync(() => MegaContactsList.Clear());
            MUserList contactsList = this.MegaSdk.getContacts();

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
                            var megaContact = new ContactViewModel(contactsList.get(i));

                            OnUiThread(() => MegaContactsList.Items.Add(megaContact));

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

        public async void GetIncomingContactRequests()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            await OnUiThreadAsync(() => this.IncomingContactRequestList.Clear());

            var inContactRequest = SdkService.MegaSdk.getIncomingContactRequests();
            var inContactRequestSize = inContactRequest.size();

            for (int i = 0; i < inContactRequestSize; i++)
            {
                // To avoid null values
                if (inContactRequest.get(i) == null) continue;

                var contactRequest = new ContactRequestViewModel(inContactRequest.get(i));
                OnUiThread(()=> this.IncomingContactRequestList.Items.Add(contactRequest));
            }
        }

        public async void GetOutgoingContactRequests()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            await OnUiThreadAsync(() => this.OutgoingContactRequestList.Clear());

            var outContactRequest = SdkService.MegaSdk.getOutgoingContactRequests();
            var outContactRequestSize = outContactRequest.size();

            for (int i = 0; i < outContactRequestSize; i++)
            {
                // To avoid null values
                if (outContactRequest.get(i) == null) continue;

                var contactRequest = new ContactRequestViewModel(outContactRequest.get(i));
                OnUiThread(() => this.OutgoingContactRequestList.Items.Add(contactRequest));
            }
        }

        #endregion

        #region Properties

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        private CancellationToken LoadingCancelToken { get; set; }

        private CollectionViewModel<IMegaContact> _megaContactsList;
        public CollectionViewModel<IMegaContact> MegaContactsList
        {
            get { return _megaContactsList; }
            set { SetField(ref _megaContactsList, value); }
        }

        private CollectionViewModel<IMegaContactRequest> _incomingContactRequestList;
        public CollectionViewModel<IMegaContactRequest> IncomingContactRequestList
        {
            get { return _incomingContactRequestList; }
            set { SetField(ref _incomingContactRequestList, value); }
        }

        private CollectionViewModel<IMegaContactRequest> _outgoingContactRequestList;
        public CollectionViewModel<IMegaContactRequest> OutgoingContactRequestList
        {
            get { return _outgoingContactRequestList; }
            set { SetField(ref _outgoingContactRequestList, value); }
        }

        #endregion

        #region UiResources

        public string ContactsTitle => ResourceService.UiResources.GetString("UI_Contacts");
        public string IncomingTitle => ResourceService.UiResources.GetString("UI_Incoming");
        public string OutgoingTitle => ResourceService.UiResources.GetString("UI_Outgoing");

        public string AddContactText => ResourceService.UiResources.GetString("UI_AddContact");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");

        public string EmptyContactsHeaderText => ResourceService.EmptyStates.GetString("ES_ContactsHeader");
        public string EmptyContactsSubHeaderText => ResourceService.EmptyStates.GetString("ES_ContactsSubHeader");

        #endregion

        #region VisualResources

        public string AddContactPathData => ResourceService.VisualResources.GetString("VR_AddContactPathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");

        #endregion
    }
}
