using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseSharedFoldersPage : PageEx<SharedFoldersViewModel> { }

    public sealed partial class SharedFoldersPage : BaseSharedFoldersPage
    {
        public SharedFoldersPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Initialize();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.Deinitialize();
            base.OnNavigatedFrom(e);
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SharedFoldersPivot.SelectedItem.Equals(this.IncomingSharesPivot))
                this.ViewModel.ActiveView = this.ViewModel.IncomingShares;

            if (this.SharedFoldersPivot.SelectedItem.Equals(this.OutgoingSharesPivot))
                this.ViewModel.ActiveView = this.ViewModel.OutgoingShares;
        }

        private void OnSortClick(object sender, RoutedEventArgs e)
        {
            var sortButton = sender as Button;
            if (sortButton == null) return;

            MenuFlyout menuFlyout = null;
            if (this.SharedFoldersPivot.SelectedItem.Equals(this.IncomingSharesPivot))
                menuFlyout = DialogService.CreateIncomingSharedItemsSortMenu(this.ViewModel.IncomingShares);
            if (this.SharedFoldersPivot.SelectedItem.Equals(this.OutgoingSharesPivot))
                menuFlyout = DialogService.CreateOutgoingSharedItemsSortMenu(this.ViewModel.OutgoingShares);

            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(sortButton);
        }

        private void OnItemTapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void OnItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

        }

        private void OnRightItemTapped(object sender, RightTappedRoutedEventArgs e)
        {

        }
    }
}
