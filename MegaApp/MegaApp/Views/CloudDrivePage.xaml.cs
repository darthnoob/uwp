using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseCloudDrivePage : PageEx<CloudDriveViewModel> { }

    public sealed partial class CloudDrivePage : BaseCloudDrivePage
    {
        public CloudDrivePage()
        {
            InitializeComponent();
        }

        public override bool CanGoBack
        {
            get
            {
                bool canGoFolderUp = false;
                if (this.ViewModel?.ActiveFolderView != null)
                    canGoFolderUp = this.ViewModel.ActiveFolderView.CanGoFolderUp();
                return canGoFolderUp || MainPivot.SelectedIndex != 0;
            }
        }

        public override void GoBack()
        {
            if (this.ViewModel.ActiveFolderView.CanGoFolderUp())
            {
                this.ViewModel.ActiveFolderView.GoFolderUp();
            }
            else
            {
                if (MainPivot.SelectedIndex > 0) MainPivot.SelectedIndex--;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.CloudDrive.FolderNavigatedTo -= OnFolderNavigatedTo;
            this.ViewModel.RubbishBin.FolderNavigatedTo -= OnFolderNavigatedTo;

            this.ViewModel.Deinitialize(App.GlobalListener);

            base.OnNavigatedFrom(e);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ViewModel.Initialize(App.GlobalListener);

            this.ViewModel.CloudDrive.FolderNavigatedTo += OnFolderNavigatedTo;
            this.ViewModel.RubbishBin.FolderNavigatedTo += OnFolderNavigatedTo;

            NavigationActionType navActionType = NavigateService.GetNavigationObject(e.Parameter).Action;

            // Need to check it always and no only in StartupMode, 
            // because this is the first page loaded
            if (!await AppService.CheckActiveAndOnlineSession(e.NavigationMode)) return;

            if (!NetworkService.IsNetworkAvailable())
            {
                //UpdateGUI(false);
                return;
            }

            switch(navActionType)
            {
                case NavigationActionType.Login:
                    this.ViewModel.FetchNodes();
                    break;
                
                default:
                    Load();
                    break;
            }

            await AppService.CheckSpecialNavigation();
        }

        private void OnFolderNavigatedTo(object sender, EventArgs eventArgs)
        {
            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        /// <summary>
        /// Method to load properly the content of the Cloud Drive and the Rubbish Bin
        /// </summary>
        private async void Load()
        {
            // If user has an active and online session but is not logged in, resume the session
            if (await AppService.CheckActiveAndOnlineSession() && !Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn()))
                SdkService.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(
                    ResourceService.SettingsResources.GetString("SR_UserMegaSession")),
                    new FastLoginRequestListener(this.ViewModel));
            // If the user's nodes haven't been retrieved, do it
            else if (!App.AppInformation.HasFetchedNodes)
                this.ViewModel.FetchNodes();
            // In other case load them
            else
                this.ViewModel.LoadFolders();
        }
       
        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
            {
                AddFolderButton.Visibility = UploadButton.Visibility = Visibility.Visible;                
                CleanRubbishBinButton.Visibility = Visibility.Collapsed;

                this.ViewModel.ActiveFolderView = this.ViewModel.CloudDrive;
            }

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
            {
                AddFolderButton.Visibility = UploadButton.Visibility = Visibility.Collapsed;
                CleanRubbishBinButton.Visibility = Visibility.Visible;

                this.ViewModel.ActiveFolderView = this.ViewModel.RubbishBin;
            }                

            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        private void OnItemTapped(object sender, TappedRoutedEventArgs e)
        {
            if(DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
                ProcessItemTapped(sender);
        }

        private void OnItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ProcessItemTapped(sender);
        }

        private void ProcessItemTapped(object sender)
        {
            IMegaNode itemTapped = null;
            if (sender is ListView)
                itemTapped = ((ListView)sender).SelectedItem as IMegaNode;
            if (sender is GridView)
                itemTapped = ((GridView)sender).SelectedItem as IMegaNode;

            if (itemTapped != null)
            {
                if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                    ListViewCloudDrive.SelectedItem = GridViewCloudDrive.SelectedItem = null;
                if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                    ListViewRubbishBin.SelectedItem = GridViewRubbishBin.SelectedItem = null;

                this.ViewModel.ActiveFolderView.OnChildNodeTapped(itemTapped);
            }
        }        

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();
        }

        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                e.Handled = !OpenFlyoutMenu(sender, (FrameworkElement)e.OriginalSource,
                    e.GetPosition(GetViewControlInUse(sender)));
            }
            catch (Exception) { }
        }

        private void OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                e.Handled = !OpenFlyoutMenu(sender, (FrameworkElement)e.OriginalSource,
                    e.GetPosition(GetViewControlInUse(sender)));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Converts the view control object in use to ListView or GridView
        /// </summary>
        /// <param name="obj">The view control object in use.</param>
        /// <returns>The view control in use as ListView or GridView</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        private UIElement GetViewControlInUse(object obj)
        {
            if (obj is ListView)
                return obj as ListView;
            if (obj is GridView)
                return obj as GridView;

            throw new ArgumentOutOfRangeException("Invalid view control");
        }

        /// <summary>
        /// Opens the flyout menu (contextual menu).
        /// </summary>
        /// <param name="sender">The view control object in use.</param>
        /// <param name="item">Item of the view control in use.</param>
        /// <param name="position">Screen position of the item.</param>
        /// <returns>Boolean value indicating if all went well.</returns>
        private bool OpenFlyoutMenu(object sender, FrameworkElement item, Point position)
        {
            SdkService.MegaSdk.retryPendingConnections();

            try
            {
                if (ViewModel?.ActiveFolderView != null)
                {
                    // If the user is moving nodes, don't show the contextual menu
                    if (ViewModel.ActiveFolderView.CurrentDisplayMode == DriveDisplayMode.CopyOrMoveItem)
                        return true;

                    // We don't want to open the menu if the focused element is not a list view item.
                    // If the list view is empty listViewItem will be null.
                    if (!(item?.DataContext is IMegaNode))
                        return true;

                    MenuFlyout menuFlyout = null;
                    if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                        menuFlyout = this.Resources["CloudDriveMenuFlyout"] as MenuFlyout;
                    if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                        menuFlyout = this.Resources["RubbishBinMenuFlyout"] as MenuFlyout;

                    menuFlyout.ShowAt(GetViewControlInUse(sender), position);
                    
                    ViewModel.ActiveFolderView.FocusedNode = item.DataContext as IMegaNode;
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch (Exception) { return false; }
        }
    }
}
