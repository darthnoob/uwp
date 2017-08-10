using System;
using System.IO;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI;
using mega;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.Classes
{
    public class ContactViewModel : BaseViewModel
    {
        public ContactViewModel(MUser contact)
        {
            MegaUser = contact;
            Handle = contact.getHandle();
            Email = contact.getEmail();
            Timestamp = contact.getTimestamp();
            Visibility = contact.getVisibility();
            AvatarColor = UiService.GetColorFromHex(SdkService.MegaSdk.getUserAvatarColor(contact));
            InSharesList = SdkService.MegaSdk.getInShares(contact);

            this.RemoveContactCommand = new RelayCommand(RemoveContact);
        }

        #region Commands

        public ICommand RemoveContactCommand { get; }

        #endregion

        #region Private Methods

        private async void RemoveContact()
        {
            var dialogResult = await DialogService.ShowOkCancelAndWarningAsync(
                this.RemoveContactText,
                string.Format(ResourceService.AppMessages.GetString("AM_RemoveContactQuestion"), this.Email),
                ResourceService.AppMessages.GetString("AM_RemoveContactWarning"),
                this.RemoveText, this.CancelText);

            if(dialogResult)
            {
                var removeContact = new RemoveContactRequestListenerAsync();
                var result = await removeContact.ExecuteAsync(() =>
                    SdkService.MegaSdk.removeContact(this.MegaUser, removeContact));
                if(!result)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, 
                        string.Format("Error removing the contact {0}", this.Email));
                    await DialogService.ShowAlertAsync(this.RemoveContactText,
                        string.Format(ResourceService.AppMessages.GetString("AM_RemoveContactFailed"), this.Email));
                }
            }
        }

        #endregion

        #region Properties

        public MUser MegaUser { get; set; }
        public ulong Handle { get; set; }
        public ulong Timestamp { get; set; }
        public MUserVisibility Visibility { get; set; }

        private string _email;
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

        public string FullName => string.Format(FirstName + " " + LastName);

        /// <summary>
        /// Avatar letter for the avatar in case of the contact has not an avatar image.
        /// </summary>
        public string AvatarLetter
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(FullName))
                    return FullName.Substring(0, 1).ToUpper();

                return Email.Substring(0, 1).ToUpper();
            }
        }

        /// <summary>
        /// Background color for the avatar in case of the contact has not an avatar image.
        /// </summary>
        private Color _avatarColor;
        public Color AvatarColor
        {
            get { return _avatarColor; }
            set { SetField(ref _avatarColor, value); }
        }

        private Uri _avatarUri;
        public Uri AvatarUri
        {
            get { return _avatarUri; }
            set { SetField(ref _avatarUri, value); }
        }

        public string AvatarPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Email)) return null;

                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    ResourceService.AppResources.GetString("AR_ThumbnailsDirectory"), Email);
            }
        }

        private MNodeList _inSharesList;
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

        public int NumberOfInShares => InSharesList.size();

        public string NumberOfInSharesText => string.Format("{0} {1}", NumberOfInShares, NumberOfInShares == 1 ? 
            ResourceService.UiResources.GetString("UI_Folder").ToLower() : 
            ResourceService.UiResources.GetString("UI_Folders").ToLower());

        #endregion

        #region UiResources

        public string RemoveContactText => ResourceService.UiResources.GetString("UI_RemoveContact");

        private string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        private string RemoveText => ResourceService.UiResources.GetString("UI_Remove");

        #endregion
    }
}
