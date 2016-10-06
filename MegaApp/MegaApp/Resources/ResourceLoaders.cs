using Windows.ApplicationModel.Resources;

namespace MegaApp.Resources
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class ResourceLoaders
    {
        private ResourceLoader _appMessages = ResourceLoader.GetForViewIndependentUse("AppMessages");
        private ResourceLoader _appResources = ResourceLoader.GetForViewIndependentUse("AppResources");
        private ResourceLoader _progressMessages = ResourceLoader.GetForViewIndependentUse("ProgressMessages");
        private ResourceLoader _settingsResources = ResourceLoader.GetForViewIndependentUse("SettingsResources");
        private ResourceLoader _uiResources = ResourceLoader.GetForViewIndependentUse("UiResources");        

        public ResourceLoader AppMessages
        {
            get { return _appMessages; }
        }

        public ResourceLoader AppResources
        {
            get { return _appResources; }
        }

        public ResourceLoader ProgressMessages
        {
            get { return _progressMessages; }
        }

        public ResourceLoader SettingsResources
        {
            get { return _settingsResources; }
        }

        public ResourceLoader UiResources
        {
            get { return _uiResources; }
        }
    }
}