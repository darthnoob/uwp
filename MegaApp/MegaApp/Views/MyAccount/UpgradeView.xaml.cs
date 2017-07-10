using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MegaApp.Views.MyAccount
{
    public sealed partial class UpgradeView : UserControl
    {
        public EventHandler<TappedRoutedEventArgs> UpgradeBackButtonTapped;
        public EventHandler<TappedRoutedEventArgs> ProPlanSelected;
        public EventHandler<RoutedEventArgs> MembershipRadioButtonChecked;
        public EventHandler<RoutedEventArgs> PaymentMethodRadioButtonChecked;

        public UpgradeView()
        {
            this.InitializeComponent();
        }

        public StackPanel MainStackPanel => this.PART_MainStackPanel;
        public GridView PlansGrid => this.PART_PlansGrid;
        public ListView PlansList => this.PART_PlansList;
        public RadioButton MonthlyRadioButton => this.PART_MonthlyRadioButton;
        public RadioButton InAppPurchaseRadioButton => this.PART_InAppPurchaseRadioButton;
        public RadioButton FortumoRadioButton => this.PART_FortumoRadioButton;
        public RadioButton CentiliRadioButton => this.PART_CentiliRadioButton;

        private void OnUpgradeBackButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            UpgradeBackButtonTapped?.Invoke(sender, e);
        }

        private void OnProPlanSelected(object sender, TappedRoutedEventArgs e)
        {
            ProPlanSelected?.Invoke(sender, e);
        }

        private void OnMembershipRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            MembershipRadioButtonChecked?.Invoke(sender, e);
        }

        private void OnPaymentMethodRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            PaymentMethodRadioButtonChecked?.Invoke(sender, e);
        }
    }
}
