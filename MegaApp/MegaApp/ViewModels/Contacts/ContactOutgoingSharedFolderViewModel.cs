using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactOutgoingSharedFolderViewModel : ContactViewModel, IMegaContactOutgoingSharedFolder
    {
        /// <summary>
        /// Constructor of the <see cref="ContactOutgoingSharedFolderViewModel"/>
        /// </summary>
        /// <param name="outShare">Folder outbound sharing with the contact</param>
        /// <param name="contactList">List that contains the contact</param>
        public ContactOutgoingSharedFolderViewModel(MShare outShare, ContactsListViewModel contactList)
            : base(SdkService.MegaSdk.getContact(outShare.getUser()), contactList)
        {
            this._outShare = outShare;

            this.AccessLevel = new AccessLevelViewModel();
            this.GetAccesLevel(outShare);

            this.ChangePermissionsCommand = new RelayCommand<MShareType>(ChangePermissions);
            this.RemoveContactFromFolderCommand = new RelayCommand(RemoveContactFromFolder);
        }

        #region Commands

        public ICommand ChangePermissionsCommand { get; }
        public ICommand RemoveContactFromFolderCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Get the contact access level to the outgoing shared folder
        /// </summary>
        /// <param name="outShare">Folder outbound sharing with the contact</param>
        public void GetAccesLevel(MShare outShare)
        {
            OnUiThread(() => this.AccessLevel.AccessType = (MShareType)outShare.getAccess());
        }

        /// <summary>
        /// Modify the contact permissions over an outgoing shared folder
        /// </summary>
        /// <param name="newAccessLevel">New access level</param>
        private async void ChangePermissions(MShareType newAccessLevel)
        {
            var contactList = this.ContactList as ContactsListOutgoingSharedFolderViewModel;

            if (contactList?.ItemCollection != null && contactList.ItemCollection.IsMultiSelectActive)
            {
                if (contactList.ChangePermissionsCommand.CanExecute(newAccessLevel))
                    contactList.ChangePermissionsCommand.Execute(newAccessLevel);
                return;
            }

            await this.ChangePermissionsAsync(newAccessLevel);
        }

        /// <summary>
        /// Modify the contact permissions over an outgoing shared folder
        /// </summary>
        /// <param name="newAccessLevel">New access level</param>
        /// <returns>Result of the action</returns>
        public async Task<bool> ChangePermissionsAsync(MShareType newAccessLevel)
        {
            var changePermissions = new ShareRequestListenerAsync();
            var result = await changePermissions.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.shareByEmail(
                    SdkService.MegaSdk.getNodeByHandle(this._outShare.getNodeHandle()),
                    this.Email, (int)newAccessLevel, changePermissions);
            });

            return result;
        }

        /// <summary>
        /// Remove contact access from an outgoing shared folder
        /// </summary>
        private async void RemoveContactFromFolder()
        {
            var contactList = this.ContactList as ContactsListOutgoingSharedFolderViewModel;

            if (contactList?.ItemCollection != null && contactList.ItemCollection.IsMultiSelectActive)
            {
                if (contactList.RemoveContactFromFolderCommand.CanExecute(null))
                    contactList.RemoveContactFromFolderCommand.Execute(null);
                return;
            }

            await RemoveContactFromFolderAsync();
        }

        /// <summary>
        /// Remove contact access from an outgoing shared folder
        /// </summary>
        /// <returns>Result of the action</returns>
        public async Task<bool> RemoveContactFromFolderAsync() =>
            await this.ChangePermissionsAsync(MShareType.ACCESS_UNKNOWN);

        #endregion

        #region Properties

        /// <summary>
        /// Folder outbound sharing with the contact
        /// </summary>
        private readonly MShare _outShare;

        private AccessLevelViewModel _accessLevel;
        /// <summary>
        /// Access level of the contact to the outgoing shared folder
        /// </summary>
        public AccessLevelViewModel AccessLevel
        {
            get { return _accessLevel; }
            set { SetField(ref _accessLevel, value); }
        }

        #endregion

        #region UiResources

        public string PermissionReadOnlyText => ResourceService.UiResources.GetString("UI_PermissionReadOnly");
        public string PermissionReadAndWriteText => ResourceService.UiResources.GetString("UI_PermissionReadAndWrite");
        public string PermissionFullAccessText => ResourceService.UiResources.GetString("UI_PermissionFullAccess");
        public string RemoveFromFolderText => ResourceService.UiResources.GetString("UI_RemoveFromFolder");

        #endregion
    }
}
