using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseLoginAndCreateAccountPage : PageEx<LoginAndCreateAccountViewModel> {}
   
    public sealed partial class LoginAndCreateAccountPage : BaseLoginAndCreateAccountPage
    {
        /// <summary>
        /// Flag to try to avoid display duplicate alerts
        /// </summary>
        static private bool isAlertAlreadyDisplayed = false;

        public LoginAndCreateAccountPage()
        {
            InitializeComponent();

            UiService.SetStatusBarBackground((Color)Application.Current.Resources["MegaAppBackground"]);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);            

            // Do not allow user to go back to any previous page
            NavigateService.CoreFrame.BackStack.Clear();
            
            // If exists a navigation object
            if (e?.Parameter != null)
            {
                NavigationActionType navActionType = NavigateService.GetNavigationObject(e.Parameter).Action;

                // Try to avoid display duplicate alerts
                if (isAlertAlreadyDisplayed) return;
                isAlertAlreadyDisplayed = true;

                switch (navActionType)
                {
                    case NavigationActionType.API_ESID:
                        // Show a message notifying the error
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_SessionIDError_Title"),
                            ResourceService.AppMessages.GetString("AM_SessionIDError"));
                        break;

                    case NavigationActionType.API_EBLOCKED:
                        // Show a message notifying the error
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_AccountBlocked_Title"),
                            ResourceService.AppMessages.GetString("AM_AccountBlocked"));
                        break;

                    case NavigationActionType.API_ESSL:
                        // Show a message notifying the error
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_SSLKeyError_Title"),
                            ResourceService.AppMessages.GetString("AM_SSLKeyError"));
                        break;
                }

                isAlertAlreadyDisplayed = false;
            }
        }

        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            if (PivotLoginAndCreateAccount?.SelectedItem == PivotItemLogin)
            {
                this.ViewModel?.LoginViewModel?.Login();
                return;
            }
            // Else it is always create account
           this.ViewModel?.CreateAccountViewModel?.CreateAccount();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PivotLoginAndCreateAccount?.SelectedItem == PivotItemLogin)
            {
                this.ViewModel.ActiveViewModel = this.ViewModel.LoginViewModel;
                return;
            }
            // Else it is always create account
            this.ViewModel.ActiveViewModel = this.ViewModel.CreateAccountViewModel;
        }

        private void OnPasswordKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;

            if (!NetworkService.IsNetworkAvailable(true)) return;

            // On enter in password box. Start the login process
            this.ViewModel?.LoginViewModel?.Login();
        }
    }
}
