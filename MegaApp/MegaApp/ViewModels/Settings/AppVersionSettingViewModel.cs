using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class AppVersionSettingViewModel : SettingViewModel<string>
    {
        public AppVersionSettingViewModel()
            : base(ResourceService.UiResources.GetString("UI_AppVersion"), null, null)
        {

        }

        public override string GetValue(string defaultValue)
        {
            if (SettingsService.Load(ResourceService.SettingsResources.GetString("SR_UseStagingServer"), false))
                return string.Format("{0} (staging)", AppService.GetAppVersion());
            else if (SettingsService.Load(ResourceService.SettingsResources.GetString("SR_UseStagingServerPort444"), false))
                return string.Format("{0} (staging:444)", AppService.GetAppVersion());
            else
                return AppService.GetAppVersion();
        }
    }
}
