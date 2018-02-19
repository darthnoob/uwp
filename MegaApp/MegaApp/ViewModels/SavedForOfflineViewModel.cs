using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class SavedForOfflineViewModel : BaseSdkViewModel
    {
        public SavedForOfflineViewModel() : base(SdkService.MegaSdk)
        {
            
        }

        #region UiResources

        public string SavedForOfflineText => ResourceService.UiResources.GetString("UI_SavedForOffline");

        #endregion
    }
}
