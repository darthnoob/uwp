using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.MegaApi;

namespace MegaApp.ViewModels.SharedFolders
{
    public class SharedFolderNodeViewModel : FolderNodeViewModel, IMegaSharedFolderNode
    {
        public SharedFolderNodeViewModel(MNode megaNode, SharedFoldersListViewModel parent)
            : base(SdkService.MegaSdk, App.AppInformation, megaNode, null)
        {
            this.Parent = parent;

            this.DownloadCommand = new RelayCommand(Download);
            this.OpenInformationPanelCommand = new RelayCommand(OpenInformationPanel);

            this.Update(megaNode);
        }

        #region Commands

        public override ICommand DownloadCommand { get; }
        public ICommand OpenInformationPanelCommand { get; }

        #endregion

        #region Virtual Commands

        public virtual ICommand LeaveShareCommand { get; }
        public virtual ICommand RemoveSharedAccessCommand { get; }

        #endregion

        #region Methods

        private void ParentItemCollectionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Parent.ItemCollection.OnlyOneSelectedItem))
                OnPropertyChanged(nameof(this.OnlyOneSelectedItem));
        }

        /// <summary>
        /// Update core data associated with the SDK MNode object
        /// </summary>
        /// <param name="megaNode">Node to update</param>
        /// <param name="externalUpdate">Indicates if is an update external to the app. For example from an `onNodesUpdate`</param>
        public override void Update(MNode megaNode, bool externalUpdate = false)
        {
            base.Update(megaNode, externalUpdate);

            OnUiThread(() => this.AccessLevel = (MShareType)SdkService.MegaSdk.getAccess(megaNode));
        }

        private void Download()
        {
            if (this.Parent.ItemCollection.IsMultiSelectActive)
            {
                if (this.Parent.DownloadCommand.CanExecute(null))
                    this.Parent.DownloadCommand.Execute(null);
                return;
            }

            base.Download(TransferService.MegaTransfers);
        }

        private void OpenInformationPanel()
        {
            if (this.Parent.ItemCollection.OnlyOneSelectedItem)
            {
                if (this.Parent.OpenInformationPanelCommand.CanExecute(null))
                    this.Parent.OpenInformationPanelCommand.Execute(null);
            }
        }

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

        private SharedFoldersListViewModel _parent;
        protected new SharedFoldersListViewModel Parent
        {
            get { return _parent; }
            set
            {
                if (_parent?.ItemCollection != null)
                    _parent.ItemCollection.PropertyChanged -= ParentItemCollectionOnPropertyChanged;

                SetField(ref _parent, value);

                if (_parent?.ItemCollection != null)
                    _parent.ItemCollection.PropertyChanged += ParentItemCollectionOnPropertyChanged;
            }
        }

        private string _contactsText;
        public string ContactsText
        {
            get { return _contactsText; }
            set { SetField(ref _contactsText, value); }
        }

        private string _folderLocation;
        public string FolderLocation
        {
            get { return _folderLocation; }
            set { SetField(ref _folderLocation, value); }
        }

        private string _owner;
        /// <summary>
        /// Acces level to the incoming shared node
        /// </summary>
        public string Owner
        {
            get { return _owner; }
            set { SetField(ref _owner, value); }
        }

        private MShareType _accessLevel;
        /// <summary>
        /// Acces level to the incoming shared node
        /// </summary>
        public MShareType AccessLevel
        {
            get { return _accessLevel; }
            set
            {
                SetField(ref _accessLevel, value);
                OnPropertyChanged(nameof(this.AccessLevelText),
                    nameof(this.AccessLevelPathData));
            }
        }

        public string AccessLevelText
        {
            get
            {
                switch (this.AccessLevel)
                {
                    case MShareType.ACCESS_READWRITE:
                        return ResourceService.UiResources.GetString("UI_PermissionReadAndWrite");
                    case MShareType.ACCESS_FULL:
                    case MShareType.ACCESS_OWNER:
                        return ResourceService.UiResources.GetString("UI_PermissionFullAccess");
                    case MShareType.ACCESS_READ:                        
                    case MShareType.ACCESS_UNKNOWN:                    
                    default:
                        return ResourceService.UiResources.GetString("UI_PermissionReadOnly");
                }
            }
        }

        public string AccessLevelPathData
        {
            get
            {
                switch (this.AccessLevel)
                {
                    case MShareType.ACCESS_READWRITE:
                        return ResourceService.VisualResources.GetString("VR_PermissionsReadAndWritePathData");
                    case MShareType.ACCESS_FULL:
                    case MShareType.ACCESS_OWNER:
                        return ResourceService.VisualResources.GetString("VR_PermissionsFullAccessPathData");
                    case MShareType.ACCESS_READ:                        
                    case MShareType.ACCESS_UNKNOWN:                    
                    default:
                        return ResourceService.VisualResources.GetString("VR_PermissionsReadOnlyPathData");
                }
            }
        }

        public bool OnlyOneSelectedItem => this.Parent.ItemCollection.OnlyOneSelectedItem;

        #region UiResources

        public string InformationText => ResourceService.UiResources.GetString("UI_Information");

        #endregion

        #endregion
    }
}
