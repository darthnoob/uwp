using mega;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.SharedFolders
{
    public class SharedFolderNodeViewModel : FolderNodeViewModel, IMegaSharedFolderNode
    {
        public SharedFolderNodeViewModel(MNode megaNode)
            : base(SdkService.MegaSdk, App.AppInformation, megaNode, null)
        {
            this.Update();
        }

        #region Methods

        public void Update(bool externalUpdate = false)
        {
            base.Update(this.OriginalMNode, externalUpdate);

            OnUiThread(() => this.AccessLevel = (MShareType)SdkService.MegaSdk.getAccess(this.OriginalMNode));
        }

        #endregion

        #region Properties

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
