using mega;
using MegaApp.Extensions;
using MegaApp.MegaApi;
using MegaApp.ViewModels;
using System;

namespace MegaApp.Services
{
    public static class AccountService
    {
        private static AccountDetailsViewModel _accountDetails;
        public static AccountDetailsViewModel AccountDetails
        {
            get
            {
                if (_accountDetails != null) return _accountDetails;
                _accountDetails = new AccountDetailsViewModel();
                return _accountDetails;
            }
        }

        public static async void GetAccountDetails()
        {
            var accountDetailsRequestListener = new GetAccountDetailsRequestListenerAsync();
            var accountDetails = await accountDetailsRequestListener.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.getAccountDetails(accountDetailsRequestListener);
            });

            if (accountDetails == null) return;

            AccountDetails.AccountType = accountDetails.getProLevel();
            AccountDetails.TotalSpace = accountDetails.getStorageMax().ToReadableSize();
            AccountDetails.TotalSpaceUnits = accountDetails.getStorageMax().ToReadableUnits();
            AccountDetails.UsedSpace = accountDetails.getStorageUsed().ToReadableSize();
            AccountDetails.UsedSpaceUnits = accountDetails.getStorageUsed().ToReadableUnits();
            AccountDetails.TransferQuota = accountDetails.getTransferMax().ToReadableSize();
            AccountDetails.TransferQuotaUnits = accountDetails.getTransferMax().ToReadableUnits();
            AccountDetails.UsedTransferQuota = accountDetails.getTransferOwnUsed().ToReadableSize();
            AccountDetails.UsedTransferQuotaUnits = accountDetails.getTransferOwnUsed().ToReadableUnits();

            AccountDetails.PaymentMethod = accountDetails.getSubscriptionMethod();

            DateTime date;
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            // If there is a valid subscription get the renew time
            if (accountDetails.getSubscriptionStatus() == MSubscriptionStatus.SUBSCRIPTION_STATUS_VALID)
            {
                try
                {
                    if (accountDetails.getSubscriptionRenewTime() != 0)
                    {
                        date = start.AddSeconds(Convert.ToDouble(accountDetails.getSubscriptionRenewTime()));
                        _accountDetails.SubscriptionRenewDate = date.ToString("dd MMM yyyy");
                    }
                    else
                    {
                        date = start.AddSeconds(Convert.ToDouble(accountDetails.getProExpiration()));
                        _accountDetails.ProExpirationDate = date.ToString("dd MMM yyyy");
                    }
                }
                catch (ArgumentOutOfRangeException) { /* Do nothing*/ }
            }
        }
    }
}
