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

            this.ViewModel.ClearSelectedItems += OnClearSelectedItems;

            this.ViewModel.CloudDrive.EnableMultiSelect += OnEnableMultiSelect;
            this.ViewModel.RubbishBin.EnableMultiSelect += OnEnableMultiSelect;

            this.ViewModel.CloudDrive.DisableMultiSelect += OnDisableMultiSelect;
            this.ViewModel.RubbishBin.DisableMultiSelect += OnDisableMultiSelect;
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
                    if (!App.AppInformation.HasFetchedNodes)
                        this.ViewModel.FetchNodes();
                    else
                        this.ViewModel.LoadFolders();
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
            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            if (this.ViewModel.ActiveFolderView.IsMultiSelectActive)
            {
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
                return;
            }

            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
                this.ViewModel.ActiveFolderView.OnChildNodeTapped(itemTapped);
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

        private void OnItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;

            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            if (((ListViewBase)sender)?.SelectedItems?.Contains(itemTapped) == true)
                ((ListViewBase)sender).SelectedItems.Remove(itemTapped);

            this.ViewModel.ActiveFolderView.OnChildNodeTapped(itemTapped);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();
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
        }

        private void OnEnableMultiSelect(object sender, EventArgs e)
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
            {
                if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                {
                    ListViewCloudDrive.SelectionMode = ListViewSelectionMode.Multiple;
                    GridViewCloudDrive.SelectionMode = ListViewSelectionMode.Multiple;
                }

                if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                {
                    ListViewRubbishBin.SelectionMode = ListViewSelectionMode.Multiple;
                    GridViewRubbishBin.SelectionMode = ListViewSelectionMode.Multiple;
                }
            }
        }

        private void OnDisableMultiSelect(object sender, EventArgs e)
        {
            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
                OnClearSelectedItems(sender, e);

            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
            {
                if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                {
                    ListViewCloudDrive.SelectionMode = ListViewSelectionMode.None;
                    GridViewCloudDrive.SelectionMode = ListViewSelectionMode.None;
                }

                if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                {
                    ListViewRubbishBin.SelectionMode = ListViewSelectionMode.None;
                    GridViewRubbishBin.SelectionMode = ListViewSelectionMode.None;
                }
            }
        }

        private void OnClearSelectedItems(object sender, EventArgs e)
        {
            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
            {
                if (ListViewCloudDrive?.SelectedItems?.Count > 0)
                    ListViewCloudDrive.SelectedItems.Clear();
                if (GridViewCloudDrive?.SelectedItems?.Count > 0)
                    GridViewCloudDrive.SelectedItems.Clear();
            }

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
            {
                if (ListViewRubbishBin?.SelectedItems?.Count > 0)
                    ListViewRubbishBin.SelectedItems.Clear();
                if (GridViewRubbishBin?.SelectedItems?.Count > 0)
                    GridViewRubbishBin.SelectedItems.Clear();
            }
        }

        private void OnSortClick(object sender, RoutedEventArgs e)
        {
            var sortButton = sender as Button;

            var buttonPosition = sortButton.TransformToVisual(BtnSort);
            Point screenCoords = buttonPosition.TransformPoint(new Point(32, 32));

            MenuFlyout menuFlyout = DialogService.CreateSortMenu(ViewModel.ActiveFolderView);            
            menuFlyout.ShowAt(sortButton, screenCoords);
        }
    }
}
