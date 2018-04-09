using MegaApp.Services;

namespace MegaApp.ViewModels.MyAccount
{
    public class AchievementsViewModel : MyAccountBaseViewModel
    {
        #region UiResources
        
        // Unlocked rewards
        public string UnlockedRewardsTitle => ResourceService.UiResources.GetString("UI_UnlockedRewards");
        public string StorageQuotaText => ResourceService.UiResources.GetString("UI_StorageQuota");
        public string TransferQuotaText => ResourceService.UiResources.GetString("UI_TransferQuota");

        // Achievements
        public string AchievementsTitle => ResourceService.UiResources.GetString("UI_Achievements");
        public string AchievementsText => ResourceService.UiResources.GetString("UI_AchievementsText");

        public string AvailableText => ResourceService.UiResources.GetString("UI_Available");
        public string CompletedText => ResourceService.UiResources.GetString("UI_Completed");

        #endregion
    }
}
