using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseSavedForOfflinePage : PageEx<SavedForOfflineViewModel> { }

    public sealed partial class SavedForOfflinePage : BaseSavedForOfflinePage
    {
        public SavedForOfflinePage()
        {
            this.InitializeComponent();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();
        }

        private void OnSortClick(object sender, RoutedEventArgs e)
        {
            var sortButton = sender as Button;
            if (sortButton == null) return;

            MenuFlyout menuFlyout = DialogService.CreateSortMenu(ViewModel.SavedForOffline);

            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(sortButton);
        }
    }
}
