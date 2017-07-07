using System;
using System.Runtime.Serialization;
using Windows.UI;
using Windows.UI.Xaml.Media;
using mega;
using MegaApp.Extensions;
using MegaApp.Services;

namespace MegaApp.Classes
{
    // "DataContact" and "DataMember" necessary for serialization during app deactivation
    // when the app opened the Web Browser for the Fortumo Payment
    [DataContract]
    public class ProductBase
    {
        [DataMember] public MAccountType AccountType { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public int Amount { get; set; }
        [DataMember] public string Currency { get; set; }
        [DataMember] public int GbStorage { get; set; }
        [DataMember] public int GbTransfer { get; set; }
        [DataMember] public string ProductPathData { get; set; }
        [DataMember] public Color ProductColor { get; set; }

        public SolidColorBrush ProductColorBrush 
        {
            get { return new SolidColorBrush(ProductColor); }
            set { ProductColor = value.Color; }
        }

        public string Storage => AccountType == MAccountType.ACCOUNT_TYPE_FREE ?
            "50 GB" : Convert.ToUInt64(GbStorage).FromGBToBytes().ToStringAndSuffix();

        public string Transfer => AccountType == MAccountType.ACCOUNT_TYPE_FREE ?
            ResourceService.UiResources.GetString("UI_Dynamic") : 
            Convert.ToUInt64(GbTransfer).FromGBToBytes().ToStringAndSuffix();

        public double Price => (double)Amount/100;
        public string PriceAndCurrency => string.Format("{0:N} {1}", Price, Currency);

        public string PriceIntegerPart => Price.ToString().Split('.')[0];
        public string PriceDecimalPart => Price.ToString().Split('.')[1];

        #region UiResources

        public virtual string PricePeriod => ResourceService.UiResources.GetString("UI_PriceMonthly");
        public string StorageText => ResourceService.UiResources.GetString("UI_Storage");
        public string TransferQuotaText => ResourceService.UiResources.GetString("UI_TransferQuota");

        #endregion
    }
}
