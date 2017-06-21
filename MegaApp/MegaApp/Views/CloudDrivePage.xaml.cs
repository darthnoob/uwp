using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        public CloudDrivePage()
        {
            InitializeComponent();

            this.ViewModel.ClearSelectedItems += OnClearSelectedItems;
            this.ViewModel.DisableSelection += OnDisableSelection;
            this.ViewModel.EnableSelection += OnEnableSelection;            

            this.ViewModel.CloudDrive.ChangeViewEvent += OnChangeView;
            this.ViewModel.RubbishBin.ChangeViewEvent += OnChangeView;
            this.ViewModel.CameraUploads.ChangeViewEvent += OnChangeView;

            this.ViewModel.CloudDrive.EnableMultiSelect += OnEnableMultiSelect;
            this.ViewModel.RubbishBin.EnableMultiSelect += OnEnableMultiSelect;
            this.ViewModel.CameraUploads.EnableMultiSelect += OnEnableMultiSelect;

            this.ViewModel.CloudDrive.DisableMultiSelect += OnDisableMultiSelect;
            this.ViewModel.RubbishBin.DisableMultiSelect += OnDisableMultiSelect;
            this.ViewModel.CameraUploads.DisableMultiSelect += OnDisableMultiSelect;

            this.ViewModel.CloudDrive.OpenNodeDetailsEvent += OnOpenNodeDetails;
            this.ViewModel.RubbishBin.OpenNodeDetailsEvent += OnOpenNodeDetails;
            this.ViewModel.CameraUploads.OpenNodeDetailsEvent += OnOpenNodeDetails;

            this.ViewModel.CloudDrive.CloseNodeDetailsEvent += OnCloseNodeDetails;
            this.ViewModel.RubbishBin.CloseNodeDetailsEvent += OnCloseNodeDetails;
            this.ViewModel.CameraUploads.CloseNodeDetailsEvent += OnCloseNodeDetails;

            this.NodeDetailsSplitView.RegisterPropertyChangedCallback(
                SplitView.IsPaneOpenProperty, IsDetailsViewOpenPropertyChanged);
            
        }

        private void IsDetailsViewOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (this.NodeDetailsSplitView.IsPaneOpen)
            {
                if(DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop || this.NodeDetailsSplitView.ActualWidth < 600)
                {
                    this.NodeDetailsSplitView.OpenPaneLength = this.NodeDetailsSplitView.ActualWidth;
                    AppService.SetAppViewBackButtonVisibility(true);
                    return;
                }

                this.NodeDetailsSplitView.OpenPaneLength = this.NodeDetailsSplitView.MinWidth;
            }

            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
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
            this.NodeDetailsSplitView.IsPaneOpen = false;

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

            this.NodeDetailsSplitView.IsPaneOpen = false;

            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            if (((ListViewBase)sender)?.SelectedItems?.Contains(itemTapped) == true)
                ((ListViewBase)sender).SelectedItems.Remove(itemTapped);

            this.ViewModel.ActiveFolderView.OnChildNodeTapped(itemTapped);
        }

        private void OnRightItemTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;

            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            this.ViewModel.ActiveFolderView.FocusedNode = itemTapped;

            if (!this.ViewModel.ActiveFolderView.IsMultiSelectActive &&
                this.ViewModel.ActiveFolderView.CurrentViewState != FolderContentViewState.CopyOrMove)
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

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
            {
                ListViewCameraUploads.SelectAll();
                GridViewCameraUploads.SelectAll();
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

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
            {
                ListViewCameraUploads.SelectedItems.Clear();
                GridViewCameraUploads.SelectedItems.Clear();
            }
        }

        private void OnEnableMultiSelect(object sender, EventArgs e)
        {
            this.NodeDetailsSplitView.IsPaneOpen = false;

            // First save the current selected nodes to restore them after enable the multi select
            var tempSelectedNodes = this.ViewModel.ActiveFolderView.ItemCollection.SelectedItems.ToList();

            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

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

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
            {
                ListViewCameraUploads.SelectionMode = ListViewSelectionMode.Multiple;
                GridViewCameraUploads.SelectionMode = ListViewSelectionMode.Multiple;
            }

            // Restore the selected items and enable the view behaviors again
            UpdateSelectedItems(tempSelectedNodes);
            EnableViewsBehaviors();
        }

        private void OnDisableMultiSelect(object sender, EventArgs e)
        {
            OnEnableSelection(sender, e);
        }

        private void OnDisableSelection(object sender, EventArgs e)
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

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
            {
                ListViewCameraUploads.SelectionMode = ListViewSelectionMode.None;
                GridViewCameraUploads.SelectionMode = ListViewSelectionMode.None;
            }
        }

        private void OnEnableSelection(object sender, EventArgs e)
        {
            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
            {
                if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                {
                    ListViewCloudDrive.SelectionMode = ListViewSelectionMode.Extended;
                    GridViewCloudDrive.SelectionMode = ListViewSelectionMode.Extended;
                }

                if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                {
                    ListViewRubbishBin.SelectionMode = ListViewSelectionMode.Extended;
                    GridViewRubbishBin.SelectionMode = ListViewSelectionMode.Extended;
                }

                if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
                {
                    ListViewCameraUploads.SelectionMode = ListViewSelectionMode.Extended;
                    GridViewCameraUploads.SelectionMode = ListViewSelectionMode.Extended;
                }
            }
            else
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

                if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                {
                    ListViewCameraUploads.SelectionMode = ListViewSelectionMode.None;
                    GridViewCameraUploads.SelectionMode = ListViewSelectionMode.None;
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

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
            {
                if (ListViewCameraUploads?.SelectedItems?.Count > 0)
                    ListViewCameraUploads.SelectedItems.Clear();
                if (GridViewCameraUploads?.SelectedItems?.Count > 0)
                    GridViewCameraUploads.SelectedItems.Clear();
            }
        }

        private void OnChangeView(object sender, EventArgs e)
        {
            // First save the current selected nodes to restore them after change the view
            var tempSelectedNodes = this.ViewModel.ActiveFolderView.ItemCollection.SelectedItems.ToList();

            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // Clean the selected items and restore in the new view
            OnClearSelectedItems(sender, e);
            UpdateSelectedItems(tempSelectedNodes);

            // Enable the view behaviors again
            EnableViewsBehaviors();
        }

        private void OnOpenNodeDetails(object sender, EventArgs e)
        {
            this.NodeDetailsSplitView.IsPaneOpen = true;
        }

        private void OnCloseNodeDetails(object sender, EventArgs e)
        {
            this.NodeDetailsSplitView.IsPaneOpen = false;
        }

        /// <summary>
        /// Enable the behaviors of the active views
        /// </summary>
        private void EnableViewsBehaviors()
        {
            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
            {
                Interaction.GetBehaviors(ListViewCloudDrive).Attach(ListViewCloudDrive);
                Interaction.GetBehaviors(GridViewCloudDrive).Attach(GridViewCloudDrive);
            }

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
            {
                Interaction.GetBehaviors(ListViewRubbishBin).Attach(ListViewRubbishBin);
                Interaction.GetBehaviors(GridViewRubbishBin).Attach(GridViewRubbishBin);
            }

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
            {
                Interaction.GetBehaviors(ListViewCameraUploads).Attach(ListViewCameraUploads);
                Interaction.GetBehaviors(GridViewCameraUploads).Attach(GridViewCameraUploads);
            }
        }

        /// <summary>
        /// Disable the behaviors of the current active views
        /// </summary>
        private void DisableViewsBehaviors()
        {
            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
            {
                Interaction.GetBehaviors(ListViewCloudDrive).Detach();
                Interaction.GetBehaviors(GridViewCloudDrive).Detach();
            }

            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
            {
                Interaction.GetBehaviors(ListViewRubbishBin).Detach();
                Interaction.GetBehaviors(GridViewRubbishBin).Detach();
            }

            if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
            {
                Interaction.GetBehaviors(ListViewCameraUploads).Detach();
                Interaction.GetBehaviors(GridViewCameraUploads).Detach();
            }
        }

        /// <summary>
        /// Update the selected nodes of the active view
        /// </summary>
        /// <param name="selectedNodes">Listo of selected nodes</param>
        private void UpdateSelectedItems(List<IMegaNode> selectedNodes)
        {
            foreach (var node in selectedNodes)
            {
                if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                {
                    switch (this.ViewModel.CloudDrive.ViewMode)
                    {
                        case FolderContentViewMode.ListView:
                            ListViewCloudDrive.SelectedItems.Add(node);
                            break;
                        case FolderContentViewMode.GridView:
                            GridViewCloudDrive.SelectedItems.Add(node);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
                {
                    switch (this.ViewModel.RubbishBin.ViewMode)
                    {
                        case FolderContentViewMode.ListView:
                            ListViewRubbishBin.SelectedItems.Add(node);
                            break;
                        case FolderContentViewMode.GridView:
                            GridViewRubbishBin.SelectedItems.Add(node);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (MainPivot.SelectedItem.Equals(CameraUploadsPivot))
                {
                    switch (this.ViewModel.CameraUploads.ViewMode)
                    {
                        case FolderContentViewMode.ListView:
                            ListViewCameraUploads.SelectedItems.Add(node);
                            break;
                        case FolderContentViewMode.GridView:
                            GridViewCameraUploads.SelectedItems.Add(node);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private void OnSortClick(object sender, RoutedEventArgs e)
        {
            var sortButton = sender as Button;

            var buttonPosition = sortButton.TransformToVisual(BtnSort);
            Point screenCoords = buttonPosition.TransformPoint(new Point(sortButton.ActualWidth, sortButton.ActualHeight));

            MenuFlyout menuFlyout = DialogService.CreateSortMenu(ViewModel.ActiveFolderView);            
            menuFlyout.ShowAt(sortButton, screenCoords);
        }
    }
}
