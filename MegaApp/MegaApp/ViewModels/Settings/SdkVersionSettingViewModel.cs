using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class SdkVersionSettingViewModel : LinkSettingViewModel
    {
        public SdkVersionSettingViewModel()
            : base(ResourceService.UiResources.GetString("UI_MEGA_SDK_Version"),
                  $"SDK {AppService.GetMegaSDK_Version()}", AppService.GetMegaSDK_Link())
        {

        }
    }
}
