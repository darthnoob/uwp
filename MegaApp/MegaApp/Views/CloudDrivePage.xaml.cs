using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
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
        private const double InformationPanelMinWidth = 432;
        private const double CopyOrMovePanelMinWidth = 432;

        public CloudDrivePage()
        {
            InitializeComponent();

            this.ViewModel.CameraUploads.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ViewModel.CameraUploads.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
            this.ViewModel.CameraUploads.ItemCollection.AllSelected += OnAllSelected;

            this.ViewModel.CloudDrive.CopyOrMoveEvent += OnCopyOrMove;
            this.ViewModel.CameraUploads.CopyOrMoveEvent += OnCopyOrMove;
            this.ViewModel.RubbishBin.CopyOrMoveEvent += OnCopyOrMove;

            this.ViewModel.CloudDrive.CancelCopyOrMoveEvent += OnResetCopyOrMove;
            this.ViewModel.CameraUploads.CancelCopyOrMoveEvent += OnResetCopyOrMove;
            this.ViewModel.RubbishBin.CancelCopyOrMoveEvent += OnResetCopyOrMove;

            this.CopyOrMovePanelControl.ViewModel.CopyOrMoveFinished += OnResetCopyOrMove;
            this.CopyOrMovePanelControl.ViewModel.CopyOrMoveCanceled += OnResetCopyOrMove;

            this.CloudDriveSplitView.RegisterPropertyChangedCallback(
                SplitView.IsPaneOpenProperty, IsSplitViewOpenPropertyChanged);
        }

        private void IsSplitViewOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (this.ViewModel.ActiveFolderView.IsPanelOpen)
            {
                if(DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop || this.CloudDriveSplitView.ActualWidth < 600)
                {
                    this.CloudDriveSplitView.OpenPaneLength = this.CloudDriveSplitView.ActualWidth;
                    AppService.SetAppViewBackButtonVisibility(true);
                    return;
                }

                switch (this.ViewModel.ActiveFolderView.VisiblePanel)
                {
                    case PanelType.Information:
                        this.CloudDriveSplitView.OpenPaneLength = InformationPanelMinWidth;
                        break;
                    
                    case PanelType.CopyOrMove:
                        this.CloudDriveSplitView.OpenPaneLength = CopyOrMovePanelMinWidth;
                        break;
                }
            }

            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        public override bool CanGoBack
        {
            get
            {
                bool canGoBack = false;
                if (this.ViewModel?.ActiveFolderView != null)
                {
                    canGoBack = this.ViewModel.ActiveFolderView.IsPanelOpen ||
                        this.ViewModel.ActiveFolderView.CanGoFolderUp();
                }
                                
                return canGoBack || MainPivot.SelectedIndex != 0;
            }
        }

        public override void GoBack()
        {
            if(CloudDriveSplitView.IsPaneOpen)
            {
                this.ViewModel.ActiveFolderView.ClosePanels();
            }
            else if (this.ViewModel.ActiveFolderView.CanGoFolderUp())
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ViewModel.Initialize(App.GlobalListener);

            this.ViewModel.CloudDrive.FolderNavigatedTo += OnFolderNavigatedTo;
            this.ViewModel.RubbishBin.FolderNavigatedTo += OnFolderNavigatedTo;

            var navObj = NavigateService.GetNavigationObject(e.Parameter) as NavigationObject;
            var navActionType = navObj?.Action ?? NavigationActionType.Default;
            if (navActionType == NavigationActionType.RubbishBin)
                this.MainPivot.SelectedItem = this.RubbishBinPivot;

            this.ViewModel.LoadFolders();
        }

        private void OnFolderNavigatedTo(object sender, EventArgs eventArgs)
        {
            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                this.ViewModel.ActiveFolderView = this.ViewModel.CloudDrive;

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                this.ViewModel.ActiveFolderView = this.ViewModel.RubbishBin;

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
                this.ViewModel.ActiveFolderView = this.ViewModel.CameraUploads;

            if (!this.ViewModel.ActiveFolderView.IsLoaded)
                this.ViewModel.LoadFolders();

            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        private void OnItemTapped(object sender, TappedRoutedEventArgs e)
        {
            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
            {
                this.ViewModel.ActiveFolderView.OnChildNodeTapped(itemTapped);
                return;
            }

            if ((itemTapped is ImageNodeViewModel) && (itemTapped as ImageNodeViewModel != null))
                (itemTapped as ImageNodeViewModel).InViewingRange = true;

            this.ViewModel.ActiveFolderView.FocusedNode = itemTapped;
        }

        private void OnItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;

            this.CloudDriveSplitView.IsPaneOpen = false;

            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            this.ViewModel.ActiveFolderView.OnChildNodeTapped(itemTapped);
        }

        private void OnRightItemTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;

            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            this.ViewModel.ActiveFolderView.FocusedNode = itemTapped;

            if (!this.ViewModel.ActiveFolderView.ItemCollection.IsMultiSelectActive)
            {
                ((ListViewBase)sender).SelectedItems.Clear();
                ((ListViewBase)sender).SelectedItems.Add(itemTapped);
            }
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

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
                GridViewCameraUploads.SelectAll();
        }

        private void OnDeselectAllClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            if (!MainPivot.SelectedItem.Equals(CameraUploadsPivot)) return;

            GridViewCameraUploads.SelectedItems.Clear();
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            this.CloudDriveSplitView.IsPaneOpen = false;

            // First save the current selected nodes to restore them after enable the multi select
            var tempSelectedNodes = this.ViewModel.ActiveFolderView.ItemCollection.SelectedItems.ToList();

            // Needed to avoid strange behaviors during the view update
            DisableViewsBehaviors();

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
                GridViewCameraUploads.SelectionMode = ListViewSelectionMode.Multiple;

            // Restore the selected items and enable the view behaviors again
            UpdateSelectedItems(tempSelectedNodes);
            EnableViewsBehaviors();
        }

        private void OnMultiSelectDisabled(object sender, EventArgs e)
        {
            EnableSelection();
        }

        private void DisableSelection()
        {
            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                this.CloudDriveExplorer.DisableSelection();

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                this.RubbishBinExplorer.DisableSelection();

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
                this.GridViewCameraUploads.SelectionMode = ListViewSelectionMode.None;
        }

        private void EnableSelection()
        {
            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                this.CloudDriveExplorer.EnableSelection();

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                this.RubbishBinExplorer.EnableSelection();

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
            {
                this.GridViewCameraUploads.SelectionMode = 
                    DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop ?
                    ListViewSelectionMode.Extended : ListViewSelectionMode.None;
            }
        }

        private void ClearSelectedItems()
        {
            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                this.CloudDriveExplorer.ClearSelectedItems();

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                this.RubbishBinExplorer.ClearSelectedItems();

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
            {
                if (this.GridViewCameraUploads?.SelectedItems?.Count > 0)
                    this.GridViewCameraUploads.SelectedItems.Clear();
            }
        }

        private void OnCopyOrMove(object sender, EventArgs e)
        {
            this.DisableSelection();
        }

        private void OnResetCopyOrMove(object sender, EventArgs e)
        {
            this.ViewModel.ActiveFolderView.ResetCopyOrMove();
            this.ClearSelectedItems();
            this.EnableSelection();
        }

        /// <summary>
        /// Enable the behaviors of the active views
        /// </summary>
        private void EnableViewsBehaviors()
        {
            if (!MainPivot.SelectedItem.Equals(CameraUploadsPivot)) return;
            Interaction.GetBehaviors(GridViewCameraUploads).Attach(GridViewCameraUploads);
        }

        /// <summary>
        /// Disable the behaviors of the current active views
        /// </summary>
        private void DisableViewsBehaviors()
        {
            if (!MainPivot.SelectedItem.Equals(CameraUploadsPivot)) return;
            Interaction.GetBehaviors(GridViewCameraUploads).Detach();
        }

        /// <summary>
        /// Update the selected nodes of the active view
        /// </summary>
        /// <param name="selectedNodes">Listo of selected nodes</param>
        private void UpdateSelectedItems(List<IMegaNode> selectedNodes)
        {
            if (!MainPivot.SelectedItem.Equals(CameraUploadsPivot)) return;

            foreach (var node in selectedNodes)
                GridViewCameraUploads.SelectedItems.Add(node);
        }

        private void OnAllSelected(object sender, bool value)
        {
            if (!MainPivot.SelectedItem.Equals(CameraUploadsPivot)) return;

            if (value)
                this.GridViewCameraUploads?.SelectAll();
            else
                this.GridViewCameraUploads?.SelectedItems.Clear();
        }

        private void OnSortClick(object sender, RoutedEventArgs e)
        {
            // Camera uploads view does not allow change the sort type
            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot)) return;

            var sortButton = sender as Button;
            if (sortButton == null) return;

            MenuFlyout menuFlyout = DialogService.CreateSortMenu(ViewModel.ActiveFolderView);

            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(sortButton);
        }
    }
}
