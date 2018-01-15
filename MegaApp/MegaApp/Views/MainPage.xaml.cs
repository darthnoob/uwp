using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;
using Windows.UI;

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

            UiService.SetStatusBarBackground((Color)Application.Current.Resources["MegaAppBarBackground"]);

            // Set the main navigation frame object to the HamburgerMenu ContentFrame
            NavigateService.MainFrame = this.ContentFrame;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ContentFrame.Navigated += ContentFrameOnNavigated;
            
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            var navObj = NavigateService.GetNavigationObject(e.Parameter) as NavigationObject;
            var navActionType = navObj?.Action ?? NavigationActionType.Default;

            // Check if the user has an active and online session, because this is the first page loaded
            if (!await AppService.CheckActiveAndOnlineSession(true)) return;

            // If user has an active and online session but is not logged in, resume the session
            if (!Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn()))
            {
                if (!await this.ViewModel.FastLogin()) return;
            }

            this.ViewModel.Initialize(navActionType);

            if (DebugService.DebugSettings.IsDebugMode && DebugService.DebugSettings.ShowDebugAlert)
                DialogService.ShowDebugModeAlert();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ContentFrame.Navigated -= ContentFrameOnNavigated;
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
            base.OnNavigatedFrom(e);
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            if (ContentFrame.ContentPage != null && 
                ContentFrame.ContentPage.CanGoBack)
            {
                ContentFrame.ContentPage.GoBack();
                AppService.SetAppViewBackButtonVisibility(
                    ContentFrame.ContentPage.CanGoBack ||
                    ContentFrame.CanGoBack);
                args.Handled = true;
                return;
            };

            // Navigate back if possible, and if the event has not already been handled
            if (!ContentFrame.CanGoBack) return;
            args.Handled = true;
            ContentFrame.GoBack();
        }

        private void ContentFrameOnNavigated(object sender, NavigationEventArgs e)
        {
            // Show the back button in desktop and tablet applications
            // Back button in mobile applications is automatic in the nav bar on screen
            AppService.SetAppViewBackButtonVisibility(ContentFrame.CanGoBack);

            // If previous page is a FileLinkPage or FolderLinkPage and the current page is of other type
            if ((this.ViewModel?.ContentViewModel is FileLinkViewModel && !(this.ContentFrame?.Content is FileLinkPage)) ||
                (this.ViewModel?.ContentViewModel is FolderLinkViewModel && !(this.ContentFrame?.Content is FolderLinkPage)))
            {
                // Remove the FolderLinkPage from the BackStack
                if (NavigateService.MainFrame.BackStack.Any())
                    NavigateService.MainFrame.BackStack.Remove(NavigateService.MainFrame.BackStack.Last());

                this.ViewModel.MenuItems = MenuItemViewModel.CreateMenuItems();
                this.ViewModel.OptionItems = MenuItemViewModel.CreateOptionItems();
            }

            // If current page is a FileLinkPage and the previous page is of other type
            if (!(this.ViewModel?.ContentViewModel is FileLinkViewModel) && this.ContentFrame?.Content is FileLinkPage)
            {
                this.ViewModel.MenuItems = MenuItemViewModel.CreateFileLinkMenuItems();
                this.ViewModel.OptionItems = MenuItemViewModel.CreatePublicLinkOptionItems();
            }

            // If current page is a FolderLinkPage and the previous page is of other type
            if (!(this.ViewModel?.ContentViewModel is FolderLinkViewModel) && this.ContentFrame?.Content is FolderLinkPage)
            {
                this.ViewModel.MenuItems = MenuItemViewModel.CreateFolderLinkMenuItems();
                this.ViewModel.OptionItems = MenuItemViewModel.CreatePublicLinkOptionItems();
            }
            
            // Set current content viewmodel as property of the main page
            // Could be handy in the future
            this.ViewModel.ContentViewModel = (this.ContentFrame.Content as Page)?.DataContext as BasePageViewModel;

            if (e.NavigationMode != NavigationMode.Back)
                this.ViewModel.NavigateOnMenuItemSelected = false;

            // Set current menu or option item 
            this.ViewModel.SelectedItem = this.ViewModel.MenuItems.FirstOrDefault(
                m => NavigateService.GetViewType(m.TargetViewModel) == ContentFrame.CurrentSourcePageType);
            this.ViewModel.SelectedOptionItem = this.ViewModel.OptionItems.FirstOrDefault(
                m => NavigateService.GetViewType(m.TargetViewModel) == ContentFrame.CurrentSourcePageType);

            this.ViewModel.NavigateOnMenuItemSelected = true;
        }

        private void OnHamburgerMenuControlItemClick(object sender, ItemClickEventArgs e)
        {
            // If in inline mode or compact inline mode, do not close the pane on item selection
            if (this.HamburgerMenuControl.DisplayMode == (SplitViewDisplayMode.Inline | SplitViewDisplayMode.CompactInline))
                return;

            HamburgerMenuControl.IsPaneOpen = false;
        }
    }
}
