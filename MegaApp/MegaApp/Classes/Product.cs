using System;
using System.Runtime.Serialization;
using mega;
using MegaApp.Services;

namespace MegaApp.Classes
{
    // "DataContact" and "DataMember" necessary for serialization during app deactivation
    // when the app opened the Web Browser for the Fortumo Payment
    [DataContract]
    public class Product : ProductBase
    {
        [DataMember] public int Months { get; set; }
        [DataMember] public ulong Handle { get; set; }

        public Product()
        {
            // Default values
            this.IsCentiliPaymentMethodAvailable = false;
            this.IsFortumoPaymentMethodAvailable = false;
            this.IsCreditCardPaymentMethodAvailable = false;
            this.IsInAppPaymentMethodAvailable = false;
        }
        
        public string Period => Months == 1 ?
            ResourceService.UiResources.GetString("UI_Month") :
            ResourceService.UiResources.GetString("UI_Year");

        public override string PricePeriod => Months == 1 ?
            ResourceService.UiResources.GetString("UI_PriceMonthly") :
            ResourceService.UiResources.GetString("UI_PriceAnnual");

        public bool IsCentiliPaymentMethodAvailable;
        public bool IsFortumoPaymentMethodAvailable;
        public bool IsCreditCardPaymentMethodAvailable;
        public bool IsInAppPaymentMethodAvailable;

        public string MicrosoftStoreId
        {
            get
            {
                switch (this.AccountType)
                {
                    case MAccountType.ACCOUNT_TYPE_FREE:
                        return null;

                    case MAccountType.ACCOUNT_TYPE_LITE:
                        switch(this.Months)
                        {
                            case 1:
                                return ResourceService.AppResources.GetString("AR_ProLiteMonth");
                            case 12:
                                return ResourceService.AppResources.GetString("AR_ProLiteYear");
                        }
                        break;

                    case MAccountType.ACCOUNT_TYPE_PROI:
                        switch (this.Months)
                        {
                            case 1:
                                return ResourceService.AppResources.GetString("AR_Pro1Month");
                            case 12:
                                return ResourceService.AppResources.GetString("AR_Pro1Year");
                        }
                        break;

                    case MAccountType.ACCOUNT_TYPE_PROII:
                        switch (this.Months)
                        {
                            case 1:
                                return ResourceService.AppResources.GetString("AR_Pro2Month");
                            case 12:
                                return ResourceService.AppResources.GetString("AR_Pro2Year");
                        }
                        break;

                    case MAccountType.ACCOUNT_TYPE_PROIII:
                        switch (this.Months)
                        {
                            case 1:
                                return ResourceService.AppResources.GetString("AR_Pro3Month");
                            case 12:
                                return ResourceService.AppResources.GetString("AR_Pro3Year");
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("AccountType");
                }

                return null;
            }
        }
    }
}
