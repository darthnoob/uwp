using System;
using System.Linq;
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
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
            {
                if (this.ViewModel.ActiveFolderView.IsMultiSelectActive) return;
                IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
                if (itemTapped == null) return;
                this.ViewModel.ActiveFolderView.OnChildNodeTapped(itemTapped);
            }
        }

        private void OnItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;

            ((ListViewBase)sender).SelectedItems.Clear();
            
            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;
            this.ViewModel.ActiveFolderView.OnChildNodeTapped(itemTapped);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();
        }

        private void OnCopyOrMoveClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            if ((ViewModel?.ActiveFolderView?.SelectedNodes?.Count == 0) && 
                (ViewModel?.ActiveFolderView?.FocusedNode != null))
            {
                ViewModel.ActiveFolderView.SelectedNodes.Add(ViewModel.ActiveFolderView.FocusedNode);
            }

            if (ViewModel?.ActiveFolderView?.SelectedNodes?.Count > 0)
            {
                foreach(var node in ViewModel.ActiveFolderView.SelectedNodes)
                    if(node != null) node.DisplayMode = NodeDisplayMode.SelectedForCopyOrMove;

                ViewModel.CloudDrive.PreviousViewState = ViewModel.CloudDrive.CurrentViewState;
                ViewModel.CloudDrive.CurrentViewState = FolderContentViewState.CopyOrMove;
                ViewModel.RubbishBin.PreviousViewState = ViewModel.RubbishBin.CurrentViewState;
                ViewModel.RubbishBin.CurrentViewState = FolderContentViewState.CopyOrMove;

                ViewModel.SourceFolderView = ViewModel.ActiveFolderView;

                //ChangeSelectionMode(false);
                ViewModel.SourceFolderView.IsMultiSelectActive = false;                
            }
        }

        private void OnCancelCopyOrMoveClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            if (ViewModel?.SourceFolderView != null)
            {
                // Release the focused node
                if (ViewModel?.SourceFolderView?.FocusedNode != null)
                {
                    ViewModel.SourceFolderView.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
                    ViewModel.SourceFolderView.FocusedNode = null;
                }

                // Clear and release the selected nodes list
                if (ViewModel?.SourceFolderView?.SelectedNodes?.Count > 0)
                {
                    foreach (var node in ViewModel.SourceFolderView.SelectedNodes)
                        if(node != null) node.DisplayMode = NodeDisplayMode.Normal;

                    ViewModel.SourceFolderView.SelectedNodes.Clear();
                }

                ViewModel.SourceFolderView = null;
            }

            ResetViewStates();
        }

        private void OnAcceptCopyClick(object sender, RoutedEventArgs e)
        {
            ViewModel.AcceptCopyAction(this.ViewModel.SourceFolderView.SelectedNodes.ToList());
            ViewModel.SourceFolderView.SelectedNodes.Clear();
            ResetViewStates();
        }

        private void OnAcceptMoveClick(object sender, RoutedEventArgs e)
        {
            ViewModel.AcceptMoveAction(this.ViewModel.SourceFolderView.SelectedNodes.ToList());
            ViewModel.SourceFolderView.SelectedNodes.Clear();
            ResetViewStates();
        }

        private void OnMultiSelectButtonClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            //ChangeSelectionMode(true);
            ViewModel.ActiveFolderView.IsMultiSelectActive = true;
        }

        private void OnCancelMultiSelectButtonClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            //ChangeSelectionMode(false);
            ViewModel.ActiveFolderView.IsMultiSelectActive = false;

            ViewModel.ActiveFolderView.SelectedNodes.Clear();
        }

        private void ResetViewStates()
        {
            ViewModel.CloudDrive.CurrentViewState = FolderContentViewState.CloudDrive;
            ViewModel.CloudDrive.PreviousViewState = FolderContentViewState.CloudDrive;

            ViewModel.RubbishBin.CurrentViewState = FolderContentViewState.RubbishBin;
            ViewModel.RubbishBin.PreviousViewState = FolderContentViewState.RubbishBin;
        }

        //private void ChangeSelectionMode(bool isMultiSelect)
        //{
        //    var selectionMode = isMultiSelect ? 
        //        ListViewSelectionMode.Multiple : ListViewSelectionMode.Extended;

        //    if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
        //    {
        //        ListViewCloudDrive.SelectionMode = selectionMode;
        //        GridViewCloudDrive.SelectionMode = selectionMode;
        //    }

        //    if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
        //    {
        //        ListViewRubbishBin.SelectionMode = selectionMode;
        //        GridViewRubbishBin.SelectionMode = selectionMode;
        //    }
        //}

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

        private void OnCancelMultiSelectTapped(object sender, TappedRoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
            {
                ListViewCloudDrive.SelectedItems.Clear();
                GridViewCloudDrive.SelectedItems.Clear();
                if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
                {
                    ListViewCloudDrive.SelectionMode = ListViewSelectionMode.None;
                    GridViewCloudDrive.SelectionMode = ListViewSelectionMode.None;
                }
            }

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
            {
                ListViewRubbishBin.SelectedItems.Clear();
                GridViewRubbishBin.SelectedItems.Clear();
                if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
                {
                    ListViewRubbishBin.SelectionMode = ListViewSelectionMode.None;
                    GridViewRubbishBin.SelectionMode = ListViewSelectionMode.None;
                }
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
