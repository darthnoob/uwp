using mega;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class IncomingSharedFolderNodeViewModel : FolderNodeViewModel
    {
        public IncomingSharedFolderNodeViewModel(MNode megaNode)
            : base(SdkService.MegaSdk, App.AppInformation, megaNode, null)
        {
            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_IncomingSharedFolderPathData");
            this.Update();            
        }

        #region Methods

        public void Update(bool externalUpdate = false)
        {
            this.AccessLevel = (MShareType)SdkService.MegaSdk.getAccess(this.OriginalMNode);
            base.Update(this.OriginalMNode, externalUpdate);
        }

        #endregion

        #region Properties

        private MShareType _accessLevel;
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
                switch(this.AccessLevel)
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
