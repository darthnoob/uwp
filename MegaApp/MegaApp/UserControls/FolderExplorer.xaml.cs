using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.ViewModels;
using MegaApp.ViewModels.SharedFolders;
using MegaApp.ViewModels.UserControls;

namespace MegaApp.UserControls
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseFolderExplorer : UserControlEx<FolderExplorerViewModel> { }

    public sealed partial class FolderExplorer : BaseFolderExplorer
    {
        public FolderExplorer()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the Folder.
        /// </summary>
        public FolderViewModel Folder
        {
            get { return (FolderViewModel)GetValue(FolderProperty); }
            set { SetValue(FolderProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="Folder" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty FolderProperty =
            DependencyProperty.Register(
                nameof(Folder),
                typeof(FolderViewModel),
                typeof(FolderExplorer),
                new PropertyMetadata(null, FolderChangedCallback));

        private static void FolderChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as FolderExplorer;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnFolderChanged((FolderViewModel)dpc.NewValue);
            }
        }

        private void OnFolderChanged(FolderViewModel folder)
        {
            if (this.ViewModel == null) return;

            if (this.ViewModel.Folder != null)
            {
                this.ViewModel.Folder.ChangeViewEvent -= OnViewChanged;
                this.ViewModel.Folder.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
                this.ViewModel.Folder.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
                this.ViewModel.Folder.ItemCollection.OnlyAllowSingleSelectStatusChanged -= OnOnlyAllowSingleSelectStatusChanged;
                this.ViewModel.Folder.ItemCollection.AllSelected -= OnAllSelected;
            }

            this.ViewModel.Folder = folder;

            if (this.ViewModel.Folder != null)
            {
                this.ViewModel.Folder.ChangeViewEvent += OnViewChanged;
                this.ViewModel.Folder.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
                this.ViewModel.Folder.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
                this.ViewModel.Folder.ItemCollection.OnlyAllowSingleSelectStatusChanged += OnOnlyAllowSingleSelectStatusChanged;
                this.ViewModel.Folder.ItemCollection.AllSelected += OnAllSelected;
            }
        }

        private void OnViewChanged(object sender, EventArgs e)
        {
            // First save the current selected nodes to restore them after change the view
            var selectedNodes = this.ViewModel.Folder.ItemCollection.SelectedItems.ToList();

            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // Clear the selected items and restore in the new view
            ClearSelectedItems();
            UpdateSelectedItems(selectedNodes);

            // Enable the view behaviors again
            EnableViewsBehaviors();
        }

        public void EnableSelection()
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;
            this.ListView.SelectionMode = ListViewSelectionMode.Extended;
            this.GridView.SelectionMode = ListViewSelectionMode.Extended;
        }

        public void DisableSelection()
        {
            this.ListView.SelectionMode = ListViewSelectionMode.None;
            this.GridView.SelectionMode = ListViewSelectionMode.None;
        }

        public void ClearSelectedItems()
        {
            if (this.ListView?.SelectedItems?.Count > 0)
                this.ListView.SelectedItems.Clear();

            if (this.GridView?.SelectedItems?.Count > 0)
                this.GridView.SelectedItems.Clear();
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // First save the current selected items to restore them after enable the multi select
            var selectedItems = this.ViewModel.Folder.ItemCollection.SelectedItems.ToList();

            this.ListView.SelectionMode = ListViewSelectionMode.Multiple;
            this.GridView.SelectionMode = ListViewSelectionMode.Multiple;

            // Restore the selected items
            UpdateSelectedItems(selectedItems);

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        private void OnMultiSelectDisabled(object sender, EventArgs e)
        {
            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // If there is only one selected item save it to restore it after disable the multi select mode
            IMegaNode selectedItem = null;
            if (this.ViewModel.Folder.ItemCollection.OnlyOneSelectedItem)
                selectedItem = this.ViewModel.Folder.ItemCollection.SelectedItems.First();

            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
            {
                this.ListView.SelectionMode = ListViewSelectionMode.Extended;
                this.GridView.SelectionMode = ListViewSelectionMode.Extended;
            }
            else
            {
                this.ListView.SelectionMode = ListViewSelectionMode.None;
                this.GridView.SelectionMode = ListViewSelectionMode.None;
            }

            // Restore the selected item
            this.ListView.SelectedItem = this.GridView.SelectedItem = 
                this.ViewModel.Folder.ItemCollection.FocusedItem = selectedItem;

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        private void OnOnlyAllowSingleSelectStatusChanged(object sender, bool isEnabled)
        {
            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // First save the current selected item to restore it after enable/disable the single select mode
            IMegaNode selectedItem = null;
            if (this.ViewModel.Folder.ItemCollection.OnlyOneSelectedItem)
                selectedItem = this.ViewModel.Folder.ItemCollection.SelectedItems.First();

            if (!isEnabled && DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
            {
                this.ListView.SelectionMode = ListViewSelectionMode.Extended;
                this.GridView.SelectionMode = ListViewSelectionMode.Extended;
            }
            else
            {
                this.ListView.SelectionMode = ListViewSelectionMode.Single;
                this.GridView.SelectionMode = ListViewSelectionMode.Single;
            }

            // Restore the selected item
            this.ListView.SelectedItem = this.GridView.SelectedItem = 
                this.ViewModel.Folder.ItemCollection.FocusedItem = selectedItem;

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        /// <summary>
        /// Enable the behaviors of the active view
        /// </summary>
        private void EnableViewsBehaviors()
        {
            Interaction.GetBehaviors(this.ListView).Attach(this.ListView);
            Interaction.GetBehaviors(this.GridView).Attach(this.GridView);
        }

        /// <summary>
        /// Disable the behaviors of the current active view
        /// </summary>
        private void DisableViewsBehaviors()
        {
            Interaction.GetBehaviors(this.ListView).Detach();
            Interaction.GetBehaviors(this.GridView).Detach();
        }

        /// <summary>
        /// Update the selected nodes of the active view
        /// </summary>
        /// <param name="selectedNodes">Listo of selected nodes</param>
        private void UpdateSelectedItems(List<IMegaNode> selectedNodes)
        {
            ListViewBase list = null;
            switch (this.Folder.ViewMode)
            {
                case FolderContentViewMode.ListView:
                    list = this.ListView;
                    break;
                case FolderContentViewMode.GridView:
                    list = this.GridView;
                    break;
                default:
                    return;
            }

            foreach (var node in selectedNodes)
            {
                if (list.SelectedItems.Contains(node)) continue;
                list.SelectedItems.Add(node);
            }
        }

        private void OnAllSelected(object sender, bool value)
        {
            if (value)
            {
                this.ListView?.SelectAll();
                this.GridView?.SelectAll();
            }
            else
            {
                this.ListView?.SelectedItems.Clear();
                this.GridView?.SelectedItems.Clear();
            }
        }

        private void OnItemTapped(object sender, TappedRoutedEventArgs e)
        {
            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
            {
                this.ViewModel.Folder.OnChildNodeTapped(itemTapped);
                return;
            }

            if (itemTapped is ImageNodeViewModel)
                (itemTapped as ImageNodeViewModel).InViewingRange = true;

            this.ViewModel.Folder.FocusedNode = itemTapped;
        }

        private void OnItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (this.ViewModel.Folder.IsPanelOpen || DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
                return;

            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            this.ViewModel.Folder.OnChildNodeTapped(itemTapped);
        }

        private void OnRightItemTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (this.ViewModel.Folder.IsPanelOpen || DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
                return;

            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            this.ViewModel.Folder.FocusedNode = itemTapped;

            if (!this.ViewModel.Folder.ItemCollection.IsMultiSelectActive &&
                this.ViewModel.Folder.CurrentViewState != FolderContentViewState.CopyOrMove)
            {
                ((ListViewBase)sender).SelectedItems.Clear();
                ((ListViewBase)sender).SelectedItems.Add(itemTapped);
            }
        }

        private void OnFolderOptionsButtonClicked(object sender, RoutedEventArgs e)
        {
            var flyoutButton = sender as Button;
            if (flyoutButton == null) return;

            MenuFlyout menuFlyout = new MenuFlyout();

            //menuFlyout.Items?.Add(new MenuFlyoutItem()
            //{
            //    Text = ResourceService.UiResources.GetString("UI_Information"),
            //    Command = this.ViewModel.InformationCommand
            //});

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_Download"),
                Command = this.ViewModel.DownloadFolderCommand
            });

            //menuFlyout.Items?.Add(new MenuFlyoutItem()
            //{
            //    Text = ResourceService.UiResources.GetString("UI_Copy"),
            //    Command = this.ViewModel.CopyFolderCommand
            //});

            if (this.ViewModel?.IsRenameFolderOptionAvailable == true)
            {
                menuFlyout.Items?.Add(new MenuFlyoutItem()
                {
                    Text = ResourceService.UiResources.GetString("UI_Rename"),
                    Command = this.ViewModel.RenameFolderCommand
                });
            }

            if (this.ViewModel?.Folder?.FolderRootNode is IncomingSharedFolderNodeViewModel)
            {
                menuFlyout.Items?.Add(new MenuFlyoutItem()
                {
                    Text = ResourceService.UiResources.GetString("UI_LeaveShare"),
                    Foreground = (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"],
                    Command = (this.ViewModel.Folder.FolderRootNode as IncomingSharedFolderNodeViewModel)?.LeaveShareCommand
                });
            }

            if (this.ViewModel?.Folder?.FolderRootNode is OutgoingSharedFolderNodeViewModel)
            {
                menuFlyout.Items?.Add(new MenuFlyoutItem()
                {
                    Text = ResourceService.UiResources.GetString("UI_RemoveSharedAccess"),
                    Foreground = (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"],
                    Command = (this.ViewModel.Folder.FolderRootNode as OutgoingSharedFolderNodeViewModel)?.RemoveSharedAccessCommand
                });
            }

            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(flyoutButton);
        }
    }
}
