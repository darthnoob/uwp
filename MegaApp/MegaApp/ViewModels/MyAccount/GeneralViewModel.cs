using MegaApp.Services;

namespace MegaApp.ViewModels.MyAccount
{
    public class GeneralViewModel : MyAccountBaseViewModel
    {
        #region UiResources

        // Title
        public string Title => ResourceService.UiResources.GetString("UI_General");

        // Account overview
        public string AccountOverviewTitle => ResourceService.UiResources.GetString("UI_AccountOverview");
        public string PlanTypeText => ResourceService.UiResources.GetString("UI_PlanType");
        public string SubscriptionEndsText => ResourceService.UiResources.GetString("UI_SubscriptionEnds");
        public string SubscriptionRenewsText => ResourceService.UiResources.GetString("UI_SubscriptionRenews");

        // Payment information
        public string PaymentInformationTitle => ResourceService.UiResources.GetString("UI_PaymentInformation");
        public string PaymentMethodText => ResourceService.UiResources.GetString("UI_PaymentMethod");
        public string SubscriptionTypeText => ResourceService.UiResources.GetString("UI_Type");

        // Overall usage
        public string OverallUsageTitle => ResourceService.UiResources.GetString("UI_OverallUsage");
        public string OverallUsageText => ResourceService.UiResources.GetString("UI_OverallUsageText");
        public string TotalStorageText => ResourceService.UiResources.GetString("UI_TotalStorage");
        public string UsedStorageText => ResourceService.UiResources.GetString("UI_UsedStorage");
        public string TotalTransferQuotaText => ResourceService.UiResources.GetString("UI_TotalTransferQuota");
        public string UsedTransferQuotaText => ResourceService.UiResources.GetString("UI_UsedTransferQuota");

        // Achievements
        public string UnlockedBonusesTitle => ResourceService.UiResources.GetString("UI_UnlockedBonuses");
        public string UnlockedBonusesText => ResourceService.UiResources.GetString("UI_UnlockedBonusesText");
        public string StorageQuotaText => ResourceService.UiResources.GetString("UI_StorageQuota");
        public string TransferQuotaText => ResourceService.UiResources.GetString("UI_TransferQuota");
        public string GetMoreBonusesText => ResourceService.UiResources.GetString("UI_GetMoreBonuses");

        #endregion
    }
}
