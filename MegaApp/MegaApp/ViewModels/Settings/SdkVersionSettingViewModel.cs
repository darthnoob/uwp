using System;
using Windows.System;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class SdkVersionSettingViewModel : SettingViewModel<string>
    {
        public SdkVersionSettingViewModel()
            : base("MEGA SDK version", null, null)
        {}

        protected override async void DoAction()
        {
            await Launcher.LaunchUriAsync(new Uri(
                ResourceService.AppResources.GetString("AR_SdkLink"), 
                UriKind.RelativeOrAbsolute));
        }

        public override string GetValue(string defaultValue)
        {
            return $"SDK {ResourceService.AppResources.GetString("AR_SkdVersion")}";
        }
    }
}
