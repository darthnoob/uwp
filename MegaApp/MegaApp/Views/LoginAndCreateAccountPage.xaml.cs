using System;
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
            AppService.SetAppViewBackButtonVisibility(false);

            // If exists a navigation object
            if (e?.Parameter != null)
            {
                var navObj = NavigateService.GetNavigationObject(e.Parameter);
                NavigationActionType navActionType = navObj.Action;

                // Try to avoid display duplicate alerts
                if (isAlertAlreadyDisplayed) return;
                isAlertAlreadyDisplayed = true;

                switch (navActionType)
                {
                    case NavigationActionType.Recovery:
                        this.ViewModel.LoginViewModel.Email = navObj.Parameters[NavigationParamType.Email] as string;
                        this.ViewModel.LoginViewModel.Password = navObj.Parameters[NavigationParamType.Password] as string;
                        break;

                    case NavigationActionType.API_ESID:
                        // Show a message notifying the error
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_SessionIDError_Title"),
                            ResourceService.AppMessages.GetString("AM_SessionIDError"));
                        break;

                    case NavigationActionType.API_EBLOCKED:
                        string message;
                        switch ((AccountBlockedReason) navObj.Parameters[NavigationParamType.Number])
                        {
                            case AccountBlockedReason.Copyright:
                                message = ResourceService.AppMessages.GetString("AM_AccountBlockedCopyright");
                                break;
                            case AccountBlockedReason.OtherReason:
                            default:
                                message = ResourceService.AppMessages.GetString("AM_AccountBlocked");
                                break;
                        }
                        
                        // Show a message notifying the error
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_AccountBlocked_Title"), message);
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

            if (DebugService.DebugSettings.IsDebugMode && DebugService.DebugSettings.ShowDebugAlert)
                DialogService.ShowDebugModeAlert();
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

            // On enter in password box. Start the login process
            this.ViewModel?.LoginViewModel?.Login();

            e.Handled = true;
        }

        private void OnMegaHeaderLogoTapped(object sender, TappedRoutedEventArgs e)
        {
            DebugService.ChangeStatusAction();
        }
    }
}
