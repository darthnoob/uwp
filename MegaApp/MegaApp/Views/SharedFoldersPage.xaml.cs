using Microsoft.Xaml.Interactivity;
using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseSharedFoldersPage : PageEx<SharedFoldersViewModel> { }

    public sealed partial class SharedFoldersPage : BaseSharedFoldersPage
    {
        private const double InformationPanelMinWidth = 432;
        private const double CopyOrMovePanelMinWidth = 432;
        private const double ContentPanelMaxWidth = 888;
        private const double ContentPanelMinWidth = 432;

        public SharedFoldersPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Initialize();

            this.ViewModel.IncomingShares.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ViewModel.IncomingShares.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
            this.ViewModel.IncomingShares.ItemCollection.OnlyAllowSingleSelectStatusChanged += OnOnlyAllowSingleSelectStatusChanged;
            this.ViewModel.IncomingShares.ItemCollection.AllSelected += OnAllSelected;
            this.ViewModel.IncomingShares.SelectedNodesActionStarted += OnSelectedNodesActionStarted;
            this.ViewModel.IncomingShares.SelectedNodesActionCanceled += OnSelectedNodesActionCanceled;

            this.ViewModel.OutgoingShares.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ViewModel.OutgoingShares.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
            this.ViewModel.OutgoingShares.ItemCollection.OnlyAllowSingleSelectStatusChanged += OnOnlyAllowSingleSelectStatusChanged;
            this.ViewModel.OutgoingShares.ItemCollection.AllSelected += OnAllSelected;
            this.ViewModel.OutgoingShares.SelectedNodesActionStarted += OnSelectedNodesActionStarted;
            this.ViewModel.OutgoingShares.SelectedNodesActionCanceled += OnSelectedNodesActionCanceled;

            this.CopyOrMovePanelControl.ViewModel.ActionFinished += OnSelectedNodesActionFinished;
            this.CopyOrMovePanelControl.ViewModel.ActionCanceled += OnSelectedNodesActionCanceled;

            this.SharedFolderSplitView.RegisterPropertyChangedCallback(
                SplitView.IsPaneOpenProperty, IsSplitViewOpenPropertyChanged);

            Window.Current.SizeChanged += OnWindowSizeChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.IncomingShares.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
            this.ViewModel.IncomingShares.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
            this.ViewModel.IncomingShares.ItemCollection.OnlyAllowSingleSelectStatusChanged -= OnOnlyAllowSingleSelectStatusChanged;
            this.ViewModel.IncomingShares.ItemCollection.AllSelected -= OnAllSelected;
            this.ViewModel.IncomingShares.SelectedNodesActionStarted -= OnSelectedNodesActionStarted;
            this.ViewModel.IncomingShares.SelectedNodesActionCanceled -= OnSelectedNodesActionCanceled;

            this.ViewModel.OutgoingShares.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
            this.ViewModel.OutgoingShares.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
            this.ViewModel.OutgoingShares.ItemCollection.OnlyAllowSingleSelectStatusChanged -= OnOnlyAllowSingleSelectStatusChanged;
            this.ViewModel.OutgoingShares.ItemCollection.AllSelected -= OnAllSelected;
            this.ViewModel.OutgoingShares.SelectedNodesActionStarted -= OnSelectedNodesActionStarted;
            this.ViewModel.OutgoingShares.SelectedNodesActionCanceled -= OnSelectedNodesActionCanceled;

            this.CopyOrMovePanelControl.ViewModel.ActionFinished -= OnSelectedNodesActionFinished;
            this.CopyOrMovePanelControl.ViewModel.ActionCanceled -= OnSelectedNodesActionCanceled;

            Window.Current.SizeChanged -= OnWindowSizeChanged;

            this.ViewModel.Deinitialize();
            base.OnNavigatedFrom(e);
        }

        private void IsSplitViewOpenPropertyChanged(DependencyObject sender, DependencyProperty dp) => this.SetPanelWidth();
        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e) => this.SetPanelWidth();

        private void SetPanelWidth()
        {
            if (this.ViewModel.ActiveView.IsPanelOpen)
            {
                if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop || this.SharedFolderSplitView.ActualWidth < 600)
                {
                    this.SharedFolderSplitView.OpenPaneLength = this.SharedFolderSplitView.ActualWidth;
                    AppService.SetAppViewBackButtonVisibility(true);
                    return;
                }

                switch (this.ViewModel.ActiveView.VisiblePanel)
                {
                    case PanelType.Information:
                        this.SharedFolderSplitView.OpenPaneLength = InformationPanelMinWidth;
                        break;

                    case PanelType.Content:
                        this.SharedFolderSplitView.OpenPaneLength = this.SharedFolderSplitView.ActualWidth < 1200 ?
                            ContentPanelMinWidth : ContentPanelMaxWidth;
                        break;

                    case PanelType.CopyMoveImport:
                        this.SharedFolderSplitView.OpenPaneLength = CopyOrMovePanelMinWidth;
                        break;
                }
            }
        }

        public override bool CanGoBack
        {
            get
            {
                if (this.ViewModel?.ActiveView != null && this.ViewModel.ActiveView.IsPanelOpen)
                    return true;

                return base.CanGoBack;
            }
        }

        public override void GoBack()
        {
            this.ViewModel?.ActiveView?.ClosePanels();
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            // Needed to avoid strange behaviors during the view update
            DisableViewsBehaviors();

            // First save the current selected items to restore them after enable the multi select
            var selectedItems = this.ViewModel.ActiveView.ItemCollection.SelectedItems.ToList();

            var listView = this.GetSelectedListView();
            listView.SelectionMode = ListViewSelectionMode.Multiple;

            // Update the selected items
            foreach (var item in selectedItems)
                listView.SelectedItems.Add(item);

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        private void OnMultiSelectDisabled(object sender, EventArgs e)
        {
            // Needed to avoid strange behaviors during the view update
            DisableViewsBehaviors();

            // If there is only one selected item save it to restore it after disable the multi select mode
            IBaseNode selectedItem = null;
            if (this.ViewModel.ActiveView.ItemCollection.OnlyOneSelectedItem)
                selectedItem = this.ViewModel.ActiveView.ItemCollection.SelectedItems.First();

            var listView = this.GetSelectedListView();
            listView.SelectionMode = 
                DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop ?
                ListViewSelectionMode.Extended : ListViewSelectionMode.Single;

            // Restore the selected item
            listView.SelectedItem = this.ViewModel.ActiveView.ItemCollection.FocusedItem = selectedItem;

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        private void OnOnlyAllowSingleSelectStatusChanged(object sender, bool isEnabled)
        {
            // Needed to avoid strange behaviors during the view update
            DisableViewsBehaviors();

            // First save the current selected item to restore it after enable/disable the single select mode
            IBaseNode selectedItem = null;
            if (this.ViewModel.ActiveView.ItemCollection.OnlyOneSelectedItem)
                selectedItem = this.ViewModel.ActiveView.ItemCollection.SelectedItems.First();

            var listView = this.GetSelectedListView();
            listView.SelectionMode =
                (!isEnabled && DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop) ?
                ListViewSelectionMode.Extended : ListViewSelectionMode.Single;

            // Restore the selected item
            listView.SelectedItem = this.ViewModel.ActiveView.ItemCollection.FocusedItem = selectedItem;

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        /// <summary>
        /// Enable the behaviors of the active view
        /// </summary>
        private void EnableViewsBehaviors()
        {
            var listView = this.GetSelectedListView();
            Interaction.GetBehaviors(listView).Attach(listView);
        }

        /// <summary>
        /// Disable the behaviors of the current active view
        /// </summary>
        private void DisableViewsBehaviors()
        {
            var listView = this.GetSelectedListView();
            Interaction.GetBehaviors(listView).Detach();
        }

        private void OnAllSelected(object sender, bool value)
        {
            var listView = this.GetSelectedListView();

            if (!value)
            {
                listView?.SelectedItems.Clear();
                return;
            }

            if (listView?.SelectionMode == ListViewSelectionMode.Extended ||
                listView?.SelectionMode == ListViewSelectionMode.Multiple)
            {
                listView?.SelectAll();
            }
        }

        private ListView GetSelectedListView()
        {
            if (this.SharedFoldersPivot.SelectedItem.Equals(this.IncomingSharesPivot))
                return this.ListViewIncomingShares;
            if (this.SharedFoldersPivot.SelectedItem.Equals(this.OutgoingSharesPivot))
                return this.ListViewOutgoingShares;
            return null;
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SharedFoldersPivot.SelectedItem.Equals(this.IncomingSharesPivot))
                this.ViewModel.ActiveView = this.ViewModel.IncomingShares;

            if (this.SharedFoldersPivot.SelectedItem.Equals(this.OutgoingSharesPivot))
                this.ViewModel.ActiveView = this.ViewModel.OutgoingShares;
        }

        private void OnSortClick(object sender, RoutedEventArgs e)
        {
            var sortButton = sender as Button;
            if (sortButton == null) return;

            MenuFlyout menuFlyout = null;
            if (this.SharedFoldersPivot.SelectedItem.Equals(this.IncomingSharesPivot))
                menuFlyout = DialogService.CreateIncomingSharedItemsSortMenu(this.ViewModel.IncomingShares);
            if (this.SharedFoldersPivot.SelectedItem.Equals(this.OutgoingSharesPivot))
                menuFlyout = DialogService.CreateOutgoingSharedItemsSortMenu(this.ViewModel.OutgoingShares);

            if (menuFlyout == null) return;
            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(sortButton);
        }

        private void OnItemTapped(object sender, TappedRoutedEventArgs e)
        {
            IMegaSharedFolderNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaSharedFolderNode;
            if (itemTapped == null) return;

            this.ViewModel.ActiveView.ItemCollection.FocusedItem = itemTapped;
        }

        private void OnItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

        }

        private void OnRightItemTapped(object sender, RightTappedRoutedEventArgs e)
        {
            IMegaSharedFolderNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaSharedFolderNode;
            if (itemTapped == null) return;

            this.ViewModel.ActiveView.ItemCollection.FocusedItem = itemTapped;

            var view = (ListViewBase)sender;
            if (view == null) return;

            if (this.ViewModel.ActiveView.ItemCollection.IsMultiSelectActive)
                view.SelectedItems?.Add(itemTapped);
            else
                view.SelectedItem = itemTapped;
        }

        private void OnSelectedNodesActionStarted(object sender, EventArgs e)
        {
            this.DisableSelection();
        }

        private void OnSelectedNodesActionFinished(object sender, EventArgs e)
        {
            ResetCopyOrMove();
        }

        private void OnSelectedNodesActionCanceled(object sender, EventArgs e)
        {
            ResetCopyOrMove();
        }

        private void ResetCopyOrMove()
        {
            this.ViewModel.ActiveView.ResetSelectedNodes();
            this.CopyOrMovePanelControl.Reset();
            this.ClearSelectedItems();
            this.EnableSelection();
        }

        private void EnableSelection()
        {
            if (SharedFoldersPivot.SelectedItem.Equals(IncomingSharesPivot))
            {
                this.ListViewIncomingShares.SelectionMode = 
                    DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop ?
                    ListViewSelectionMode.Extended : ListViewSelectionMode.Single;
            }

            if (SharedFoldersPivot.SelectedItem.Equals(OutgoingSharesPivot))
            {
                this.ListViewOutgoingShares.SelectionMode =
                    DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop ?
                    ListViewSelectionMode.Extended : ListViewSelectionMode.Single;
            }
        }

        private void DisableSelection()
        {
            if (SharedFoldersPivot.SelectedItem.Equals(IncomingSharesPivot))
            {
                this.ListViewIncomingShares.SelectionMode = ListViewSelectionMode.None;
                this.ListViewIncomingShares.IsRightTapEnabled = false;
            }

            if (SharedFoldersPivot.SelectedItem.Equals(OutgoingSharesPivot))
            {
                this.ListViewOutgoingShares.SelectionMode = ListViewSelectionMode.None;
                this.ListViewOutgoingShares.IsRightTapEnabled = false;
            }
        }

        private void ClearSelectedItems()
        {
            if (SharedFoldersPivot.SelectedItem.Equals(IncomingSharesPivot) && 
                this.ListViewIncomingShares?.SelectedItems?.Count > 0)
                this.ListViewIncomingShares.SelectedItems.Clear();

            if (SharedFoldersPivot.SelectedItem.Equals(OutgoingSharesPivot) &&
                this.ListViewOutgoingShares?.SelectedItems?.Count > 0)
                this.ListViewOutgoingShares.SelectedItems.Clear();
        }
    }
}
