using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class MultiFactorAuthenticationSettingViewModel : SettingViewModel<bool>
    {
        public MultiFactorAuthenticationSettingViewModel()
            : base(ResourceService.UiResources.GetString("UI_EnableTwoFactorAuthentication"), 
                  null, "MultiFactorAuthenticationSettingsKey")
        {
            this.ValueChanged += async (sender, args) =>
            {
                await StoreValue(this.Key, this.Value);

                if(this.Value)
                    this.Value = await DialogService.ShowMultiFactorAuthenticationSetupDialog();
            };
        }

        public override void Initialize()
        {
            this.CheckMultiFactorAuthStatus();
        }

        private async void CheckMultiFactorAuthStatus()
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
