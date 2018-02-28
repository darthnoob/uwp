using System.Collections.ObjectModel;
using MegaApp.Classes;

namespace MegaApp.ViewModels
{
    public class UpgradeAccountViewModel : BaseUiViewModel
    {
        public UpgradeAccountViewModel()
        {
            Plans = new ObservableCollection<ProductBase>();
            Products = new ObservableCollection<Product>();
        }

        public ObservableCollection<ProductBase> Plans { get; set; }
        public ObservableCollection<Product> Products { get; set; }

        private static string _LiteMonthlyFormattedPrice;
        public string LiteMonthlyFormattedPrice
        {
            get { return _LiteMonthlyFormattedPrice; }
            set { SetField(ref _LiteMonthlyFormattedPrice, value); }
        }

        private bool _isCentiliPaymentMethodAvailable;
        public bool IsCentiliPaymentMethodAvailable
        {
            get { return _isCentiliPaymentMethodAvailable; }
            set { SetField(ref _isCentiliPaymentMethodAvailable, value); }
        }

        private bool _isFortumoPaymentMethodAvailable;
        public bool IsFortumoPaymentMethodAvailable
        {
            get { return _isFortumoPaymentMethodAvailable; }
            set { SetField(ref _isFortumoPaymentMethodAvailable, value); }
        }

        //private bool _isCreditCardPaymentMethodAvailable;
        //public bool IsCreditCardPaymentMethodAvailable
        //{
        //    get { return _isCreditCardPaymentMethodAvailable; }
        //    set { SetField(ref _isCreditCardPaymentMethodAvailable, value); }
        //}

        private bool _isInAppPaymentMethodAvailable;
        public bool IsInAppPaymentMethodAvailable
        {
            get { return _isInAppPaymentMethodAvailable; }
            set { SetField(ref _isInAppPaymentMethodAvailable, value); }
        }
    }
}
