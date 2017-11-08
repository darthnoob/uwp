using System.Threading.Tasks;
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

            this.Update(megaNode);
        }

        #region Methods

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
            set { SetField(ref _parent, value); }
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
                    case MShareType.ACCESS_READ:
                        return ResourceService.UiResources.GetString("UI_PermissionReadOnly");
                    case MShareType.ACCESS_READWRITE:
                        return ResourceService.UiResources.GetString("UI_PermissionReadAndWrite");
                    case MShareType.ACCESS_FULL:
                        return ResourceService.UiResources.GetString("UI_PermissionFullAccess");
                    case MShareType.ACCESS_UNKNOWN:
                    case MShareType.ACCESS_OWNER:
                    default:
                        return string.Empty;
                }
            }
        }

        public string AccessLevelPathData
        {
            get
            {
                switch (this.AccessLevel)
                {
                    case MShareType.ACCESS_READ:
                        return ResourceService.VisualResources.GetString("VR_PermissionsReadOnlyPathData");
                    case MShareType.ACCESS_READWRITE:
                        return ResourceService.VisualResources.GetString("VR_PermissionsReadAndWritePathData");
                    case MShareType.ACCESS_FULL:
                        return ResourceService.VisualResources.GetString("VR_PermissionsFullAccessPathData");
                    case MShareType.ACCESS_UNKNOWN:
                    case MShareType.ACCESS_OWNER:
                    default:
                        return string.Empty;
                }
            }
        }
        
        #endregion
    }
}
