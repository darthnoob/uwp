using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using MegaApp.ViewModels;
using MegaApp.UserControls;
using MegaApp.Services;

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
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            SdkService.MegaSdk.retryPendingConnections();
        }
    }
}
