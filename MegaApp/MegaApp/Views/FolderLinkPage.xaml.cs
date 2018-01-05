using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseFolderLinkPage : PageEx<FolderLinkViewModel> { }

    public sealed partial class FolderLinkPage : BaseFolderLinkPage
    {
        private const double InformationPanelMinWidth = 432;

        public FolderLinkPage()
        {
            this.InitializeComponent();

            this.FolderLinkSplitView.RegisterPropertyChangedCallback(
                SplitView.IsPaneOpenProperty, IsSplitViewOpenPropertyChanged);
        }

        private void IsSplitViewOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (this.ViewModel.FolderLink.IsPanelOpen)
            {
                if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop || this.FolderLinkSplitView.ActualWidth < 600)
                {
                    this.FolderLinkSplitView.OpenPaneLength = this.FolderLinkSplitView.ActualWidth;
                    AppService.SetAppViewBackButtonVisibility(true);
                    return;
                }

                switch (this.ViewModel.FolderLink.VisiblePanel)
                {
                    case PanelType.Information:
                        this.FolderLinkSplitView.OpenPaneLength = InformationPanelMinWidth;
                        break;
                }
            }

            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        public override bool CanGoBack
        {
            get
            {
                bool canGoBack = false;
                if (this.ViewModel?.FolderLink != null)
                {
                    canGoBack = this.ViewModel.FolderLink.IsPanelOpen ||
                        this.ViewModel.FolderLink.CanGoFolderUp();
                }

                return canGoBack;
            }
        }

        public override void GoBack()
        {
            if (FolderLinkSplitView.IsPaneOpen)
                this.ViewModel.FolderLink.ClosePanels();
            else if (this.ViewModel.FolderLink.CanGoFolderUp())
                this.ViewModel.FolderLink.GoFolderUp();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.FolderLink.FolderNavigatedTo -= OnFolderNavigatedTo;

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ViewModel.FolderLink.FolderNavigatedTo += OnFolderNavigatedTo;

            this.ViewModel.LoginToFolder(App.LinkInformation.ActiveLink);
        }

        private void OnFolderNavigatedTo(object sender, EventArgs eventArgs)
        {
            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        private void OnSortClick(object sender, RoutedEventArgs e)
        {
            var sortButton = sender as Button;
            if (sortButton == null) return;

            MenuFlyout menuFlyout = DialogService.CreateSortMenu(ViewModel.FolderLink);

            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(sortButton);
        }
    }
}
