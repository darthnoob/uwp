using System;
using System.Threading.Tasks;
using MegaApp.Enums;
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
            this.ValueChanged -= this.OnValueChanged;

            var result = await AccountService.CheckMultiFactorAuthStatusAsync();
            switch (result)
            {
                case MultiFactorAuthStatus.Enabled:
                    this.Value = true;
                    break;

                case MultiFactorAuthStatus.Disabled:
                    this.Value = false;
                    break;

                case MultiFactorAuthStatus.Unknown:
                    OnUiThread(async () =>
                    {
                        await DialogService.ShowAlertAsync(
                            ResourceService.UiResources.GetString("UI_Warning"),
                            ResourceService.AppMessages.GetString("AM_MFA_CheckStatusFailed"));
                    });
                    break;
            }

            this.ValueChanged += this.OnValueChanged;
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
                ResourceService.AppMessages.GetString("AM_2FA_DisableDialogTitle"));

            if (result)
                DialogService.ShowMultiFactorAuthDisabledDialog();

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
