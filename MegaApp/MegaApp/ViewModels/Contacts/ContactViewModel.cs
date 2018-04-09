using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media.Imaging;
using mega;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactViewModel : BaseSdkViewModel, IMegaContact
    {
        public ContactViewModel(MUser contact, ContactsListViewModel contactList) : base(SdkService.MegaSdk)
        {
            this.MegaUser = contact;
            this.ContactList = contactList;
            
            this.Handle = contact.getHandle();
            this.Email = contact.getEmail();
            this.Timestamp = contact.getTimestamp();
            this.Visibility = contact.getVisibility();
            this.AvatarColor = UiService.GetColorFromHex(this.MegaSdk.getUserAvatarColor(contact));
            this.SharedItems = new ContactSharedItemsViewModel(contact);

            this.RemoveContactCommand = new RelayCommand(RemoveContact);
            this.ViewProfileCommand = new RelayCommand(ViewProfile);
        }

        #region Commands

        public ICommand RemoveContactCommand { get; }
        public ICommand ViewProfileCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the contact first name attribute
        /// </summary>
        public async void GetContactFirstname()
        {
            var firstName = await ContactsService.GetContactFirstName(this.MegaUser);
            UiService.OnUiThread(() => this.FirstName = firstName);
        }

        /// <summary>
        /// Gets the contact last name attribute
        /// </summary>
        public async void GetContactLastname()
        {
            var lastName = await ContactsService.GetContactLastName(this.MegaUser);
            UiService.OnUiThread(() => this.LastName = lastName);
        }

        /// <summary>
        /// Gets the contact avatar color
        /// </summary>
        public void GetContactAvatarColor()
        {
            var avatarColor = UiService.GetColorFromHex(this.MegaSdk.getUserAvatarColor(this.MegaUser));
            UiService.OnUiThread(() => this.AvatarColor = avatarColor);
        }

        /// <summary>
        /// Gets the contact avatar
        /// </summary>
        public async void GetContactAvatar()
        {
            var contactAvatarRequestListener = new GetUserAvatarRequestListenerAsync();
            var contactAvatarResult = await contactAvatarRequestListener.ExecuteAsync(() =>
                this.MegaSdk.getUserAvatar(this.MegaUser, this.AvatarPath, contactAvatarRequestListener));

            if (contactAvatarResult)
            {
                UiService.OnUiThread(() =>
                {
                    var img = new BitmapImage()
                    {
                        CreateOptions = BitmapCreateOptions.IgnoreImageCache,
                        UriSource = new Uri(this.AvatarPath)
                    };
                    this.AvatarUri = img.UriSource;
                });
            }
            else
            {
                UiService.OnUiThread(() => this.AvatarUri = null);
            }
        }

        /// <summary>
        /// View the profile of the contact
        /// </summary>
        public void ViewProfile()
        {
            var contactListItemCollection = this.ContactList?.ItemCollection;
            if (contactListItemCollection == null || contactListItemCollection.IsMultiSelectActive) return;
            if (this.ContactList.OpenContactProfileCommand.CanExecute(null))
                this.ContactList.OpenContactProfileCommand.Execute(null);
        }

        /// <summary>
        /// Share a folder with the contact
        /// </summary>
        public void ShareFolder()
        {
            
        }

        private async void RemoveContact()
        {
            var contactListItemCollection = this.ContactList?.ItemCollection;
            if (contactListItemCollection != null && contactListItemCollection.IsMultiSelectActive)
            {
                if (this.ContactList.RemoveContactCommand.CanExecute(null))
                    this.ContactList.RemoveContactCommand.Execute(null);
                return;
            }

            await RemoveContactAsync();
        }

        /// <summary>
        /// Remove the contact from the contact list
        /// </summary>
        /// <param name="isMultiSelect">True if the contact is in a multi-select scenario</param>
        /// <returns>Result of the action</returns>
        public async Task<bool> RemoveContactAsync(bool isMultiSelect = false)
        {
            // User must be online to perform this operation
            if (!await IsUserOnlineAsync()) return false;

            if (this.MegaUser == null) return false;

            if(!isMultiSelect)
            {
                var dialogResult = await DialogService.ShowOkCancelAsync(
                    this.RemoveContactText,
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveContactQuestion"), this.Email),
                    this.RemoveContactWarningText,
                    OkCancelDialogButtons.Custom, this.RemoveText, this.CancelText);

                if (!dialogResult) return true;
            }

            var removeContact = new RemoveContactRequestListenerAsync();
            var result = await removeContact.ExecuteAsync(() =>
                this.MegaSdk.removeContact(this.MegaUser, removeContact));

            if (result) return true;

            LogService.Log(MLogLevel.LOG_LEVEL_ERROR,
                string.Format("Error removing the contact {0}", this.Email));
            if (!isMultiSelect)
            {
                await DialogService.ShowAlertAsync(this.RemoveContactText,
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveContactFailed"), this.Email));
            }

            return false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Original MUser from the Mega SDK that is the base of the contact
        /// </summary>
        public MUser MegaUser { get; private set; }

        /// <summary>
        /// Unique identifier of the contact
        /// </summary>
        public ulong Handle { get; private set; }

        /// <summary>
        /// Timestamp when the contact was added to the contact list (in seconds since the epoch)
        /// </summary>
        public ulong Timestamp { get; private set; }

        /// <summary>
        /// Visibility of the contact
        /// </summary>
        public MUserVisibility Visibility { get; private set; }

        private string _email;
        /// <summary>
        /// Email associated with the contact
        /// </summary>
        public string Email
        {
            get { return _email; }
            set
            {
                SetField(ref _email, value);
                OnPropertyChanged("AvatarLetter");
            }
        }

        private string _fistName;
        /// <summary>
        /// Firstname of the contact
        /// </summary>
        public string FirstName
        {
            get { return _fistName; }
            set
            {
                SetField(ref _fistName, value);
                OnPropertyChanged("FullName");
                OnPropertyChanged("AvatarLetter");
            }
        }

        private string _lastName;
        /// <summary>
        /// Lastname of the contact
        /// </summary>
        public string LastName
        {
            get { return _lastName; }
            set
            {
                SetField(ref _lastName, value);
                OnPropertyChanged("FullName");
                OnPropertyChanged("AvatarLetter");
            }
        }

        /// <summary>
        /// Full name of the contact
        /// </summary>
        public string FullName => string.Format(FirstName + " " + LastName);

        /// <summary>
        /// Avatar letter for the contact avatar in case of the contact has not an avatar image
        /// </summary>
        public string AvatarLetter => string.IsNullOrWhiteSpace(FullName) ?
            Email.Substring(0, 1).ToUpper() : FullName.Substring(0, 1).ToUpper();

        private Color _avatarColor;
        /// <summary>
        /// Background color for the contact avatar in case of the contact has not an avatar image
        /// </summary>
        public Color AvatarColor
        {
            get { return _avatarColor; }
            set { SetField(ref _avatarColor, value); }
        }

        private Uri _avatarUri;
        /// <summary>
        /// The uniform resource identifier of the avatar image of the contact
        /// </summary>
        public Uri AvatarUri
        {
            get { return _avatarUri; }
            set { SetField(ref _avatarUri, value); }
        }

        /// <summary>
        /// Returns the path to store the contact avatar image
        /// </summary>
        public string AvatarPath => string.IsNullOrWhiteSpace(Email) ? null :
            Path.Combine(ApplicationData.Current.LocalFolder.Path, 
                ResourceService.AppResources.GetString("AR_ThumbnailsDirectory"), Email);

        private ContactSharedItemsViewModel _sharedItems;
        /// <summary>
        /// Folders shared with or by the contact
        /// </summary>
        public ContactSharedItemsViewModel SharedItems
        {
            get { return _sharedItems; }
            set { SetField(ref _sharedItems, value); }
        }

        private ContactsListViewModel _contactList;
        public ContactsListViewModel ContactList
        {
            get { return _contactList; }
            set { SetField(ref _contactList, value); }
        }

        private bool _isMultiSelected;
        /// <summary>
        /// Indicates if the contact is currently selected in a multi-select scenario
        /// Needed as path for the ListView to auto select/deselect
        /// </summary>
        public bool IsMultiSelected
        {
            get { return _isMultiSelected; }
            set { SetField(ref _isMultiSelected, value); }
        }

        #region Achievement properties

        private long _storageAmount;
        public long StorageAmount
        {
            get { return _storageAmount; }
            set
            {
                SetField(ref _storageAmount, value);
                OnPropertyChanged(nameof(StorageAmountText));
            }
        }

        private long _transferAmount;
        public long TransferAmount
        {
            get { return _transferAmount; }
            set
            {
                SetField(ref _transferAmount, value);
                OnPropertyChanged(nameof(TransferAmountText));
            }
        }

        public string StorageAmountText => ((ulong)StorageAmount).ToStringAndSuffix();

        public string TransferAmountText => ((ulong)TransferAmount).ToStringAndSuffix();

        private DateTime? _referralBonusExpireDate;
        public DateTime? ReferralBonusExpireDate
        {
            get { return _referralBonusExpireDate; }
            set { SetField(ref _referralBonusExpireDate, value); }
        }

        public bool IsReferralBonusExpired => 
            ReferralBonusExpireDate.HasValue && ReferralBonusExpireDate <= DateTime.Now;

        public int ReferralBonusExpiresIn => 
            ReferralBonusExpireDate?.Subtract(DateTime.Today).Days ?? -1;

        public string ReferralBonusDaysRemaining => AccountService.GetDaysRemaining(ReferralBonusExpiresIn);

        private MContactRequestStatusType StatusType => this.MegaSdk.getContactRequestByHandle(this.Handle) != null
            ? (MContactRequestStatusType) this.MegaSdk.getContactRequestByHandle(this.Handle).getStatus()
            : MContactRequestStatusType.STATUS_ACCEPTED;

        public string ReferralStatus
        {
            get
            {
                switch (this.StatusType)
                {
                    case MContactRequestStatusType.STATUS_UNRESOLVED:
                        return "Pending";
                    case MContactRequestStatusType.STATUS_ACCEPTED:
                        return this.ReferralBonusDaysRemaining;
                    case MContactRequestStatusType.STATUS_DENIED:                        
                    case MContactRequestStatusType.STATUS_IGNORED:
                    case MContactRequestStatusType.STATUS_DELETED:
                    case MContactRequestStatusType.STATUS_REMINDED:
                        return string.Empty;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public int ReferralStatusOrder => GetReferralStatusOrder();

        private int GetReferralStatusOrder()
        {
            switch (this.StatusType)
            {
                case MContactRequestStatusType.STATUS_UNRESOLVED:
                    return 1;
                case MContactRequestStatusType.STATUS_ACCEPTED:
                    return IsReferralBonusExpired ? 3 : 0;
                case MContactRequestStatusType.STATUS_DENIED:
                case MContactRequestStatusType.STATUS_IGNORED:
                case MContactRequestStatusType.STATUS_DELETED:
                case MContactRequestStatusType.STATUS_REMINDED:
                    return 2;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #endregion

        #region UiResources

        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string ProfileText => ResourceService.UiResources.GetString("UI_Profile");
        public string RemoveText => ResourceService.UiResources.GetString("UI_Remove");
        public string RemoveContactText => ResourceService.UiResources.GetString("UI_RemoveContact");
        public string RemoveContactWarningText => ResourceService.AppMessages.GetString("AM_RemoveContactWarning");
        public string RemoveMultipleContactsText => ResourceService.UiResources.GetString("UI_RemoveMultipleContacts");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");
        public string SharedItemsText => ResourceService.UiResources.GetString("UI_SharedItems");
        public string ViewProfileText => ResourceService.UiResources.GetString("UI_ViewProfile");

        #endregion

        #region VisualResources

        public string ShareIconPathData => ResourceService.VisualResources.GetString("VR_ShareIconPathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");
        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }
}
