using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseMainPage : PageEx<MainViewModel> { }

    public sealed partial class MainPage : BaseMainPage
    {
        public MainPage()
        {
            InitializeComponent();
            // Set the main navigation frame object to the HamburgerMenu ContentFrame
            NavigateService.MainFrame = this.ContentFrame;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ContentFrame.Navigated += ContentFrameOnNavigated;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            NavigationObject navObj = NavigateService.GetNavigationObject(e.Parameter) as NavigationObject;

            this.ViewModel.Initialize((navObj != null) ? navObj.Action : NavigationActionType.Default);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ContentFrame.Navigated -= ContentFrameOnNavigated;
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
            base.OnNavigatedFrom(e);
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            // If the content frame is the CloudDrivePage, is needed that it process the back request before
            if ((ContentFrame.Content as Page).GetType().Equals(typeof(CloudDrivePage)) && 
                OnBackRequestedCloudDrive(args.Handled))
            {
                args.Handled = true;
                return;
            }

            SetAppViewBackButtonVisibility(ContentFrame.CanGoBack);

            // Navigate back if possible, and if the event has not already been handled
            if (!ContentFrame.CanGoBack) return;
            args.Handled = true;
            ContentFrame.GoBack();
        }

        private bool OnBackRequestedCloudDrive(bool isHandled)
        {
            var cloudDriveViewModel = ViewModel.ContentViewModel as CloudDriveViewModel;
            var previousActiveFolderView = cloudDriveViewModel.ActiveFolderView;

            if ((ContentFrame.Content as CloudDrivePage).ProcessBackRequest(isHandled))
            {
                if (previousActiveFolderView.Equals(cloudDriveViewModel.CloudDrive))
                    SetAppViewBackButtonVisibility(cloudDriveViewModel.CloudDrive.CanGoFolderUp() || ContentFrame.CanGoBack);

                return true;
            }

            return false;
        }

        private void ContentFrameOnNavigated(object sender, NavigationEventArgs e)
        {
            // Show the back button in desktop and tablet applications
            // Back button in mobile applications is automatic in the nav bar on screen
            SetAppViewBackButtonVisibility(ContentFrame.CanGoBack);

            // Set current content viewmodel as property of the main page
            // Could be handy in the future
            this.ViewModel.ContentViewModel = (this.ContentFrame.Content as Page)?.DataContext as BasePageViewModel;

            if (e.NavigationMode != NavigationMode.Back) return;
            // Set current menu or option item 
            this.ViewModel.SelectedItem = this.ViewModel.MenuItems.FirstOrDefault(
                m => NavigateService.GetViewType(m.TargetViewModel) == ContentFrame.CurrentSourcePageType);
            this.ViewModel.SelectedOptionItem = this.ViewModel.OptionItems.FirstOrDefault(
                m => NavigateService.GetViewType(m.TargetViewModel) == ContentFrame.CurrentSourcePageType);
        }

        private void SetAppViewBackButtonVisibility(bool isVisible)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
               isVisible ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }
    }
}
