using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using mega;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Services;

namespace MegaApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MainPageViewModel _mainPageViewModel;

        public MainPage()
        {
            _mainPageViewModel = new MainPageViewModel(App.MegaSdk, App.AppInformation);
            this.DataContext = _mainPageViewModel;

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
            bool isAlreadyOnline = Convert.ToBoolean(App.MegaSdk.isLoggedIn());
            if (!isAlreadyOnline)
            {
                if (! await SettingsService.HasValidSession())
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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

            App.MegaSdk.logout(new LogOutRequestListener());
        }
    }
}
