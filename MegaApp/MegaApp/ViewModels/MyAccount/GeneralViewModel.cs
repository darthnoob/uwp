using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels.MyAccount
{
    public class GeneralViewModel : BaseViewModel
    {
        public EventHandler GoToUpgrade;

        public GeneralViewModel()
        {
            this.RubbishBinCommand = new RelayCommand(RubbishBin);
            this.UpgradeCommand = new RelayCommand(Upgrade);
        }

        #region Commands

        public ICommand RubbishBinCommand { get; }
        public ICommand UpgradeCommand { get; }

        #endregion

        #region Private Methods

        private void RubbishBin()
        {
            UiService.OnUiThread(() =>
            {
                NavigateService.Instance.Navigate(typeof(CloudDrivePage), false,
                    NavigationObject.Create(typeof(GeneralViewModel), NavigationActionType.RubbishBin));
            });
        }

        private void Upgrade()
        {
            GoToUpgrade?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public AccountDetailsViewModel AccountDetails => AccountService.AccountDetails;

        #endregion

        #region UiResources

        // Account overview
        public string AccountOverviewTitle => ResourceService.UiResources.GetString("UI_AccountOverview");
        public string PlanTypeText => ResourceService.UiResources.GetString("UI_PlanType");
        public string PaymentMethodText => ResourceService.UiResources.GetString("UI_PaymentMethod");
        public string SubscriptionEndsText => ResourceService.UiResources.GetString("UI_SubscriptionEnds");
        public string SubscriptionRenewsText => ResourceService.UiResources.GetString("UI_SubscriptionRenews");

        // Overall usage
        public string OverallUsageTitle => ResourceService.UiResources.GetString("UI_OverallUsage");
        public string OverallUsageText => ResourceService.UiResources.GetString("UI_OverallUsageText");
        public string OverallUsageStorageOverquotaText => ResourceService.UiResources.GetString("UI_OverallUsageStorageOverquotaText");
        public string TotalStorageText => ResourceService.UiResources.GetString("UI_TotalStorage");
        public string UsedStorageText => ResourceService.UiResources.GetString("UI_UsedStorage");
        public string TotalTransferQuotaText => ResourceService.UiResources.GetString("UI_TotalTransferQuota");
        public string UsedTransferQuotaText => ResourceService.UiResources.GetString("UI_UsedTransferQuota");
        public string UpgradeText => ResourceService.UiResources.GetString("UI_Upgrade");
        public string RubbishBinText => ResourceService.UiResources.GetString("UI_RubbishBinName");

        #endregion
    }
}
