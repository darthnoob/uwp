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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Initialize();
            this.GeneralView.ViewModel.GoToUpgrade += GoToUpgrade;
            this.GeneralView.ViewModel.GoToAchievements += GoToAchievements;
            this.StorageAndTransferView.ViewModel.GoToUpgrade += GoToUpgrade;

            var navObj = NavigateService.GetNavigationObject(e.Parameter) as NavigationObject;
            var navActionType = navObj?.Action ?? NavigationActionType.Default;
            if (navActionType == NavigationActionType.Upgrade)
                this.MyAccountPivot.SelectedItem = this.UpgradePivot;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.GeneralView.ViewModel.GoToUpgrade -= GoToUpgrade;
            this.GeneralView.ViewModel.GoToAchievements -= GoToAchievements;
            this.StorageAndTransferView.ViewModel.GoToUpgrade -= GoToUpgrade;
            base.OnNavigatedTo(e);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            SdkService.MegaSdk.retryPendingConnections();
        }

        private void GoToUpgrade(object sender, EventArgs e)
        {
            this.MyAccountPivot.SelectedItem = this.UpgradePivot;
        }

        private void GoToAchievements(object sender, EventArgs e)
        {
            this.MyAccountPivot.SelectedItem = this.AchievementsPivot;
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
