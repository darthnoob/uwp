using Windows.UI.Xaml;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseSettingsPage : PageEx<SettingsViewModel> { }
    public sealed partial class SettingsPage : BaseSettingsPage
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            SdkService.MegaSdk.logout(new LogOutRequestListener());
        }
    }
}
