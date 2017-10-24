using mega;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

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
            this.AccessLevel = new SharedFolderAccessLevelViewModel();
            this.Initialize(outShare);
        }

        #region Methods

        /// <summary>
        /// Initialize the view model of the
        /// </summary>
        /// <param name="outShare">Folder outbound sharing with the contact</param>
        private void Initialize(MShare outShare)
        {
            OnUiThread(() => this.AccessLevel.AccessType = (MShareType)outShare.getAccess());
        }

        #endregion

        #region Properties

        private SharedFolderAccessLevelViewModel _accessLevel;
        /// <summary>
        /// Access level of the contact to the outgoing shared folder
        /// </summary>
        public SharedFolderAccessLevelViewModel AccessLevel
        {
            get { return _accessLevel; }
            set { SetField(ref _accessLevel, value); }
        }

        #endregion
    }
}
