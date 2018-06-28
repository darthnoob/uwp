using MegaApp.Extensions;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class ClearCacheSettingViewModel : ButtonSettingViewModel
    {
        public ClearCacheSettingViewModel() : base(
            ResourceService.UiResources.GetString("UI_UsedCache"), null,
            ResourceService.UiResources.GetString("UI_ClearCache"))
        {
            this.ButtonAction = this.ClearCache;
            this.Value = AppService.GetAppCacheSize().ToStringAndSuffix();
        }

        #region Methods

        public override string GetValue(string defaultValue) => this.Value;

        /// <summary>
        /// Clear the app cache
        /// </summary>
        private async void ClearCache()
        {
            if (AppService.ClearAppCache())
            {
                ToastService.ShowTextNotification(
                    ResourceService.AppMessages.GetString("AM_CacheCleared_Title"),
                    ResourceService.AppMessages.GetString("AM_CacheCleared"));
            }
            else
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_ClearCacheFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_ClearCacheFailed"));
            }

            this.Value =  AppService.GetAppCacheSize().ToStringAndSuffix();
        }

        #endregion
    }
}
