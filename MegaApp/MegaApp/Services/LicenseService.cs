using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;


namespace MegaApp.Services
{
    /// <summary>
    /// Service that handles in app purchases (IAP) and MEGA license validation
    /// </summary>
    public static class LicenseService
    {
        /// <summary>
        /// Current license information retrieved from the Windows Store
        /// </summary>
        public static LicenseInformation CurrentLicenseInformation
        {
            get
            {
                {
#if DEBUG
                    return CurrentAppSimulator.LicenseInformation;
#else
                    return CurrentApp.LicenseInformation;
#endif
                }

            }
        }

        /// <summary>
        /// Current listing information retrieved from the Windows Store
        /// </summary>
        /// <returns>Current Windows Store listing information</returns>
        public static async Task<ListingInformation> GetListingInformationAsync()
        {
            try
            {
#if DEBUG
                return await CurrentAppSimulator.LoadListingInformationAsync();
#else
                return await CurrentApp.LoadListingInformationAsync();
#endif 
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Failure retrieving Windows Store listing information", e);
                return null;
            }
        }


#if DEBUG
        /// <summary>
        /// ONLY DEBUG: Load WindowsStoreProxy.xml file for debug/simulation purposes 
        /// </summary>
        public static async Task LoadSimulatorAsync()
        {
            await CurrentAppSimulator.ReloadSimulatorAsync(
                await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri("ms-appx:///Resources/WindowsStoreProxy.xml", UriKind.RelativeOrAbsolute)));
        }
#endif


        /// <summary>
        /// Get value if Windows Store & listing information is available
        /// </summary>
        /// <returns>True if information is available, else false</returns>
        public static async Task<bool> GetIsAvailableAsync()
        {
            try
            {
                var listing = await GetListingInformationAsync();
                return listing != null &&
                       listing.ProductListings.Any();
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Failure retrieving Windows Store listing information", e);
                // If listing information can not be loaded. No Internet is available or the 
                // Windows Store is unavailable for retrieving data
                return false;
            }           
        }

        /// <summary>
        /// Get Windows Store product ID matching a MEGA product
        /// </summary>
        /// <param name="megaProductId">MEGA product identifier to match</param>
        /// <returns>Windows Store product identifier or NULL if none available</returns>
        public static async Task<string> GetProductIdAsync(string megaProductId)
        {
            try
            {
                if (string.IsNullOrEmpty(megaProductId)) return null;

                var listing = await GetListingInformationAsync();

                if (listing == null || !listing.ProductListings.Any()) return null;

                var result = listing.ProductListings.First(
                    p => p.Key.ToLower().Equals(megaProductId.ToLower()));

                return result.Value.ProductId;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, $"Failure retrieving product id for {megaProductId}", e);
                return null;
            }           
        }

        /// <summary>
        /// Get Windows Store product matching a MEGA product
        /// </summary>
        /// <param name="megaProductId">MEGA product identifier to match</param>
        /// <returns>Windows Store product or NULL if none available</returns>
        public static async Task<ProductListing> GetProductAsync(string megaProductId)
        {
            try
            {
                if (string.IsNullOrEmpty(megaProductId)) return null;

                var listing = await GetListingInformationAsync();
                if (listing == null || !listing.ProductListings.Any()) return null;

                var result = listing.ProductListings.First(
                    p => p.Key.ToLower().Equals(megaProductId.ToLower()));

                return result.Value;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, $"Failure retrieving store product for {megaProductId}", e);
                return null;
            }
        }

        public static async Task<PurchaseResponse> PurchaseProductAsync(string productId)
        {
            var purchaseReponse = new PurchaseResponse();

            // Check product id argument
            if (string.IsNullOrEmpty(productId))
            {
                purchaseReponse.Type = PurchaseResponseType.PurchaseFailed;
                return purchaseReponse;
            }

            // Check if Windows Store product list is available
            var available = await GetIsAvailableAsync();
            if (!available)
            {
                purchaseReponse.Type = PurchaseResponseType.UnAvailable;
                return purchaseReponse;
            }

            // Check if the user has already purchased the Windows Store product
            if (CurrentLicenseInformation.ProductLicenses[productId].IsActive)
            {
                purchaseReponse.Type = PurchaseResponseType.AlreadyPurchased;
                return purchaseReponse;
            }

            // Check if product exists in the Windows Store
            var listing = await GetListingInformationAsync();
            if (!listing.ProductListings.ContainsKey(productId))
            {
                purchaseReponse.Type = PurchaseResponseType.UnAvailable;
                return purchaseReponse;
            }

            try
            {
                // The actual purchase
#if DEBUG
                purchaseReponse.Result = await CurrentAppSimulator.RequestProductPurchaseAsync(productId);
#else
                purchaseReponse.Result = await CurrentApp.RequestProductPurchaseAsync(productId);
#endif
                switch (purchaseReponse.Result.Status)
                {
                    case ProductPurchaseStatus.Succeeded:
                        purchaseReponse.Type = PurchaseResponseType.Succeeded;
                        break;
                    case ProductPurchaseStatus.AlreadyPurchased:
                        purchaseReponse.Type = PurchaseResponseType.AlreadyPurchased;
                        break;
                    case ProductPurchaseStatus.NotFulfilled:
                    case ProductPurchaseStatus.NotPurchased:
                        purchaseReponse.Type = PurchaseResponseType.PurchaseFailed;
                        break;
                    default:
                        purchaseReponse.Type = PurchaseResponseType.PurchaseFailed;
                        break;
                }

                return purchaseReponse;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, $"Failure purchasing product {productId}", e);
                purchaseReponse.Type = PurchaseResponseType.PurchaseFailed;
                return purchaseReponse;
            }
        }

        /// <summary>
        /// Activate and verify the MEGA product license on the MEGA license server
        /// </summary>
        /// <param name="receipt">Windows Store product purchase receipt</param>
        /// <returns>True if activation has been successful, False if not succeeded</returns>
        public static async Task<bool> ActivateMegaLicenseAsync(string receipt)
        {
            if (string.IsNullOrEmpty(receipt)) return false;

            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Activating license on the MEGA License Server...");
            LogService.Log(MLogLevel.LOG_LEVEL_DEBUG, "License info: " + receipt);

            // Validate and activate the MEGA Windows Store (int 13) subscription on the MEGA license server
            var submitPurchaseReceipt = new SubmitPurchaseReceiptRequestListenerAsync();
            var result = await submitPurchaseReceipt.ExecuteAsync(() =>
            {
                // If user has accessed a public node in the last 24 hours, also send the node handle (Task #10801)
                var lastPublicNodeHandle = SettingsService.GetLastPublicNodeHandle();
                if (lastPublicNodeHandle.HasValue)
                {
                    SdkService.MegaSdk.submitPurchaseReceiptWithLastPublicHandle(
                        (int)MPaymentMethod.PAYMENT_METHOD_WINDOWS_STORE,
                        receipt, lastPublicNodeHandle.Value, submitPurchaseReceipt);
                    return;
                }

                SdkService.MegaSdk.submitPurchaseReceipt(
                    (int) MPaymentMethod.PAYMENT_METHOD_WINDOWS_STORE,
                    receipt, submitPurchaseReceipt);
            });

            // If succeeded, save the receipt Id for later checks
            if (result) SaveUniqueReceiptId(receipt);

            return result;
        }

        /// <summary>
        /// (Re-)validate all the current user product licenses
        /// </summary>
        public static async Task ValidateLicensesAsync()
        {
            try
            {
                // If no Internet connection, stop the check
                if (!NetworkService.HasInternetAccess()) return;
                // If the Windows Store product listing is not available, stop the check
                var available = await GetIsAvailableAsync();
                if (!available) return;

                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Validating licenses...");

                foreach (var productLicense in CurrentLicenseInformation.ProductLicenses)
                {
                    if (!productLicense.Value.IsActive) continue;
                    await CheckLicenseAsync(productLicense.Key);
                }
            }
            catch (Exception e)
            {
                // If an error occurs, ignore. App will try again on restart
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error validating licenses", e);
            }
        }

        /// <summary>
        /// Check if the product is already activated on MEGA license server
        /// </summary>
        /// <param name="productId">Product identifier</param>
        private static async Task CheckLicenseAsync(string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId)) return;
