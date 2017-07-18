using System;
using System.Linq;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.MyAccount
{
    public class UpgradeViewModel : BaseUiViewModel
    {
        private const int TotalSteps = 3;

        public UpgradeViewModel()
        {
            this.MembershipDurationSelectedCommand = new RelayCommand(MembershipDurationSelected);
            this.PaymentMethodSelectedCommand = new RelayCommand(PaymentMethodSelected);

            this.Step1();
        }

        #region Commands

        public ICommand MembershipDurationSelectedCommand { get; }
        public ICommand PaymentMethodSelectedCommand { get; }

        #endregion

        #region Public Methods

        public void Step1()
        {
            this.CurrentStep = 1;
            this.StepBreadcrumText = string.Format(ResourceService.UiResources.GetString("UI_UpgradeStepCounter"), 
                this.CurrentStep, TotalSteps);
            this.StepTitleText = ResourceService.UiResources.GetString("UI_UpgradeStep1");
            this.BackButtonVisibility = Visibility.Collapsed;
            this.SelectedPlan = this.SelectedProduct = null;
        }

        public void Step2()
        {
            this.CurrentStep = 2;
            this.StepBreadcrumText = ResourceService.UiResources.GetString("UI_UpgradeStep1");
            this.StepTitleText = ResourceService.UiResources.GetString("UI_UpgradeStep2");
            this.BackButtonVisibility = Visibility.Visible;           
        }

        public void Step3()
        {
            this.CurrentStep = 3;
            this.StepBreadcrumText = ResourceService.UiResources.GetString("UI_UpgradeStep2");
            this.StepTitleText = ResourceService.UiResources.GetString("UI_UpgradeStep3");
            this.BackButtonVisibility = Visibility.Visible;
        }

        #endregion

        #region Private Methods

        private void MembershipDurationSelected()
        {
            this.Step3();
        }

        private async void PaymentMethodSelected()
        {
            switch(this.SelectedPaymentMethod)
            {
                case MPaymentMethod.PAYMENT_METHOD_CENTILI:
                case MPaymentMethod.PAYMENT_METHOD_FORTUMO:
                    var paymentUrlRequestListener = new GetPaymentUrlRequestListenerAsync();
                    var paymentUrl = await paymentUrlRequestListener.ExecuteAsync(() =>
                        SdkService.MegaSdk.getPaymentId(SelectedProduct.Handle, paymentUrlRequestListener));

                    if (this.SelectedPaymentMethod == MPaymentMethod.PAYMENT_METHOD_CENTILI)
                        paymentUrl = "https://www.centili.com/widget/WidgetModule?api=9e8eee856f4c048821954052a8d734ac&clientid=" + paymentUrl;
                    if (this.SelectedPaymentMethod == MPaymentMethod.PAYMENT_METHOD_FORTUMO)
                        paymentUrl = "http://fortumo.com/mobile_payments/f250460ec5d97fd27e361afaa366db0f?cuid=" + paymentUrl;

                    await Launcher.LaunchUriAsync(new Uri(paymentUrl, UriKind.RelativeOrAbsolute));
                    break;

                case MPaymentMethod.PAYMENT_METHOD_WINDOWS_STORE:
                    var purchaseResponse = await LicenseService.PurchaseProductAsync(
                        await LicenseService.GetProductIdAsync(this.SelectedProduct.MicrosoftStoreId));

                    string title = string.Empty, message = string.Empty;
                    switch(purchaseResponse.Type)
                    {
                        case PurchaseResponseType.Unknown:
                        case PurchaseResponseType.PurchaseFailed:
                            title = ResourceService.AppMessages.GetString("AM_PurchaseFailed_Title");
                            message = ResourceService.AppMessages.GetString("AM_PurchaseFailed");
                            break;

                        case PurchaseResponseType.UnAvailable:
                            title = ResourceService.AppMessages.GetString("AM_ProductUnAvailable_Title");
                            message = ResourceService.AppMessages.GetString("AM_ProductUnAvailable");
                            break;

                        case PurchaseResponseType.AlreadyPurchased:
                            title = ResourceService.AppMessages.GetString("AM_AlreadyPurchased_Title");
                            message = ResourceService.AppMessages.GetString("AM_AlreadyPurchased");
                            break;

                        case PurchaseResponseType.Succeeded:
                            title = ResourceService.AppMessages.GetString("AM_PurchaseSucceeded_Title");
                            message = ResourceService.AppMessages.GetString("AM_PurchaseSucceeded");
                            break;

                        default:
                            throw new ArgumentOutOfRangeException("PurchaseResponse.Type");
                    }

                    this.OnUiThread(() => new CustomMessageDialog(title, message, App.AppInformation).ShowDialog());
                    break;
            }
        }

        private void SetProducts()
        {
            for (int i = 0; i < UpgradeAccount.Products.Count; i++)
            {
                if (UpgradeAccount.Products.ElementAt(i).AccountType == SelectedPlan.AccountType)
                {
                    switch (UpgradeAccount.Products.ElementAt(i).Months)
                    {
                        case 1:
                            MonthlyProduct = UpgradeAccount.Products.ElementAt(i);
                            break;

                        case 12:
                            AnnualProduct = UpgradeAccount.Products.ElementAt(i);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        #endregion

        #region Properties

        public UpgradeAccountViewModel UpgradeAccount => AccountService.UpgradeAccount;

        private int _currentStep;
        public int CurrentStep
        {
            get { return _currentStep; }
            set { SetField(ref _currentStep, value); }
        }

        private string _stepBreadcrumText;
        public string StepBreadcrumText
        {
            get { return _stepBreadcrumText; }
            set { SetField(ref _stepBreadcrumText, value); }
        }

        private string _stepTitleText;
        public string StepTitleText
        {
            get { return _stepTitleText; }
            set { SetField(ref _stepTitleText, value); }
        }

        private Visibility _backButtonVisibility;
        public Visibility BackButtonVisibility
        {
            get { return _backButtonVisibility; }
            set { SetField(ref _backButtonVisibility, value); }
        }

        private ProductBase _selectedPlan;
        public ProductBase SelectedPlan
        {
            get { return _selectedPlan; }
            set
            {
                SetField(ref _selectedPlan, value);
                if (_selectedPlan == null) return;
                SetProducts();
            }
        }

        private Product _monthlyProduct;
        public Product MonthlyProduct
        {
            get { return _monthlyProduct; }
            set
            {
                SetField(ref _monthlyProduct, value);
                OnPropertyChanged("MonthlyProductPrice");
                OnPropertyChanged("SavedMoney");
            }
        }

        private Product _annualProduct;
        public Product AnnualProduct
        {
            get { return _annualProduct; }
            set
            {
                SetField(ref _annualProduct, value);
                OnPropertyChanged("AnnualProductPrice");
                OnPropertyChanged("SavedMoney");
            }
        }

        public string MonthlyProductPrice => (MonthlyProduct == null) ? string.Empty :
            string.Format("{0} {1}", MonthlyProduct.PriceAndCurrency, MonthlyProduct.Period.ToLower());

        public string AnnualProductPrice => (AnnualProduct == null) ? string.Empty :
            string.Format("{0} {1}", AnnualProduct.PriceAndCurrency, AnnualProduct.Period.ToLower());

        public string SavedMoney => (MonthlyProduct == null) || (AnnualProduct == null) ? string.Empty :
            string.Format(ResourceService.UiResources.GetString("UI_SaveMoney"),
                string.Format("{0} {1}", (int)Math.Ceiling((MonthlyProduct.Price*12)-AnnualProduct.Price), AnnualProduct.Currency));

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                SetField(ref _selectedProduct, value);
                OnPropertyChanged("IsCentiliPaymentMethodAvailable");
                OnPropertyChanged("IsFortumoPaymentMethodAvailable");
                OnPropertyChanged("IsInAppPaymentMethodAvailable");
                OnPropertyChanged("IsPurchaseButtonEnabled");
            }
        }

        public bool IsCentiliPaymentMethodAvailable => SelectedProduct == null ? false :
            this.SelectedProduct.IsCentiliPaymentMethodAvailable;

        public bool IsFortumoPaymentMethodAvailable => SelectedProduct == null ? false :
            this.SelectedProduct.IsFortumoPaymentMethodAvailable;

        public bool IsInAppPaymentMethodAvailable => SelectedProduct == null ? false :
            this.SelectedProduct.IsInAppPaymentMethodAvailable;

        private MPaymentMethod _selectedPaymentMethod;
        public MPaymentMethod SelectedPaymentMethod
        {
            get { return _selectedPaymentMethod; }
            set { SetField(ref _selectedPaymentMethod, value); }
        }

        public bool IsPurchaseButtonEnabled
        {
            get
            {
                if (!IsCentiliPaymentMethodAvailable && !IsFortumoPaymentMethodAvailable &&
                    !IsInAppPaymentMethodAvailable)
                    return false;

                return true;
            }
        }

        #endregion

        #region AppResources

        public Uri RefundPolicyUri => new Uri(ResourceService.AppResources.GetString("AR_RefundPolicyUri"));

        #endregion

        #region UiResources

        // Title
        public string Title => ResourceService.UiResources.GetString("UI_Upgrade");

        // Step 2. Select membership duration
        public string MembershipDurationText => ResourceService.UiResources.GetString("UI_MembershipDuration");
        public string NextText => ResourceService.UiResources.GetString("UI_Next");

        // Step 3. Select payment method
        public string PaymentMethodText => ResourceService.UiResources.GetString("UI_PaymentMethod");
        public string PurchaseText => ResourceService.UiResources.GetString("UI_Purchase");
        public string CentiliText => ResourceService.UiResources.GetString("UI_CentiliPaymentMethod");
        public string FortumoText => ResourceService.UiResources.GetString("UI_FortumoPaymentMethod");
        public string CreditCardText => ResourceService.UiResources.GetString("UI_CreditCard");
        public string InAppPurchaseText => ResourceService.UiResources.GetString("UI_InAppPurchasePaymentMethod");

        // Refund policy
        public string RefundPolicyText => ResourceService.UiResources.GetString("UI_RefundPolicy");
        public string RefundPolicyDescriptionText => ResourceService.UiResources.GetString("UI_RefundPolicyDescription");
        public string SeeMoreInfoText => ResourceService.UiResources.GetString("UI_SeeMoreInfo");

        #endregion

        #region VisualResources

        public string BackIconPathData => ResourceService.VisualResources.GetString("VR_BackIconPathData");

        #endregion
    }
}
