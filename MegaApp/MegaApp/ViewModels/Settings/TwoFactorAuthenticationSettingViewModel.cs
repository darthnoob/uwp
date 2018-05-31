using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class TwoFactorAuthenticationSettingViewModel : SettingViewModel<bool>
    {
        public TwoFactorAuthenticationSettingViewModel()
            : base(ResourceService.UiResources.GetString("UI_EnableTwoFactorAuthentication"), 
                  null, "TwoFactorAuthenticationSettingsKey")
        {
            this.ValueChanged += async (sender, args) =>
            {
                await StoreValue(this.Key, this.Value);
            };
        }

        public override async void Initialize()
        {
            var multiFactorAuthCheck = new MultiFactorAuthCheckRequestListenerAsync();
            var result = await multiFactorAuthCheck.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.multiFactorAuthCheck(
                    SdkService.MegaSdk.getMyEmail(), multiFactorAuthCheck);
            });

            if (result.HasValue)
                this.Value = result.Value;
        }
    }
}
