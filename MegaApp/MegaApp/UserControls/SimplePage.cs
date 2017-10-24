using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.UserControls
{
    public class SimplePage<T>: PageEx<T>
        where T : BasePageViewModel, new()
    {
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            // Show the back button in desktop and tablet applications
            // Back button in mobile applications is automatic in the navigation bar on screen
            AppService.SetAppViewBackButtonVisibility(true);

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
            base.OnNavigatedFrom(e);
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            args.Handled = true;
            NavigateService.Instance.GoBack(true);
        }
    }
}
