using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class MultiFactorAuthSettingViewModel : SettingViewModel<bool>
    {
        public MultiFactorAuthSettingViewModel()
            : base(ResourceService.UiResources.GetString("UI_EnableTwoFactorAuth"), 
                  null, "MultiFactorAuthSettingsKey")
        {
            this.ValueChanged += async (sender, args) =>
            {
                await StoreValue(this.Key, this.Value);

                if(this.Value)
                    this.Value = await DialogService.ShowMultiFactorAuthSetupDialogAsync();
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
