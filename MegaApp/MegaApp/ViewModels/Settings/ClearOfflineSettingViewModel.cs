using MegaApp.Extensions;
using MegaApp.Services;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.ViewModels.Settings
{
    public class ClearOfflineSettingViewModel : ButtonSettingViewModel
    {
        public ClearOfflineSettingViewModel() : base(
            ResourceService.UiResources.GetString("UI_UsedOffline"), null,
            ResourceService.UiResources.GetString("UI_ClearOffline"))
        {
            this.ButtonAction = this.ClearOffline;
            this.GetOfflineSize();
        }

        #region Methods

        public override string GetValue(string defaultValue) => this.Value;

        /// <summary>
        /// Get the size of the offline content
        /// </summary>
        private async void GetOfflineSize() =>
            this.Value = (await AppService.GetOfflineSizeAsync()).ToStringAndSuffix(1);

        /// <summary>
        /// Clear all the offline content of the app
        /// </summary>
        private async void ClearOffline()
        {
            var result = await DialogService.ShowOkCancelAsync(
                ResourceService.UiResources.GetString("UI_ClearOffline"),
                ResourceService.AppMessages.GetString("AM_ClearOfflineQuestion"),
                TwoButtonsDialogType.YesNo);

            if (!result) return;

            if (AppService.ClearOffline())
            {
                ToastService.ShowTextNotification(
                    ResourceService.AppMessages.GetString("AM_ClearOfflineSuccess_Title"),
                    ResourceService.AppMessages.GetString("AM_ClearOfflineSuccess"));
            }
            else
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_ClearOfflineFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_ClearOfflineFailed"));
            }

            this.GetOfflineSize();
        }

        #endregion
    }
}
