using System.Runtime.Serialization;
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
    }
}
