using System;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseMainPage : PageEx<MainPageViewModel> { }

    public sealed partial class MainPage : BaseMainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // Code to execute when a Network change is detected.        
        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            UpdateGUI(await NetworkService.IsNetworkAvailable());
            throw new NotImplementedException();
        }

        private void UpdateGUI(bool isNetworkConnected = true)
        {
            
        }

        private async Task<bool> CheckActiveAndOnlineSession(NavigationMode navigationMode = NavigationMode.New)
        {
            bool isAlreadyOnline = Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn());
            if (!isAlreadyOnline)
            {
                if (! await SettingsService.HasValidSession())
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        (Window.Current.Content as Frame).Navigate(typeof(LoginAndCreateAccountPage), NavigationParameter.Normal));

                    return false;
                }
            }

            return true;
        }

        private bool CheckPinLock()
        {
            //if (!App.AppInformation.HasPinLockIntroduced && SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled))
            //{
            //    NavigateService.NavigateTo(typeof(PasswordPage), NavigationParameter.Normal, this.GetType());
            //    return false;
            //}

            return true;
        }

        private async Task<bool> CheckSessionAndPinLock(NavigationMode navigationMode = NavigationMode.New)
        {
            if (!await CheckActiveAndOnlineSession(navigationMode)) return false;
            if (!CheckPinLock()) return false;
            return true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Un-Subscribe to the NetworkStatusChanged event
            NetworkInformation.NetworkStatusChanged -= NetworkInformation_NetworkStatusChanged;

            //_mainPageViewModel.Deinitialize(App.GlobalDriveListener);
            base.OnNavigatedFrom(e);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Subscribe to the NetworkStatusChanged event            
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            // Need to check it always and no only in StartupMode, 
            // because this is the first page loaded
            if (!await CheckSessionAndPinLock(e.NavigationMode)) return;
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnCloudDriveItemTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void OnListLoaded(object sender, RoutedEventArgs e)
        {

        }

        private async void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            if (! await NetworkService.IsNetworkAvailable(true)) return;

            SdkService.MegaSdk.logout(new LogOutRequestListener());
        }
    }
}
