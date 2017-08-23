using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactViewModel : BaseSdkViewModel, IMegaContact
    {
        public ContactViewModel(MUser contact, ContactsListViewModel contactList)
        {
            MegaUser = contact;
            Handle = contact.getHandle();
            Email = contact.getEmail();
            Timestamp = contact.getTimestamp();
            Visibility = contact.getVisibility();
            AvatarColor = UiService.GetColorFromHex(SdkService.MegaSdk.getUserAvatarColor(contact));
            InSharesList = SdkService.MegaSdk.getInShares(contact);
            this.ContactList = contactList;

            this.RemoveContactCommand = new RelayCommand(RemoveContact);
        }

        #region Commands

        public ICommand RemoveContactCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// View the profile of the contact
        /// </summary>
        public void ViewProfile()
        {
            
        }

        /// <summary>
        /// Share a folder with the contact
        /// </summary>
        public void ShareFolder()
        {
            
        }

        private async void RemoveContact()
        {
            if (this.ContactList != null && this.ContactList.IsMultiSelectActive)
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
            if (!IsUserOnline()) return false;

            if (this.MegaUser == null) return false;

            if(!isMultiSelect)
            {
                var dialogResult = await DialogService.ShowOkCancelAndWarningAsync(
                    this.RemoveContactText,
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveContactQuestion"), this.Email),
                    ResourceService.AppMessages.GetString("AM_RemoveContactWarning"),
                    this.RemoveText, this.CancelText);

                if (!dialogResult) return true;
            }

            var removeContact = new RemoveContactRequestListenerAsync();
            var result = await removeContact.ExecuteAsync(() =>
                SdkService.MegaSdk.removeContact(this.MegaUser, removeContact));
            if (!result)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR,
                    string.Format("Error removing the contact {0}", this.Email));
                if(!isMultiSelect)
                {
                    await DialogService.ShowAlertAsync(this.RemoveContactText,
                        string.Format(ResourceService.AppMessages.GetString("AM_RemoveContactFailed"), this.Email));
                }
            }

            return result;
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

        private MNodeList _inSharesList;
        /// <summary>
        /// List of folders shared by the contact
        /// </summary>
        public MNodeList InSharesList
        {
            get { return _inSharesList; }
            set
            {
                SetField(ref _inSharesList, value);
                OnPropertyChanged("NumberOfInShares");
                OnPropertyChanged("NumberOfInSharesText");
            }
        }

        /// <summary>
        /// Number of folders shared by the contact
        /// </summary>
        public int NumberOfInShares => InSharesList.size();

        /// <summary>
        /// Number of folders shared by the contact as a formatted text string
        /// </summary>
        public string NumberOfInSharesText => string.Format("{0} {1}", NumberOfInShares, NumberOfInShares == 1 ? 
            ResourceService.UiResources.GetString("UI_SharedFolder").ToLower() : 
            ResourceService.UiResources.GetString("UI_SharedFolders").ToLower());

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

        #endregion

        #region UiResources

        public string RemoveContactText => ResourceService.UiResources.GetString("UI_RemoveContact");
        public string RemoveMultipleContactsText => ResourceService.UiResources.GetString("UI_RemoveMultipleContacts");

        private string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        private string RemoveText => ResourceService.UiResources.GetString("UI_Remove");

        #endregion

        #region VisualResources

        public string IncomingSharedFolderPathData => ResourceService.VisualResources.GetString("VR_IncomingSharedFolderPathData");

        #endregion
    }
}
