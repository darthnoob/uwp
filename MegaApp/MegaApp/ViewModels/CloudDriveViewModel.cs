using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class CloudDriveViewModel: BaseSdkViewModel
    {
        #region UiResources

        public string UI_CloudDriveName { get { return ResourceService.UiResources.GetString("UI_CloudDriveName"); } }
        public string UI_RubbishBinName { get { return ResourceService.UiResources.GetString("UI_RubbishBinName"); } }

        #endregion
    }
}
