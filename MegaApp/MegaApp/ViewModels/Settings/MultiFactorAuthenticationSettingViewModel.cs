using System;
using System.Threading.Tasks;
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
            this.ValueChanged += this.OnValueChanged;
        }

        public override async void Initialize()
        {
            var result = await this.CheckMultiFactorAuthStatusAsync();

            this.ValueChanged -= this.OnValueChanged;

            if (result.HasValue)
                this.Value = result.Value;

            this.ValueChanged += this.OnValueChanged;
        }

        /// <summary>
        /// Check the status of the Multi-Factor Authentication
        /// </summary>
        /// <returns>The current status of the or NULL if something failed</returns>
        private async Task<bool?> CheckMultiFactorAuthStatusAsync()
        {
            var multiFactorAuthCheck = new MultiFactorAuthCheckRequestListenerAsync();
            var result = await multiFactorAuthCheck.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.multiFactorAuthCheck(
                    SdkService.MegaSdk.getMyEmail(), multiFactorAuthCheck);
            });

            return result;            
        }

        /// <summary>
        /// Enable the Multi-Factor Authentication
        /// </summary>
        /// <returns>TRUE if all is OK or FALSE if something failed</returns>
        private async Task<bool> EnableMultiFactorAuthAsync() =>
            await DialogService.ShowMultiFactorAuthSetupDialogAsync();

        /// <summary>
        /// Disable the Multi-Factor Authentication
        /// </summary>
        /// <returns>TRUE if all is OK or FALSE if something failed</returns>
        private async Task<bool> DisableMultiFactorAuthAsync()
        {
            var code = await DialogService.ShowInputDialogAsync(
                ResourceService.AppMessages.GetString("AM_2FA_DisableDialogTitle"),
                ResourceService.AppMessages.GetString("AM_2FA_DisableDialogMessage"),
                ResourceService.UiResources.GetString("UI_Disable"),
                ResourceService.UiResources.GetString("UI_Close"));

            if (string.IsNullOrWhiteSpace(code)) return false;

            var disableMultiFactorAuth = new MultiFactorAuthDisableRequestListenerAsync();
            var result = await disableMultiFactorAuth.ExecuteAsync(() =>
                SdkService.MegaSdk.multiFactorAuthDisable(code, disableMultiFactorAuth));

            return result;
        }

        private async void OnValueChanged(object sender, EventArgs args)
        {
            this.ValueChanged -= this.OnValueChanged;

            this.Value = this.Value ?
                await this.EnableMultiFactorAuthAsync() :
                !await this.DisableMultiFactorAuthAsync();

            this.ValueChanged += this.OnValueChanged;

            await StoreValue(this.Key, this.Value);
        }
    }
}
