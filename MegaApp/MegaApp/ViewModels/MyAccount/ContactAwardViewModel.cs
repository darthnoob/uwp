using System;
using System.IO;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using mega;
using MegaApp.Extensions;
using MegaApp.Services;

namespace MegaApp.ViewModels.MyAccount
{
    public class ContactAwardViewModel : BaseViewModel
    {
        public ContactAwardViewModel(MUser user)
        {
            this.MegaUser = user;
            this.Email = user.getEmail();
        }

        public ContactAwardViewModel(string name)
        {
            this.Name = name;
        }

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

        #region Properties

        public MUser MegaUser { get; }

        public bool IsUser => MegaUser != null;

        public string Name { get; }

        private ulong _storageAmount;
        public ulong StorageAmount
        {
            get { return _storageAmount; }
            set { SetField(ref _storageAmount, value); }
        }

        private ulong _transferAmount;
        public ulong TransferAmount
        {
            get { return _transferAmount; }
            set { SetField(ref _transferAmount, value); }
        }

        private string _fistName;
        public string FirstName
        {
            get { return _fistName; }
            set
            {
                SetField(ref _fistName, value);
                OnPropertyChanged("DisplayName");
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
                OnPropertyChanged("DisplayName");
                OnPropertyChanged("AvatarLetter");
            }
        }

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

        private Color _avatarColor;
        /// <summary>
        /// Background color for the contact avatar in case of the contact has not an avatar image
        /// </summary>
        public Color AvatarColor
        {
            get { return _avatarColor; }
            set { SetField(ref _avatarColor, value); }
        }

        public Color AvatarDisplayColor => IsUser
            ? AvatarColor
            : (Color) Application.Current.Resources["MegaAppForeground"];

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

        public string StorageAmountText => StorageAmount.ToStringAndSuffix();
        public string TransferAmountText => TransferAmount.ToStringAndSuffix();

        public bool IsTransferAmountVisible => TransferAmount > 0;

        public Brush ForegroundColor => IsUser
            ? Application.Current.Resources["MegaAppForegroundBrush"] as SolidColorBrush
            : Application.Current.Resources["MegaAppBackgroundBrush"] as SolidColorBrush;


        public string DisplayName => IsUser
            ? $"{FirstName} {LastName}"
            : Name;

        public string AvatarLetter => IsUser
            ? string.IsNullOrWhiteSpace(DisplayName) 
                ? Email.Substring(0, 1).ToUpper()
                : DisplayName.Substring(0, 1).ToUpper()
            : "M";
            
            

        #endregion

    }
}
