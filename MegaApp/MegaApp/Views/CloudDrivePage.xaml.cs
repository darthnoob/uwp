using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
                this.ViewModel.ActiveFolderView = this.ViewModel.CloudDrive;

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                this.ViewModel.ActiveFolderView = this.ViewModel.RubbishBin;

            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        private void OnItemTapped(object sender, TappedRoutedEventArgs e)
        {
            if(DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
                ProcessItemTapped(sender, (FrameworkElement)e.OriginalSource);
        }

        private void OnItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ProcessItemTapped(sender, (FrameworkElement)e.OriginalSource);
        }

        /// <summary>
        /// Processes the tapped item.
        /// </summary>
        /// <param name="sender">View control which contains the tapped item.</param>
        /// <param name="item">Tapped item.</param>
        private void ProcessItemTapped(object sender, FrameworkElement item)
        {
            IMegaNode itemTapped = item?.DataContext as IMegaNode;

            if (itemTapped != null)
            {
                if(this.ViewModel.ActiveFolderView.IsMultiSelectActive)
                {
                    // Add or delete the item tapped to or from the selected nodes list
                    if (this.ViewModel.ActiveFolderView.SelectedNodes.Contains(itemTapped))
                        this.ViewModel.ActiveFolderView.SelectedNodes.Remove(itemTapped);
                    else
                        this.ViewModel.ActiveFolderView.SelectedNodes.Add(itemTapped);

                    // Manage the selected items of the view control which is not now in use, 
                    // because the view control in use y automatically managed.
                    switch (this.ViewModel.ActiveFolderView.ViewMode)
                    {
                        case FolderContentViewMode.ListView:
                            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                                ManageMultiSelectItems(GridViewCloudDrive, itemTapped);
                            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                                ManageMultiSelectItems(GridViewRubbishBin, itemTapped);
                            break;

                        case FolderContentViewMode.GridView:
                            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                                ManageMultiSelectItems(ListViewCloudDrive, itemTapped);
                            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                                ManageMultiSelectItems(ListViewRubbishBin, itemTapped);
                            break;
                    }
                }
                else
                {                    
                    this.ViewModel.ActiveFolderView.OnChildNodeTapped(itemTapped);
                }
            }
        }

        /// <summary>
        /// Adds or removes the item to the selected items collection of the view control.
        /// </summary>
        /// <param name="viewObject">View control where add or remove the item.</param>
        /// <param name="item">Item to add or remove.</param>
        private void ManageMultiSelectItems(ListViewBase viewControl, IMegaNode item)
        {
            if (viewControl.SelectedItems.Contains(item))
                viewControl.SelectedItems.Remove(item);
            else
                viewControl.SelectedItems.Add(item);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();
        }

        private void OnMultiSelectButtonClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
            {
                ListViewCloudDrive.SelectionMode = GridViewCloudDrive.SelectionMode = 
                    ListViewSelectionMode.Multiple;
            }
                
            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
            {
                ListViewRubbishBin.SelectionMode = GridViewRubbishBin.SelectionMode = 
                    ListViewSelectionMode.Multiple;
            }
        }

        private void OnCancelMultiSelectButtonClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
            {
                ListViewCloudDrive.SelectionMode = GridViewCloudDrive.SelectionMode =
                    ListViewSelectionMode.Extended;
            }

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
            {
                ListViewRubbishBin.SelectionMode = GridViewRubbishBin.SelectionMode =
                    ListViewSelectionMode.Extended;
            }

            this.ViewModel.ActiveFolderView.SelectedNodes.Clear();
        }

        private void OnSelectAllClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
            {
                ListViewCloudDrive.SelectAll();
                GridViewCloudDrive.SelectAll();
            }

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
            {
                ListViewRubbishBin.SelectAll();
                GridViewRubbishBin.SelectAll();
            }

            this.ViewModel.ActiveFolderView.SelectedNodes.AddRange(this.ViewModel.ActiveFolderView.ChildNodes);
        }

        private void OnDeselectAllClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
            {
                ListViewCloudDrive.SelectedItems.Clear();
                GridViewCloudDrive.SelectedItems.Clear();
            }

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
            {
                ListViewRubbishBin.SelectedItems.Clear();
                GridViewRubbishBin.SelectedItems.Clear();
            }

            this.ViewModel.ActiveFolderView.SelectedNodes.Clear();
        }

        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                e.Handled = !OpenFlyoutMenu(sender, (FrameworkElement)e.OriginalSource,
                    e.GetPosition(sender as ListViewBase));
            }
            catch (Exception) { }
        }

        private void OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                e.Handled = !OpenFlyoutMenu(sender, (FrameworkElement)e.OriginalSource,
                    e.GetPosition(sender as ListViewBase));
            }
            catch (Exception) { }
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
                if (this.ViewModel?.ActiveFolderView != null)
                {
                    // If the user is moving nodes, don't show the contextual menu
                    if (this.ViewModel.ActiveFolderView.CurrentViewState == FolderContentViewState.CopyOrMoveItem)
                        return true;

                    // We don't want to open the menu if the focused element is not a list view item.
                    // If the list view is empty listViewItem will be null.
                    if (!(item?.DataContext is IMegaNode))
                        return true;

                    MenuFlyout menuFlyout = null;

                    if ((sender as ListViewBase).SelectedItems.Count > 1)
                    {
                        if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                            menuFlyout = this.Resources["MultiselectCloudDriveMenuFlyout"] as MenuFlyout;
                        else if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                            menuFlyout = this.Resources["MultiselectRubbishBinMenuFlyout"] as MenuFlyout;
                    }
                    else
                    {
                        if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                            menuFlyout = this.Resources["CloudDriveMenuFlyout"] as MenuFlyout;
                        else if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                            menuFlyout = this.Resources["RubbishBinMenuFlyout"] as MenuFlyout;
                    }

                    menuFlyout.ShowAt(sender as ListViewBase, position);

                    this.ViewModel.ActiveFolderView.FocusedNode = item.DataContext as IMegaNode;

                    this.ViewModel.ActiveFolderView.SelectedNodes.Clear();
                    foreach (var selectedItem in (sender as ListViewBase).SelectedItems)
                        this.ViewModel.ActiveFolderView.SelectedNodes.Add(selectedItem as IMegaNode);
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
