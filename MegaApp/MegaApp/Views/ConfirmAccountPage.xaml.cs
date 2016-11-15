using System;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;
using Windows.UI.Core;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseConfirmAccountPage : PageEx<ConfirmAccountViewModel> {}

    public sealed partial class ConfirmAccountPage : BaseConfirmAccountPage
    {
        private readonly ConfirmAccountViewModel _confirmAccountViewModel;
        public ConfirmAccountPage()
        {
            _confirmAccountViewModel = new ConfirmAccountViewModel();
            this.DataContext = _confirmAccountViewModel;

            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            // Show the back button in desktop and tablet applications
            // Back button in mobile applications is automatic in the nav bar on screen
            AppService.SetAppViewBackButtonVisibility(true);

            if (Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn()))
            {
                var waitHandleLogout = new AutoResetEvent(false);
                SdkService.MegaSdk.logout(new LogOutRequestListener(false, waitHandleLogout));
                waitHandleLogout.WaitOne();
            }

            if (App.LinkInformation.ActiveLink.Contains("#confirm"))
            {
                _confirmAccountViewModel.ConfirmLink = App.LinkInformation.ActiveLink;
                SdkService.MegaSdk.querySignupLink(_confirmAccountViewModel.ConfirmLink,
                    new ConfirmAccountRequestListener(_confirmAccountViewModel));

                App.LinkInformation.Reset();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
            base.OnNavigatedFrom(e);
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            if (args.Handled) return;

            App.LinkInformation.Reset();
            (Window.Current.Content as Frame).Navigate(typeof(LoginAndCreateAccountPage));

            args.Handled = true;
        }
    }
}