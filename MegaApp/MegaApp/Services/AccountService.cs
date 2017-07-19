using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using mega;
using MegaApp.Classes;
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
        /// Storages all the info to upgrade an account
        /// </summary>
        private static UpgradeAccountViewModel _upgradeAccount;
        public static UpgradeAccountViewModel UpgradeAccount
        {
            get
            {
                if (_upgradeAccount != null) return _upgradeAccount;
                _upgradeAccount = new UpgradeAccountViewModel();
                return _upgradeAccount;
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
                AccountDetails.TotalSpace = accountDetails.getStorageMax();
                AccountDetails.UsedSpace = accountDetails.getStorageUsed();

                AccountDetails.CloudDriveUsedSpace = accountDetails.getStorageUsed(SdkService.MegaSdk.getRootNode().getHandle());
                AccountDetails.IncomingSharesUsedSpace = GetIncomingSharesUsedSpace();
                AccountDetails.RubbishBinUsedSpace = accountDetails.getStorageUsed(SdkService.MegaSdk.getRubbishNode().getHandle());
                
                AccountDetails.TransferQuota = accountDetails.getTransferMax();
                AccountDetails.UsedTransferQuota = accountDetails.getTransferOwnUsed();

                AccountDetails.TransferOverquotaDelay = SdkService.MegaSdk.getBandwidthOverquotaDelay();
                AccountDetails.IsInTransferOverquota = AccountDetails.TransferOverquotaDelay != 0;
                if (AccountDetails.IsInTransferOverquota)
                    AccountDetails.TimerTransferOverquota?.Start();
                else
                    AccountDetails.TimerTransferOverquota?.Stop();

                AccountDetails.PaymentMethod = accountDetails.getSubscriptionMethod();
            });

            // If there is a valid subscription
            if (accountDetails.getSubscriptionStatus() == MSubscriptionStatus.SUBSCRIPTION_STATUS_VALID)
            {
                // If is a valid and active subscription (auto renewable subscription)
                if (accountDetails.getSubscriptionRenewTime() != 0)
                    GetSubscriptionDetails(accountDetails);
                // If is a valid but non active subscription (canceled subscription)
                else if (accountDetails.getProExpiration() != 0)
                    GetNonSubscriptionDetails(accountDetails);
            }
            // If is a pro account without subscription
            else if (accountDetails.getProExpiration() != 0)
            {
                GetNonSubscriptionDetails(accountDetails);
            }
        }

        /// <summary>
        /// Get the specific details of valid and active subscription (auto renewable subscription)
        /// </summary>
        /// <param name="accountDetails">Details related to the MEGA account</param>
        private static void GetSubscriptionDetails(MAccountDetails accountDetails)
        {
            DateTime date;
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            try
            {
                date = start.AddSeconds(Convert.ToDouble(accountDetails.getSubscriptionRenewTime()));
                UiService.OnUiThread(() => AccountDetails.SubscriptionRenewDate = date.ToString("dd MMM yyyy"));
            }
            catch (ArgumentOutOfRangeException)
            {
                UiService.OnUiThread(() => AccountDetails.SubscriptionRenewDate = ResourceService.UiResources.GetString("UI_NotAvailable"));
            }

            switch (accountDetails.getSubscriptionCycle())
            {
                case "1 M":
                    UiService.OnUiThread(() => AccountDetails.SubscriptionType = ResourceService.UiResources.GetString("UI_MonthlyRecurring"));
                    break;
                case "1 Y":
                    UiService.OnUiThread(() => AccountDetails.SubscriptionType = ResourceService.UiResources.GetString("UI_AnnualRecurring"));
                    break;
                default:
                    UiService.OnUiThread(() => AccountDetails.SubscriptionType = ResourceService.UiResources.GetString("UI_NotAvailable"));
                    break;
            }
        }

        /// <summary>
        /// Get the specific details of valid but non active subscription (canceled subscription) or
        /// of a pro account without subscription.
        /// </summary>
        /// <param name="accountDetails">Details related to the MEGA account</param>
        private static void GetNonSubscriptionDetails(MAccountDetails accountDetails)
        {
            DateTime date;
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            try
            {
                date = start.AddSeconds(Convert.ToDouble(accountDetails.getProExpiration()));
                UiService.OnUiThread(() => AccountDetails.ProExpirationDate = date.ToString("dd MMM yyyy"));
            }
            catch (ArgumentOutOfRangeException)
            {
                UiService.OnUiThread(() => AccountDetails.ProExpirationDate = ResourceService.UiResources.GetString("UI_NotAvailable"));
            }

            switch (accountDetails.getSubscriptionCycle())
            {
                case "1 M":
                    UiService.OnUiThread(() => AccountDetails.SubscriptionType = ResourceService.UiResources.GetString("UI_Monthly"));
                    break;
                case "1 Y":
                    UiService.OnUiThread(() => AccountDetails.SubscriptionType = ResourceService.UiResources.GetString("UI_Annual"));
                    break;
                default:
                    UiService.OnUiThread(() => AccountDetails.SubscriptionType = ResourceService.UiResources.GetString("UI_NotAvailable"));
                    break;
            }
        }

        /// <summary>
        /// Get the account used space by the incoming shared folders
        /// </summary>
        /// <returns>Used space by the incoming shared folders</returns>
        private static ulong GetIncomingSharesUsedSpace()
        {
            MNodeList inSharesList = SdkService.MegaSdk.getInShares();
            int inSharesListSize = inSharesList.size();

            ulong inSharesUsedSpace = 0;
            for (int i = 0; i < inSharesListSize; i++)
            {
                MNode inShare = inSharesList.get(i);
                if (inShare != null)
                    inSharesUsedSpace += SdkService.MegaSdk.getSize(inShare);
            }

            return inSharesUsedSpace;
        }

        /// <summary>
        /// Gets all the user data info
        /// </summary>
        public static async void GetUserData()
        {
            await GetUserEmail();
            GetUserAvatarColor();
            GetUserAvatar();
            GetUserFirstname();
            GetUserLastname();
        }

        /// <summary>
        /// Gets the user email
        /// </summary>
        public static async Task GetUserEmail()
        {
            await UiService.OnUiThreadAsync(() => UserData.UserEmail = SdkService.MegaSdk.getMyEmail());
        }

        /// <summary>
        /// Gets the user avatar color
        /// </summary>
        public static void GetUserAvatarColor()
        {
            var avatarColor = UiService.GetColorFromHex(SdkService.MegaSdk.getUserAvatarColor(SdkService.MegaSdk.getMyUser()));
            UiService.OnUiThread(() => UserData.AvatarColor = avatarColor);
        }

        /// <summary>
        /// Gets the user avatar
        /// </summary>
        public static async void GetUserAvatar()
        {
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
        }

        /// <summary>
        /// Gets the user first name attribute
        /// </summary>
        public static async void GetUserFirstname()
        {
            var userAttributeRequestListener = new GetUserAttributeRequestListenerAsync();
            var firstname = await userAttributeRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getOwnUserAttribute((int)MUserAttrType.USER_ATTR_FIRSTNAME, userAttributeRequestListener));
            UiService.OnUiThread(() => UserData.Firstname = firstname);
        }

        /// <summary>
        /// Gets the user last name attribute
        /// </summary>
        public static async void GetUserLastname()
        {
            var userAttributeRequestListener = new GetUserAttributeRequestListenerAsync();
            var lastname = await userAttributeRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getOwnUserAttribute((int)MUserAttrType.USER_ATTR_LASTNAME, userAttributeRequestListener));
            UiService.OnUiThread(() => UserData.Lastname = lastname);
        }

        /// <summary>
        /// Gets all the pricing info
        /// </summary>
        public static void GetPricing()
        {
            GetPaymentMethods();
            GetPricingDetails();
        }

        /// <summary>
        /// Gets the available payment methods
        /// </summary>
        private static async void GetPaymentMethods()
        {
            UpgradeAccount.IsInAppPaymentMethodAvailable = await LicenseService.GetIsAvailableAsync();

            var paymentMethodsRequestListener = new GetPaymentMethodsRequestListenerAsync();
            var availablePaymentMethods = await paymentMethodsRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getPaymentMethods(paymentMethodsRequestListener));

            UpgradeAccount.IsCentiliPaymentMethodAvailable = Convert.ToBoolean(availablePaymentMethods & (1 << (int)MPaymentMethod.PAYMENT_METHOD_CENTILI));
            UpgradeAccount.IsFortumoPaymentMethodAvailable = Convert.ToBoolean(availablePaymentMethods & (1 << (int)MPaymentMethod.PAYMENT_METHOD_FORTUMO));
            //UpgradeAccount.IsCreditCardPaymentMethodAvailable = Convert.ToBoolean(availablePaymentMethods & (1 << (int)MPaymentMethod.PAYMENT_METHOD_CREDIT_CARD));
        }

        /// <summary>
        /// Gets all pricing details info
        /// </summary>
        private static async void GetPricingDetails()
        {
            await UiService.OnUiThreadAsync(() =>
            {
                UpgradeAccount.Plans.Clear();
                UpgradeAccount.Products.Clear();
            });

            var pricingRequestListener = new GetPricingRequestListenerAsync();
            var pricingDetails = await pricingRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getPricing(pricingRequestListener));

            if (pricingDetails == null) return;

            int numberOfProducts = pricingDetails.getNumProducts();

            for (int i = 0; i < numberOfProducts; i++)
            {
                var accountType = (MAccountType)Enum.Parse(typeof(MAccountType),
                    pricingDetails.getProLevel(i).ToString());

                var product = new Product
                {
                    AccountType = accountType,
                    Amount = pricingDetails.getAmount(i),
                    Currency = GetCurrencySymbol(pricingDetails.getCurrency(i)),
                    GbStorage = pricingDetails.getGBStorage(i),
                    GbTransfer = pricingDetails.getGBTransfer(i),
                    Months = pricingDetails.getMonths(i),
                    Handle = pricingDetails.getHandle(i)
                };

                switch (accountType)
                {
                    case MAccountType.ACCOUNT_TYPE_FREE:
                        product.Name = ResourceService.AppResources.GetString("AR_AccountTypeFree");
                        product.ProductPathData = ResourceService.VisualResources.GetString("VR_AccountTypeFreePathData");
                        break;

                    case MAccountType.ACCOUNT_TYPE_LITE:
                        product.Name = ResourceService.AppResources.GetString("AR_AccountTypeLite");
                        product.ProductColor = (Color)Application.Current.Resources["MegaProLiteAccountColor"];
                        product.ProductPathData = ResourceService.VisualResources.GetString("VR_AccountTypeProLitePathData");

                        // If Centili payment method is active, and product is LITE monthly include it into the product
                        product.IsCentiliPaymentMethodAvailable = UpgradeAccount.IsCentiliPaymentMethodAvailable && product.Months == 1;

                        // If Fortumo payment method is active, and product is LITE monthly include it into the product
                        product.IsFortumoPaymentMethodAvailable = UpgradeAccount.IsFortumoPaymentMethodAvailable && product.Months == 1;
                        break;

                    case MAccountType.ACCOUNT_TYPE_PROI:
                        product.Name = ResourceService.AppResources.GetString("AR_AccountTypePro1");
                        product.ProductColor = (Color)Application.Current.Resources["MegaProAccountColor"];
                        product.ProductPathData = ResourceService.VisualResources.GetString("VR_AccountTypePro1PathData");
                        break;

                    case MAccountType.ACCOUNT_TYPE_PROII:
                        product.Name = ResourceService.AppResources.GetString("AR_AccountTypePro2");
                        product.ProductColor = (Color)Application.Current.Resources["MegaProAccountColor"];
                        product.ProductPathData = ResourceService.VisualResources.GetString("VR_AccountTypePro2PathData");
                        break;

                    case MAccountType.ACCOUNT_TYPE_PROIII:
                        product.Name = ResourceService.AppResources.GetString("AR_AccountTypePro3");
                        product.ProductColor = (Color)Application.Current.Resources["MegaProAccountColor"];
                        product.ProductPathData = ResourceService.VisualResources.GetString("VR_AccountTypePro3PathData");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // If CC payment method is active, include it into the product
                //product.IsCreditCardPaymentMethodAvailable = UpgradeAccount.IsCreditCardPaymentMethodAvailable;

                // If in-app payment method is active, include it into the product
                product.IsInAppPaymentMethodAvailable = UpgradeAccount.IsInAppPaymentMethodAvailable;

                await UiService.OnUiThreadAsync(() => UpgradeAccount.Products.Add(product));

                // Plans show only the information off the monthly plans
                if (pricingDetails.getMonths(i) == 1)
                {
                    var plan = new ProductBase
                    {
                        AccountType = accountType,
                        Name = product.Name,
                        Amount = product.Amount,
                        Currency = product.Currency,
                        GbStorage = product.GbStorage,
                        GbTransfer = product.GbTransfer,
                        ProductPathData = product.ProductPathData,
                        ProductColor = product.ProductColor
                    };

                    await UiService.OnUiThreadAsync(() => UpgradeAccount.Plans.Add(plan));
                }
            }
        }

        /// <summary>
        /// Gets the currency symbol corresponding to a currency ISO code
        /// </summary>
        /// <param name="currencyCode">Currency ISO code</param>
        /// <returns></returns>
        private static string GetCurrencySymbol(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode)) return string.Empty;

            switch(currencyCode)
            {
                case "EUR": return "€";
                case "USD": return "$";
                default:    return currencyCode;
            }
        }

        /// <summary>
        /// Clear all the account details info
        /// </summary>
        public static void ClearAccountDetails()
        {
            UiService.OnUiThread(() =>
            {
                AccountDetails.AccountType = MAccountType.ACCOUNT_TYPE_FREE;
                AccountDetails.TotalSpace = 0;
                AccountDetails.UsedSpace = 0;
                AccountDetails.CloudDriveUsedSpace = 0;
                AccountDetails.RubbishBinUsedSpace = 0;
                AccountDetails.TransferQuota = 0;
                AccountDetails.UsedTransferQuota = 0;
                AccountDetails.IsInTransferOverquota = false;
                AccountDetails.PaymentMethod = string.Empty;
                AccountDetails.SubscriptionRenewDate = string.Empty;
                AccountDetails.ProExpirationDate = string.Empty;
            });
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
