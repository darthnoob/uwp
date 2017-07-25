using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using mega;
using MegaApp.Classes;
using MegaApp.UserControls;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Views.MyAccount
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseUpgradeView : UserControlEx<UpgradeViewModel> { }

    public sealed partial class UpgradeView : BaseUpgradeView
    {
        public UpgradeView()
        {
            this.InitializeComponent();
        }

        public StackPanel ViewArea => this.MainStackPanel;

        private void OnUpgradeBackButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            switch (this.ViewModel.CurrentStep)
            {
                case 2:
                    this.ViewModel.Step1();
                    this.PlansGrid.SelectedItem = this.PlansList.SelectedItem = null;
                    break;
                case 3:
                    this.ViewModel.Step2();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnProPlanSelected(object sender, TappedRoutedEventArgs e)
        {
            var selector = sender as ListViewBase;
            if (selector == null) return;

            this.ViewModel.SelectedPlan = ((ProductBase)selector.SelectedItem);
            this.ViewModel.Step2();

            // Set the monthly product as the default option
            this.MonthlyRadioButton.IsChecked = true;
            this.ViewModel.SelectedProduct = this.ViewModel.MonthlyProduct;
        }

        private void OnMembershipRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == null) return;

            switch (radioButton.Tag.ToString())
            {
                case "Monthly":
                    this.ViewModel.SelectedProduct = this.ViewModel.MonthlyProduct;
                    break;
                case "Annual":
                    this.ViewModel.SelectedProduct = this.ViewModel.AnnualProduct;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetDefaultPaymentMethod();
        }

        private void OnPaymentMethodRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == null) return;

            switch (radioButton.Tag.ToString())
            {
                case "Centili":
                    this.ViewModel.SelectedPaymentMethod = MPaymentMethod.PAYMENT_METHOD_CENTILI;
                    break;
                case "Fortumo":
                    this.ViewModel.SelectedPaymentMethod = MPaymentMethod.PAYMENT_METHOD_FORTUMO;
                    break;
                case "InAppPurchase":
                    this.ViewModel.SelectedPaymentMethod = MPaymentMethod.PAYMENT_METHOD_WINDOWS_STORE;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetDefaultPaymentMethod()
        {
            var selectedProduct = this.ViewModel.SelectedProduct;
            if (selectedProduct == null) return;

            if (selectedProduct.IsInAppPaymentMethodAvailable)
                this.InAppPurchaseRadioButton.IsChecked = true;
            else if (selectedProduct.IsFortumoPaymentMethodAvailable)
                this.FortumoRadioButton.IsChecked = true;
            else if (selectedProduct.IsCentiliPaymentMethodAvailable)
                this.CentiliRadioButton.IsChecked = true;
        }
    }
}
