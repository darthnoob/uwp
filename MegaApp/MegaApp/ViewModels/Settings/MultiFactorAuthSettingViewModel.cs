using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.Dialogs;

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
        /// Show the dialog to disable the Multi-Factor Authentication
        /// </summary>
        /// <returns>TRUE if all is OK or FALSE if something failed</returns>
        private async Task<bool> ShowDisableMultiFactorAuthDialogAsync()
        {
            var result = await DialogService.ShowAsyncMultiFactorAuthCodeInputDialogAsync(
                this.DisableMultiFactorAuthAsync,
                ResourceService.AppMessages.GetString("AM_2FA_DisableDialogTitle"),
                ResourceService.AppMessages.GetString("AM_2FA_DisableDialogMessage"));

            if (result)
            {
                ToastService.ShowTextNotification(
                    ResourceService.AppMessages.GetString("AM_2FA_DisabledDialogTitle"));
            }

            return result;
        }

        /// <summary>
        /// Disable the Multi-Factor Authentication
        /// </summary>
        /// <returns>TRUE if all is OK or FALSE if something failed</returns>
        private async Task<bool> DisableMultiFactorAuthAsync(string code)
        {
            var disableMultiFactorAuth = new MultiFactorAuthDisableRequestListenerAsync();
            var result = await disableMultiFactorAuth.ExecuteAsync(() =>
                SdkService.MegaSdk.multiFactorAuthDisable(code, disableMultiFactorAuth));

            if(!result)
                DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();

            return result;
        }

        private async void OnValueChanged(object sender, EventArgs args)
        {
            this.ValueChanged -= this.OnValueChanged;

            this.Value = this.Value ?
                await this.EnableMultiFactorAuthAsync() :
                !await this.ShowDisableMultiFactorAuthDialogAsync();

            this.ValueChanged += this.OnValueChanged;

            await StoreValue(this.Key, this.Value);
        }
    }
}
