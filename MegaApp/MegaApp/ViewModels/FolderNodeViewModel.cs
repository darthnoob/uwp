using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.Contacts;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.ViewModels
{
    public class FolderNodeViewModel: NodeViewModel, IMegaFolderNode
    {
        public FolderNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, FolderViewModel parent,
            ObservableCollection<IBaseNode> parentCollection = null, ObservableCollection<IBaseNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parent, parentCollection, childCollection)
        {
            this.ShareCommand = new RelayCommand(Share);
            this.RemoveSharedAccessCommand = new RelayCommand(RemoveSharedAccess);

            this.Update(megaNode);
        }

        #region Commands

        public ICommand ShareCommand { get; set; }
        public ICommand RemoveSharedAccessCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Update core data associated with the SDK MNode object
        /// </summary>
        /// <param name="megaNode">Node to update</param>
        /// <param name="externalUpdate">Indicates if is an update external to the app. For example from an `onNodesUpdate`</param>
        public override void Update(MNode megaNode, bool externalUpdate = false)
        {
            base.Update(megaNode, externalUpdate);
            SetFolderInfo();

            if (megaNode.isInShare()) return;

            OnPropertyChanged(nameof(this.IsOutShare), nameof(this.SharingText));

            if (megaNode.isOutShare())
            {
                if(this.ContactsList == null)
                {
                    this.ContactsList = new ContactsListOutgoingSharedFolderViewModel(megaNode);
                    this.GetContactsList();
                }
            }
            else
            {
                if (this.ContactsList != null)
                    OnUiThread(() => this.ContactsList.ItemCollection.Clear());
            }

            this.DefaultImagePathData = megaNode.isOutShare() ?
                ResourceService.VisualResources.GetString("VR_OutgoingSharedFolderPathData") :
                ResourceService.VisualResources.GetString("VR_FolderTypePath_default");

            if (!megaNode.getName().ToLower().Equals("camera uploads")) return;
            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_FolderTypePath_photo");
        }

        public void SetFolderInfo()
        {
            this.ChildFolders = this.MegaSdk.getNumChildFolders(this.OriginalMNode);
            this.ChildFiles = this.MegaSdk.getNumChildFiles(this.OriginalMNode);

            string infoString = ResourceService.UiResources.GetString("UI_EmptyFolder");
            if (this.ChildFolders > 0 && this.ChildFiles > 0)
            {
                infoString = string.Format("{0} {1}, {2} {3}",
                    this.ChildFolders, this.ChildFolders == 1 ? this.SingleForderString : this.MultipleFordersString,
                    this.ChildFiles, this.ChildFiles == 1 ? this.SingleFileString : this.MultipleFilesString);
            }
            else if (this.ChildFolders > 0)
            {
                infoString = string.Format("{0} {1}", this.ChildFolders, 
                    this.ChildFolders == 1 ? this.SingleForderString : this.MultipleFordersString);
            }
            else if (this.ChildFiles > 0)
            {
                infoString = string.Format("{0} {1}", this.ChildFiles,
                    this.ChildFiles == 1 ? this.SingleFileString: this.MultipleFilesString);
            }

            OnUiThread(() => this.Contents = infoString);
        }

        private async void GetContactsList()
        {
            await OnUiThreadAsync(() => this.ContactsList.ItemCollection.Clear());

            var contactsList = SdkService.MegaSdk.getOutShares(this.OriginalMNode);
            var contactsListSize = contactsList.size();

            for (int i = 0; i < contactsListSize; i++)
            {
                // To avoid null values
                if (contactsList.get(i) == null) continue;

                var megaContact = new ContactOutgoingSharedFolderViewModel(contactsList.get(i), this.ContactsList);

                OnUiThread(() => this.ContactsList.ItemCollection.Items.Add(megaContact));

                megaContact.GetContactFirstname();
                megaContact.GetContactLastname();
                megaContact.GetContactAvatarColor();
                megaContact.GetContactAvatar();
            }
        }

        /// <summary>
        /// Start and manage sharing of a folder in MEGA
        /// </summary>
        public void Share()
        {
            if (this.Parent == null || !this.Parent.ItemCollection.OnlyOneSelectedItem) return;

            if (this.Parent.ShareCommand.CanExecute(null))
                this.Parent.ShareCommand.Execute(null);
        }

        /// <summary>
        /// Stop sharing a folder in MEGA
        /// </summary>
        private async void RemoveSharedAccess()
        {
            var dialogResult = await DialogService.ShowOkCancelAsync(
                ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolder_Title"),
                string.Format(ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolderQuestion"), this.Name),
                ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolderWarning"),
                OkCancelDialogButtons.Custom, this.RemoveText, this.CancelText);

            if (!dialogResult) return;

            if (await this.RemoveSharedAccessAsync()) return;

            OnUiThread(async () =>
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolder_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolderFailed"), this.Name));
            });
        }

        /// <summary>
        /// Stop sharing a folder in MEGA
        /// </summary>
        /// <returns>Result of the action</returns>
        public async Task<bool> RemoveSharedAccessAsync()
        {
            var removeSharedAccess = new ShareRequestListenerAsync();
            var outShares = SdkService.MegaSdk.getOutShares(this.OriginalMNode);
            var outSharesSize = outShares.size();
            bool result = true;
            for (int i = 0; i < outSharesSize; i++)
            {
                result = result & await removeSharedAccess.ExecuteAsync(() =>
                {
                    this.MegaSdk.shareByEmail(this.OriginalMNode, outShares.get(i).getUser(),
                        (int)MShareType.ACCESS_UNKNOWN, removeSharedAccess);
                });
            }

            return result;
        }

        #endregion

        #region Properties

        public bool IsOutShare => this.OriginalMNode.isOutShare();

        private string _contents;
        public string Contents
        {
            get { return _contents; }
            set { SetField(ref _contents, value); }
        }

        private ContactsListOutgoingSharedFolderViewModel _contactsList;
        /// <summary>
        /// List of contacts with the folder is shared
        /// </summary>
        public ContactsListOutgoingSharedFolderViewModel ContactsList
        {
            get { return _contactsList; }
            set
            {
                _contactsList?.Deinitialize();

                SetField(ref _contactsList, value);

                _contactsList?.Initialize();
            }
        }

        #endregion

        #region UiResources

        public string SharingText => this.IsOutShare ? this.ManageCollaboratorsText :
            ResourceService.UiResources.GetString("UI_Sharing");

        private string ManageCollaboratorsText => ResourceService.UiResources.GetString("UI_ManageCollaborators");

        private string SingleForderString => ResourceService.UiResources.GetString("UI_SingleFolder").ToLower();
        private string MultipleFordersString => ResourceService.UiResources.GetString("UI_MultipleFolders").ToLower();
        private string SingleFileString => ResourceService.UiResources.GetString("UI_SingleFile").ToLower();
        private string MultipleFilesString => ResourceService.UiResources.GetString("UI_MultipleFiles").ToLower();

        #endregion
    }
}
