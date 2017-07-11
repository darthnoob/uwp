using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseMyAccountPage : PageEx<MyAccountViewModel> { }

    public sealed partial class MyAccountPage : BaseMyAccountPage
    {
        public MyAccountPage()
        {
            this.InitializeComponent();

            this.MainGrid.SizeChanged += OnSizeChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Initialize();
            this.GeneralView.ViewModel.GoToUpgrade += GoToUpgrade;
            this.StorageAndTransferView.ViewModel.GoToUpgrade += GoToUpgrade;

            var navObj = NavigateService.GetNavigationObject(e.Parameter) as NavigationObject;
            var navActionType = navObj?.Action ?? NavigationActionType.Default;
            if (navActionType == NavigationActionType.Upgrade)
                this.MyAccountPivot.SelectedItem = this.UpgradePivot;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.GeneralView.ViewModel.GoToUpgrade -= GoToUpgrade;
            this.StorageAndTransferView.ViewModel.GoToUpgrade -= GoToUpgrade;
            base.OnNavigatedTo(e);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            SdkService.MegaSdk.retryPendingConnections();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 600)
            {
                this.GeneralView.ViewArea.Width = 600;

                this.ProfileView.ViewArea.Width = 600;

                this.StorageAndTransferView.ViewArea.Width = 600;
                this.StorageAndTransferView.ViewArea.HorizontalAlignment = HorizontalAlignment.Left;

                this.UpgradeView.ViewArea.Width = 600;
            }
            else
            {
                this.GeneralView.ViewArea.Width = this.MyAccountPivot.ActualWidth;

                this.ProfileView.ViewArea.Width = this.MyAccountPivot.ActualWidth;

                this.StorageAndTransferView.ViewArea.Width = this.MyAccountPivot.ActualWidth;
                this.StorageAndTransferView.ViewArea.HorizontalAlignment = HorizontalAlignment.Stretch;

                this.UpgradeView.ViewArea.Width = this.MyAccountPivot.ActualWidth;
            }
        }

        private void GoToUpgrade(object sender, EventArgs e)
        {
            this.MyAccountPivot.SelectedItem = this.UpgradePivot;
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
