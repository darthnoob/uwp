using System;
using System.IO;
using Windows.Storage;
using Windows.UI;
using mega;
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
        }

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
    }
}
