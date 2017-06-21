using System;
using Windows.UI.Xaml.Media.Imaging;
using mega;
using MegaApp.Extensions;
using MegaApp.MegaApi;
using MegaApp.ViewModels;

namespace MegaApp.Services
{
    public static class AccountService
    {
        /// <summary>
        /// Storages all the account details info
        /// </summary>
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

        /// <summary>
        /// Storages all the user data info
        /// </summary>
        private static UserDataViewModel _userData;
        public static UserDataViewModel UserData
        {
            get
            {
                if (_userData != null) return _userData;
                _userData = new UserDataViewModel();
                return _userData;
            }
        }

        /// <summary>
        /// Gets all the account details info
        /// </summary>
        public static async void GetAccountDetails()
        {
            var accountDetailsRequestListener = new GetAccountDetailsRequestListenerAsync();
            var accountDetails = await accountDetailsRequestListener.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.getAccountDetails(accountDetailsRequestListener);
            });

            if (accountDetails == null) return;

            UiService.OnUiThread(() =>
            {
                AccountDetails.AccountType = accountDetails.getProLevel();
                AccountDetails.TotalSpace = accountDetails.getStorageMax().ToReadableSize();
                AccountDetails.TotalSpaceUnits = accountDetails.getStorageMax().ToReadableUnits();
                AccountDetails.UsedSpace = accountDetails.getStorageUsed().ToReadableSize();
                AccountDetails.UsedSpaceUnits = accountDetails.getStorageUsed().ToReadableUnits();
                AccountDetails.TransferQuota = accountDetails.getTransferMax().ToReadableSize();
                AccountDetails.TransferQuotaUnits = accountDetails.getTransferMax().ToReadableUnits();
                AccountDetails.UsedTransferQuota = accountDetails.getTransferOwnUsed().ToReadableSize();
                AccountDetails.UsedTransferQuotaUnits = accountDetails.getTransferOwnUsed().ToReadableUnits();

                AccountDetails.IsInTransferOverquota = SdkService.MegaSdk.getBandwidthOverquotaDelay() != 0;

                AccountDetails.PaymentMethod = accountDetails.getSubscriptionMethod();
            });

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
                        UiService.OnUiThread(() => AccountDetails.SubscriptionRenewDate = date.ToString("dd MMM yyyy"));
                    }
                    else
                    {
                        date = start.AddSeconds(Convert.ToDouble(accountDetails.getProExpiration()));
                        UiService.OnUiThread(() => AccountDetails.ProExpirationDate = date.ToString("dd MMM yyyy"));
                    }
                }
                catch (ArgumentOutOfRangeException) { /* Do nothing*/ }
            }
        }

        /// <summary>
        /// Gets all the user data info
        /// </summary>
        public static async void GetUserData()
        {
            await UiService.OnUiThreadAsync(() => UserData.UserEmail = SdkService.MegaSdk.getMyEmail());

            var avatarColor = UiService.GetColorFromHex(SdkService.MegaSdk.getUserAvatarColor(SdkService.MegaSdk.getMyUser()));
            UiService.OnUiThread(() => UserData.AvatarColor = avatarColor);

            var userAvatarRequestListener = new GetUserAvatarRequestListenerAsync();
            var userAvatarResult = await userAvatarRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getOwnUserAvatar(UserData.AvatarPath, userAvatarRequestListener));

            if (userAvatarResult)
            {
                UiService.OnUiThread(() =>
                {
                    var img = new BitmapImage();
                    img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    img.UriSource = new Uri(UserData.AvatarPath);
                    UserData.AvatarUri = img.UriSource;
                });
            }
            else
            {
                UiService.OnUiThread(() => UserData.AvatarUri = null);
            }

            var userAttributeRequestListener = new GetUserAttributeRequestListenerAsync();
            var firstname = await userAttributeRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getOwnUserAttribute((int)MUserAttrType.USER_ATTR_FIRSTNAME, userAttributeRequestListener));
            UiService.OnUiThread(() => UserData.Firstname = firstname);
            var lastname = await userAttributeRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getOwnUserAttribute((int)MUserAttrType.USER_ATTR_LASTNAME, userAttributeRequestListener));
            UiService.OnUiThread(() => UserData.Lastname = lastname);
        }

        /// <summary>
        /// Clear all the user data info
        /// </summary>
        public static void ClearUserData()
        {
            UiService.OnUiThread(() =>
            {
                UserData.UserEmail = string.Empty;
                UserData.AvatarColor = UiService.GetColorFromHex("#00000000");
                UserData.AvatarUri = null;
                UserData.Firstname = string.Empty;
                UserData.Lastname = string.Empty;
            });
        }
    }
}
