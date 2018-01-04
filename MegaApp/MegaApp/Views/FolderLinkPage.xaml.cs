using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
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
        public FolderLinkPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ViewModel.LoginToFolder(App.LinkInformation.ActiveLink);
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
