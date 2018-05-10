using System;
using Windows.System;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class SdkVersionSettingViewModel : SettingViewModel<string>
    {
        public SdkVersionSettingViewModel()
            : base(ResourceService.UiResources.GetString("UI_MEGA_SDK_Version"), null, null)
        {}

        protected override async void DoAction()
        {
            await Launcher.LaunchUriAsync(new Uri(
                AppService.GetMegaSDK_Link(), 
                UriKind.RelativeOrAbsolute));
        }

        public override string GetValue(string defaultValue)
        {
            return $"SDK {AppService.GetMegaSDK_Version()}";
        }
    }
}
