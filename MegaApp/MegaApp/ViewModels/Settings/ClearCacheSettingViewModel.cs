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
            this.GetAppCacheSize();
        }

        #region Methods

        public override string GetValue(string defaultValue) => this.Value;

        /// <summary>
        /// Get the size of the app cache
        /// </summary>
        private async void GetAppCacheSize() =>
            this.Value = (await AppService.GetAppCacheSizeAsync()).ToStringAndSuffix(1);

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

            this.GetAppCacheSize();
        }

        #endregion
    }
}
