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
            return AppService.GetAppVersion();
        }
    }
}
