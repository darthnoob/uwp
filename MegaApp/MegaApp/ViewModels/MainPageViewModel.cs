using mega;
using MegaApp.Classes;

namespace MegaApp.ViewModels
{
    public class MainPageViewModel : BaseSdkViewModel
    {
        public MainPageViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk)
        {
            
        }

        #region UiResources

        public string UI_CloudDriveName { get { return App.ResourceLoaders.UiResources.GetString("UI_CloudDriveName"); } }
        public string UI_RubbishBinName { get { return App.ResourceLoaders.UiResources.GetString("UI_RubbishBinName"); } }

        #endregion
    }
}
