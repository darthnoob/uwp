using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MegaApp.ViewModels;
using MegaApp.Services;
using MegaApp.UserControls;

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
            this.ViewModel.GoToUpgrade += GoToUpgrade;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.GoToUpgrade -= GoToUpgrade;
            base.OnNavigatedTo(e);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            SdkService.MegaSdk.retryPendingConnections();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(e.NewSize.Width > 600)
                this.GeneralStackPanel.Width = e.NewSize.Width * 2 / 3;
            else
                this.GeneralStackPanel.Width = this.MyAccountPivot.Width;
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
