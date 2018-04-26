using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using mega;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.MegaApi;
using MegaApp.ViewModels;
using MegaApp.ViewModels.Contacts;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Services
{
    public static class AccountService
    {
        private const string DateFormat = "dd MMM yyyy";

        /// <summary>
        /// Flag to store if the account has been blocked
        /// </summary>
        public static bool IsAccountBlocked;

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
        /// Storages all the account achievements info
        /// </summary>
        private static AccountAchievementsViewModel _accountAchievements;
        public static AccountAchievementsViewModel AccountAchievements
        {
            get
            {
                if (_accountAchievements != null) return _accountAchievements;
                _accountAchievements = new AccountAchievementsViewModel();
                return _accountAchievements;
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
        /// Check if should show the password reminder dialog and show it in that case
        /// </summary>
        public static async Task<bool> ShouldShowPasswordReminderDialogAsync()
        {
            var passwordReminderDialogListener = new ShouldShowPasswordReminderDialogRequestListenerAsync();
            return await passwordReminderDialogListener.ExecuteAsync(() =>
                SdkService.MegaSdk.shouldShowPasswordReminderDialog(false, passwordReminderDialogListener));
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

                GetTransferOverquotaDetails();

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

        public static async void GetAccountAchievements()
        {
            UiService.OnUiThread(() => 
                AccountAchievements.IsAchievementsEnabled = SdkService.MegaSdk.isAchievementsEnabled());

            if (!SdkService.MegaSdk.isAchievementsEnabled()) return;

            var accountAchievementsRequestListener = new GetAccountAchievementsRequestListenerAsync();
            var accountAchievements = await accountAchievementsRequestListener.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.getAccountAchievements(accountAchievementsRequestListener);
            });
            
            if (accountAchievements == null) return;
            
            var awards = new List<AwardClassViewModel>
            {
                // Add Base storage & transfer
                new AwardClassViewModel(null, true)
                {
                    StorageReward = accountAchievements.getBaseStorage(),
                }
            };

            var awardedClasses = new List<AwardClassViewModel>
            {
                // Add Base storage & transfer
                new AwardClassViewModel(null, true)
                {
                    StorageReward = accountAchievements.getBaseStorage(),
                    IsTransferAmountVisible = false,
                }
            };


            // Get all the user awards
            var awardsCount = accountAchievements.getAwardsCount();

            for (uint i = 0; i < awardsCount; i++)
            {
                var awardId = accountAchievements.getAwardId(i);
                var awardClass = (MAchievementClass) accountAchievements.getAwardClass(i);

                var awardedClass = awardedClasses.FirstOrDefault(a => a.AchievementClass == awardClass);
                if (awardedClass == null)
                {
                    awardedClass = new AwardClassViewModel(awardClass)
                    {
                        IsGranted = true
                    };
                    awardedClasses.Add(awardedClass);
                }
                
                var storageReward = accountAchievements.getRewardStorageByAwardId(awardId);
                var transferReward = accountAchievements.getRewardTransferByAwardId(awardId);
                var expireDate = accountAchievements.getAwardExpirationTs(i).ConvertTimestampToDateTime();
                var achievedOnDate = accountAchievements.getAwardTimestamp(i).ConvertTimestampToDateTime();
                var durationInDays = accountAchievements.getClassExpire((int) awardClass);
                awards.Add(new AwardClassViewModel(awardClass)
                {
                    StorageReward = storageReward,
                    TransferReward = transferReward,
                    ExpireDate = expireDate,
                    AchievedOnDate = achievedOnDate,
                    DurationInDays = durationInDays,
                    IsGranted = true
                });

               
                if (awardClass == MAchievementClass.MEGA_ACHIEVEMENT_INVITE)
                {
                    var mails = accountAchievements.getAwardEmails(i);
                    var mailSize = mails.size();
                    for (int m = 0; m < mailSize; m++)
                    {
                        var contact = new ContactViewModel(
                            SdkService.MegaSdk.getContact(mails.get(m)), awardedClass.Contacts)
                        {
                            StorageAmount = storageReward,
                            TransferAmount = transferReward,
                            ReferralBonusExpireDate = expireDate,
                        };
                        awardedClass.Contacts.ItemCollection.Items.Add(contact);
                        contact.GetContactFirstname();
                        contact.GetContactLastname();
                        contact.GetContactAvatarColor();
                        contact.GetContactAvatar();
                    }

                    if (expireDate <= DateTime.Now) continue;

                    awardedClass.StorageReward += storageReward;
                    awardedClass.TransferReward += transferReward;
                }
                else
                {
                    awardedClass.ExpireDate = expireDate;
                    awardedClass.StorageReward = storageReward;
                    awardedClass.TransferReward = transferReward;
                    awardedClass.AchievedOnDate = achievedOnDate;
                    awardedClass.DurationInDays = durationInDays;
                }
            }

            var awardClasses = Enum.GetValues(typeof(MAchievementClass)).OfType<MAchievementClass>();
            var availableAwards = new List<AwardClassViewModel>();
            foreach (var awardClass in awardClasses)
            {
                switch (awardClass)
                {
                    case MAchievementClass.MEGA_ACHIEVEMENT_WELCOME:
                        continue;
                    case MAchievementClass.MEGA_ACHIEVEMENT_INVITE:
                    {
                        var inviteClass = new AwardClassViewModel(awardClass)
                        {
                            StorageReward = accountAchievements.getClassStorage((int)awardClass),
                            TransferReward = accountAchievements.getClassTransfer((int)awardClass),
                            DurationInDays = accountAchievements.getClassExpire((int)awardClass),
                        };

                        inviteClass.Contacts.ItemCollection.Items =
                            awardedClasses.FirstOrDefault(a => a.AchievementClass == awardClass)?.Contacts.ItemCollection.Items;
                        availableAwards.Add(inviteClass);

                        continue;
                    };
                }

                var available = awards.FirstOrDefault(a => a.AchievementClass == awardClass);

                if (available != null) continue;

                availableAwards.Add(new AwardClassViewModel(awardClass)
                {
                    StorageReward = accountAchievements.getClassStorage((int)awardClass),
                    TransferReward = accountAchievements.getClassTransfer((int)awardClass),
                    DurationInDays = accountAchievements.getClassExpire((int)awardClass)
                });
            }

            UiService.OnUiThread(() =>
            {
                if (accountAchievements.currentStorage() != -1)
                    AccountAchievements.CurrentStorageQuota = (ulong) accountAchievements.currentStorage();

                if (accountAchievements.currentTransfer() != -1)
                    AccountAchievements.CurrentTransferQuota = (ulong) accountAchievements.currentTransfer();

                AccountAchievements.Awards = awards;
                AccountAchievements.AwardedClasses = awardedClasses;
                AccountAchievements.AvailableAwards = availableAwards;
                AccountAchievements.CompletedAwards = awardedClasses.Where(a => a.IsGranted).ToList();
            });

        }

        /// <summary>
        /// Gets the specific details related to a transfer over-quota
        /// </summary>
        public static void GetTransferOverquotaDetails()
        {
            UiService.OnUiThread(() =>
            {
                AccountDetails.TransferOverquotaDelay = SdkService.MegaSdk.getBandwidthOverquotaDelay();
                AccountDetails.IsInTransferOverquota = AccountDetails.TransferOverquotaDelay != 0;
                if (AccountDetails.IsInTransferOverquota)
                    AccountDetails.TimerTransferOverquota?.Start();
                else
                    AccountDetails.TimerTransferOverquota?.Stop();
            });
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
                UiService.OnUiThread(() => AccountDetails.SubscriptionRenewDate = date.ToString(DateFormat));
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
                UiService.OnUiThread(() => AccountDetails.ProExpirationDate = date.ToString(DateFormat));
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
                    var img = new BitmapImage()
                    {
                        CreateOptions = BitmapCreateOptions.IgnoreImageCache,
                        UriSource = new Uri(UserData.AvatarPath)
                    };
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
        /// Gets all pricing details info.
        /// </summary>
        private static async void GetPricingDetails()
        {
            // Only if they were not obtained before
            if (UpgradeAccount.Plans.Any() && UpgradeAccount.Products.Any())
                return;

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
                    FormattedPrice = string.Format("{0:N} {1}", (double)pricingDetails.getAmount(i) / 100, GetCurrencySymbol(pricingDetails.getCurrency(i))),
                    Currency = GetCurrencySymbol(pricingDetails.getCurrency(i)),
                    GbStorage = pricingDetails.getGBStorage(i),
                    GbTransfer = pricingDetails.getGBTransfer(i),
                    Months = pricingDetails.getMonths(i),
                    Handle = pricingDetails.getHandle(i)
                };

                // Try get the local pricing details from the store
                var storeProduct = await LicenseService.GetProductAsync(product.MicrosoftStoreId);
                if (storeProduct != null)
                {
                    product.FormattedPrice = storeProduct.FormattedPrice;

                    try
                    {
                        // 'ProductListing.CurrencyCode' property was introduced on the Windows 10.0.10586 build.
                        // In previous builds like Windows 10.0.10240, it will throw an 'InvalidCastException'.
                        product.Currency = GetCurrencySymbol(
                            ApiInformation.IsPropertyPresent("Windows.ApplicationModel.Store.ProductListing", "CurrencyCode") ?
                                storeProduct.CurrencyCode : GetCurrencyFromFormattedPrice(storeProduct.FormattedPrice));
                    }
                    catch (InvalidCastException)
                    {
                        product.Currency = GetCurrencySymbol(GetCurrencyFromFormattedPrice(storeProduct.FormattedPrice));
                    }
                }

                switch (accountType)
                {
                    case MAccountType.ACCOUNT_TYPE_FREE:
                        product.Name = ResourceService.AppResources.GetString("AR_AccountTypeFree");
                        product.ProductColor = (Color)Application.Current.Resources["MegaFreeAccountColor"];
                        product.ProductPathData = ResourceService.VisualResources.GetString("VR_AccountTypeFreePathData");
                        break;

                    case MAccountType.ACCOUNT_TYPE_LITE:
                        product.Name = ResourceService.AppResources.GetString("AR_AccountTypeLite");
                        product.ProductColor = (Color)Application.Current.Resources["MegaProLiteAccountColor"];
                        product.ProductPathData = ResourceService.VisualResources.GetString("VR_AccountTypeProLitePathData");

                        // If product is is LITE monthly, store the price and currency for upgrade notifications purposes 
                        // and include Centili and Fortumo payments methods if available
                        if (product.Months == 1)
                        {
                            product.IsCentiliPaymentMethodAvailable = UpgradeAccount.IsCentiliPaymentMethodAvailable;
                            product.IsFortumoPaymentMethodAvailable = UpgradeAccount.IsFortumoPaymentMethodAvailable;
                            UpgradeAccount.LiteMonthlyFormattedPrice = product.FormattedPrice;
                        }
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
                        FormattedPrice = product.FormattedPrice,
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

        public static async Task<string> GetLiteMonthlyFormattedPrice()
        {
            var pricingRequestListener = new GetPricingRequestListenerAsync();
            var pricingDetails = await pricingRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getPricing(pricingRequestListener));

            if (pricingDetails == null) return null;

            int numberOfProducts = pricingDetails.getNumProducts();

            for (int i = 0; i < numberOfProducts; i++)
            {
                var accountType = (MAccountType)Enum.Parse(typeof(MAccountType),
                    pricingDetails.getProLevel(i).ToString());

                if ((accountType == MAccountType.ACCOUNT_TYPE_LITE) && (pricingDetails.getMonths(i) == 1))
                {
                    // Try get the local pricing details from the store
                    var storeProduct = await LicenseService.GetProductAsync(ResourceService.AppResources.GetString("AR_ProLiteMonth"));
                    if (storeProduct != null)
                    {
                        UpgradeAccount.LiteMonthlyFormattedPrice = storeProduct.FormattedPrice;
                        break;
                    }

                    // Get the price from the MEGA server
                    UpgradeAccount.LiteMonthlyFormattedPrice = string.Format("{0:N} {1}",
                        (double)pricingDetails.getAmount(i) / 100, GetCurrencySymbol(pricingDetails.getCurrency(i)));
                }
            }

            return UpgradeAccount.LiteMonthlyFormattedPrice;
        }

        /// <summary>
        /// Gets the currency (ISO code or symbol) from a formatted price string
        /// </summary>
        /// <param name="formattedPrice">String with the price and the currency.</param>
        /// <returns>Currency ISO code or symbol of the formatted price string</returns>
        private static string GetCurrencyFromFormattedPrice(string formattedPrice)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(formattedPrice)) return string.Empty;

                char[] charsToTrim = { '0','1','2','3','4','5','6','7','8','9',' ','.',',' };
                return formattedPrice.Trim(charsToTrim);
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, $"Failure getting currency from {formattedPrice}", e);
                return "n/a";
            }
        }

        /// <summary>
        /// Gets the currency symbol corresponding to a currency ISO code
        /// </summary>
        /// <param name="currencyCode">Currency ISO code</param>
        /// <returns>Currency symbol associated with the curreny ISO code.</returns>
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
                UserData.AvatarColor = (Color)Application.Current.Resources["MegaRedColor"];
                UserData.AvatarUri = null;
                UserData.Firstname = string.Empty;
                UserData.Lastname = string.Empty;
            });
        }

        /// <summary>
        /// Gets a string that resembles a number of day(s).
        /// </summary>
        /// <param name="days">Number of days to convert to string</param>
        /// <returns>
        /// A string that resembles a number of days or
        /// '-' if the number of days is less than 1.
        /// </returns>
        public static string GetDays(long days)
        {
            if (days < 1) return "-";
            return days == 1
                ? string.Format("1 {0}", ResourceService.UiResources.GetString("UI_Day"))
                : string.Format("{0} {1}", days, ResourceService.UiResources.GetString("UI_Days"));

        }

        /// <summary>
        /// Gets a string that resembles a number of days remaining
        /// </summary>
        /// <param name="days">Number of days to convert to string</param>
        /// <returns>
        /// A string that resembles a number of days remaining or 
        /// 'Expired' if the number of days is less than 1.
        /// </returns>
        public static string GetDaysRemaining(long days)
        {
            if (days < 1) return ResourceService.UiResources.GetString("UI_Expired");
            return days == 1
                ? string.Format("1 {0}", ResourceService.UiResources.GetString("UI_RemainingDay"))
                : string.Format(ResourceService.UiResources.GetString("UI_RemainingDays"), days);

        }
    }
}
