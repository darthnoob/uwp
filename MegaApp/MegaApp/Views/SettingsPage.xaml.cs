using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.UserControls;
using MegaApp.Services;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Initialize();
        }

        private void OnSdkVersionTapped(object sender, TappedRoutedEventArgs e)
        {
            DebugService.ChangeStatusAction();
        }
    }
}
