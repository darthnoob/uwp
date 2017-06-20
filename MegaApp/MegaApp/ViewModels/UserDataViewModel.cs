using System;
using System.IO;
using Windows.Storage;
using Windows.UI;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class UserDataViewModel : BaseViewModel
    {
        public UserDataViewModel()
        {
            if (string.IsNullOrWhiteSpace(AvatarPath) || !File.Exists(AvatarPath)) return;
            AvatarUri = new Uri(AvatarPath);
        }

        #region Events

        /// <summary>
        /// Event triggered when the user email is changed.
        /// </summary>
        public event EventHandler UserEmailChanged;

        /// <summary>
        /// Event triggered when the user name is changed.
        /// </summary>
        public event EventHandler UserNameChanged;

        #endregion

        #region Properties

        /// <summary>
        /// User email.
        /// </summary>
        private string _userEmail;
        public string UserEmail
        {
            get { return _userEmail; }
            set
            {
                SetField(ref _userEmail, value);
                OnPropertyChanged("AvatarLetter");
                UserEmailChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// User name, composed by the fistname and the lastname.
        /// </summary>
        public string UserName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Firstname) && !string.IsNullOrWhiteSpace(Lastname))
                    return string.Format("{0} {1}", Firstname, Lastname);
                else if (!string.IsNullOrWhiteSpace(Firstname))
                    return Firstname;
                else
                    return ResourceService.UiResources.GetString("UI_MyAccount");
            }
        }
        /// <summary>
        /// User fistname.
        /// </summary>
        private string _firstname;
        public string Firstname
        {
            get { return _firstname; }
            set
            {
                SetField(ref _firstname, value);
                OnPropertyChanged("UserName");
                OnPropertyChanged("AvatarLetter");
                UserNameChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// User lastname.
        /// </summary>
        private string _lastname;
        public string Lastname
        {
            get { return _lastname; }
            set
            {
                SetField(ref _lastname, value);
                OnPropertyChanged("UserName");
                OnPropertyChanged("AvatarLetter");
                UserNameChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// User avatar image.
        /// </summary>
        private Uri _avatarUri;
        public Uri AvatarUri
        {
            get { return _avatarUri; }
            set { SetField(ref _avatarUri, value); }
        }

        /// <summary>
        /// Path of the user avatar image.
        /// </summary>
        public string AvatarPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UserEmail)) return null;

                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    ResourceService.AppResources.GetString("AR_ThumbnailsDirectory"), UserEmail);
            }
        }

        /// <summary>
        /// Avatar letter for the avatar in case of the user has not an avatar image.
        /// </summary>
        public string AvatarLetter
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(UserName))
                    return UserName.Substring(0, 1).ToUpper();
                else if (!string.IsNullOrWhiteSpace(UserEmail))
                    return UserEmail.Substring(0, 1).ToUpper();
                else
                    return "M"; // If no data available, return "M" of MEGA
            }
        }

        /// <summary>
        /// Background color for the avatar in case of the user has not an avatar image.
        /// </summary>
        private Color _avatarColor;
        public Color AvatarColor
        {
            get { return _avatarColor; }
            set { SetField(ref _avatarColor, value); }
        }

        #endregion
    }
}
