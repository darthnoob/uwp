using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class AppVersionSettingViewModel : SettingViewModel<string>
    {
        public AppVersionSettingViewModel()
            : base("App version", null, null)
        {}

        public override string GetValue(string defaultValue)
        {
            return SettingsService.Load(ResourceService.SettingsResources.GetString("SR_UseStagingServer"), false) ? 
                string.Format("{0} (staging)", AppService.GetAppVersion()) : AppService.GetAppVersion();
        }
    }
}