#if DEBUG
                var receipt = await CurrentAppSimulator.GetProductReceiptAsync(productId);
#else
                var receipt = await CurrentApp.GetProductReceiptAsync(productId);
#endif
                if (string.IsNullOrEmpty(receipt)) return;

                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Checking product license...");
                LogService.Log(MLogLevel.LOG_LEVEL_DEBUG, "License info: " + receipt);

                if (!CheckReceiptIdStatus(receipt))
                {
                    await ActivateMegaLicenseAsync(receipt);
                }
            }
            catch (Exception e)
            {
                // If an error occurs, ignore. App will try again on restart
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error (re-)checking licenses", e);
            }
        }

        /// <summary>
        /// Get unique identifier from Windows Store receipt xml
        /// </summary>
        /// <param name="receipt">Windows Store receipt xml</param>
        /// <returns>Unique receipt identifier</returns>
        public static string GetUniqueReceiptId(string receipt)
        {
            if (string.IsNullOrEmpty(receipt)) return null;

            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Retrieving unique receipt ID...");

            try
            {
                var xDoc = XDocument.Parse(receipt, LoadOptions.None);
                return xDoc.Root?.Descendants().First().Attribute("Id")?.Value;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Failed to retrieve unique receipt ID", e);
                return null;
            }
        }

        /// <summary>
        /// Save the unique receipt id to local user settings
        /// </summary>
        /// <param name="receipt">Windows Store receipt xml</param>
        /// <returns>True if save succeeded, else False</returns>
        private static bool SaveUniqueReceiptId(string receipt)
        {
            try
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Saving receipt ID...");

                var key = ResourceService.SettingsResources.GetString("SR_Receipts");
                var currentIds = SettingsService.Load(key, string.Empty);
                var id = GetUniqueReceiptId(receipt);

                if (id == null) return false;
                
                currentIds += id + ";";
                SettingsService.Save(key, currentIds);

                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Failed to save receipt ID", e);
                return false;
            }
        }

        /// <summary>
        /// Check if unique receipt id is already stored in local user settings
        /// </summary>
        /// <param name="receipt">Windows Store receipt xml</param>
        /// <returns>True if already stored in settings, else False</returns>
        private static bool CheckReceiptIdStatus(string receipt)
        {
            try
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Checking receipt ID status...");

                var uniqueId = GetUniqueReceiptId(receipt);
                // return true to stop activation of receipt
                if (uniqueId == null) return true;

                var key = ResourceService.SettingsResources.GetString("SR_Receipts");
                var currentIds = SettingsService.Load(key, string.Empty);
                var currentIdsList = currentIds.Split(';');

                return currentIdsList.Any(id => id.Equals(uniqueId));
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Failed to check receipt ID status", e);
                return false;
            }
        }
    }
}
