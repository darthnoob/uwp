using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class CloudDriveViewModel: BaseSdkViewModel
    {
        #region UiResources

        public string CloudDriveNameText { get { return ResourceService.UiResources.GetString("UI_CloudDriveName"); } }
        public string RubbishBinNameText { get { return ResourceService.UiResources.GetString("UI_RubbishBinName"); } }

        #endregion
    }
}
