using mega;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class AccessLevelViewModel : BaseViewModel
    {
        #region Properties

        private MShareType _accessType;
        /// <summary>
        /// Access level to the node
        /// </summary>
        public MShareType AccessType
        {
            get { return _accessType; }
            set
            {
                SetField(ref _accessType, value);
                OnPropertyChanged(nameof(this.AccessLevelText),
                    nameof(this.AccessLevelPathData));
            }
        }

        public string AccessLevelText
        {
            get
            {
                switch (this.AccessType)
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
                switch (this.AccessType)
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

        #endregion
    }
}
