using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;
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
            
            this.ViewModel.OutgoingShares.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ViewModel.OutgoingShares.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
            this.ViewModel.OutgoingShares.ItemCollection.OnlyAllowSingleSelectStatusChanged += OnOnlyAllowSingleSelectStatusChanged;
            this.ViewModel.OutgoingShares.ItemCollection.AllSelected += OnAllSelected;

            this.SharedFolderInformationSplitView.RegisterPropertyChangedCallback(
                SplitView.IsPaneOpenProperty, IsInformationViewOpenPropertyChanged);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.IncomingShares.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
            this.ViewModel.IncomingShares.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
            this.ViewModel.IncomingShares.ItemCollection.OnlyAllowSingleSelectStatusChanged -= OnOnlyAllowSingleSelectStatusChanged;
            this.ViewModel.IncomingShares.ItemCollection.AllSelected -= OnAllSelected;

            this.ViewModel.OutgoingShares.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
            this.ViewModel.OutgoingShares.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
            this.ViewModel.OutgoingShares.ItemCollection.OnlyAllowSingleSelectStatusChanged -= OnOnlyAllowSingleSelectStatusChanged;
            this.ViewModel.OutgoingShares.ItemCollection.AllSelected -= OnAllSelected;

            this.ViewModel.Deinitialize();
            base.OnNavigatedFrom(e);
        }

        private void IsInformationViewOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (this.SharedFolderInformationSplitView.IsPaneOpen)
            {
                if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop || this.SharedFolderInformationSplitView.ActualWidth < 600)
                {
                    this.SharedFolderInformationSplitView.OpenPaneLength = this.SharedFolderInformationSplitView.ActualWidth;
                    AppService.SetAppViewBackButtonVisibility(true);
                    return;
                }

                this.SharedFolderInformationSplitView.OpenPaneLength = InformationPanelMinWidth;
            }

            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
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
            if (this.ViewModel?.ActiveView != null && this.ViewModel.ActiveView.IsPanelOpen)
                this.ViewModel.ActiveView.IsPanelOpen = false;
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            // Needed to avoid extrange behaviors during the view update
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
            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // If there is only one selected item save it to restore it after disable the multi select mode
            IMegaSharedFolderNode selectedItem = null;
            if (this.ViewModel.ActiveView.ItemCollection.OnlyOneSelectedItem)
                selectedItem = this.ViewModel.ActiveView.ItemCollection.SelectedItems.First();

            var listView = this.GetSelectedListView();

            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
                listView.SelectionMode = ListViewSelectionMode.Extended;
            else
                listView.SelectionMode = ListViewSelectionMode.Single;

            // Restore the selected item
            listView.SelectedItem = this.ViewModel.ActiveView.ItemCollection.FocusedItem = selectedItem;

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        private void OnOnlyAllowSingleSelectStatusChanged(object sender, bool isEnabled)
        {
            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // First save the current selected item to restore it after enable/disable the single select mode
            IMegaSharedFolderNode selectedItem = null;
            if (this.ViewModel.ActiveView.ItemCollection.OnlyOneSelectedItem)
                selectedItem = this.ViewModel.ActiveView.ItemCollection.SelectedItems.First();

            var listView = this.GetSelectedListView();
            if(isEnabled)
            {
                listView.SelectionMode = ListViewSelectionMode.Single;
            }
            else
            {
                if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
                    listView.SelectionMode = ListViewSelectionMode.Extended;
                else
                    listView.SelectionMode = ListViewSelectionMode.Single;
            }

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

            if (value)
                listView?.SelectAll();
            else
                listView?.SelectedItems.Clear();
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
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;

            IMegaSharedFolderNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaSharedFolderNode;
            if (itemTapped == null) return;

            this.ViewModel.ActiveView.ItemCollection.FocusedItem = itemTapped;

            if (!this.ViewModel.ActiveView.ItemCollection.IsMultiSelectActive)
                ((ListViewBase)sender).SelectedItems?.Clear();

            ((ListViewBase)sender).SelectedItems?.Add(itemTapped);
        }
    }
}
