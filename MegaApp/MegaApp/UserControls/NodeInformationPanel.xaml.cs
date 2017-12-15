using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.ViewModels;
using MegaApp.ViewModels.UserControls;

namespace MegaApp.UserControls
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseNodeInformationPanel : UserControlEx<NodeInformationPanelViewModel> { }

    public sealed partial class NodeInformationPanel : BaseNodeInformationPanel
    {
        public NodeInformationPanel()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the Node.
        /// </summary>
        public NodeViewModel Node
        {
            get { return (NodeViewModel)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="Node" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty NodeProperty =
            DependencyProperty.Register(
                nameof(Node),
                typeof(NodeViewModel),
                typeof(NodeInformationPanel),
                new PropertyMetadata(null, NodeChangedCallback));

        private static void NodeChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as NodeInformationPanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnNodeChanged((NodeViewModel)dpc.NewValue);
            }
        }

        private void OnNodeChanged(NodeViewModel node)
        {
            if (this.ViewModel?.Node is FolderNodeViewModel)
            {
                var folderNode = this.ViewModel.Node as FolderNodeViewModel;
                if (folderNode?.ContactsList?.ItemCollection != null)
                {
                    folderNode.ContactsList.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
                    folderNode.ContactsList.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
                }
            }

            if (node is FolderNodeViewModel)
            {
                var folderNode = node as FolderNodeViewModel;
                if (folderNode?.ContactsList?.ItemCollection != null)
                {
                    folderNode.ContactsList.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
                    folderNode.ContactsList.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
                }
            }

            if (this.ViewModel != null)
                this.ViewModel.Node = node;

            this.LinkWithKeyRadioButton.IsChecked = true;

            if (this.ViewModel?.IsInShare == true)
            {
                this.PivotControl.Items.Remove(this.LinkPivot);
                this.PivotControl.Items.Remove(this.SharePivot);
                this.PivotControl.SelectedItem = this.DetailsPivot;
                return;
            }

            if (this.ViewModel?.IsFolder == false)
            {
                var changeSelectedItem = this.PivotControl.SelectedItem != null ?
                    this.PivotControl.SelectedItem.Equals(this.SharePivot) : true;

                this.PivotControl.Items.Remove(this.SharePivot);

                if (!this.PivotControl.Items.Contains(this.LinkPivot))
                    this.PivotControl.Items.Add(this.LinkPivot);
                
                if (changeSelectedItem == true)
                    this.PivotControl.SelectedItem = this.DetailsPivot;
                return;
            }

            if (this.PivotControl.Items != null)
            {
                if (!this.PivotControl.Items.Contains(this.LinkPivot))
                    this.PivotControl.Items.Add(this.LinkPivot);
                if (!this.PivotControl.Items.Contains(this.SharePivot))
                    this.PivotControl.Items.Add(this.SharePivot);
            }

            ChangeCommandBar();
        }

        private void OnEnableLinkSwitchToggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            if (toggle == null) return;

            this.ViewModel.EnableLink(toggle.IsOn);
        }

        private void OnSetExpirationDateSwitchToggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            if (toggle == null) return;

            this.ExpirationDateCalendarDatePicker.IsEnabled = toggle.IsOn;
            this.ExpirationDateCalendarDatePicker.Date = toggle.IsOn ?
                this.Node.LinkExpirationDate : null;
        }

        private void OnExpirationDateCalendarDataPickerOpened(object sender, object e)
        {
            this.ExpirationDateCalendarDatePicker.LightDismissOverlayMode = LightDismissOverlayMode.On;
            this.ExpirationDateCalendarDatePicker.MinDate = DateTime.Today.AddDays(1);
        }

        private void OnExpirationDateCalendarDataPickerDateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            this.ExpirationDateCalendarDatePicker.IsCalendarOpen = false;

            if (this.ExpirationDateCalendarDatePicker.Date == null)
            {
                this.EnableLinkExpirationDateSwitch.IsOn = false;
                if (this.Node.LinkExpirationTime > 0)
                    this.Node.SetLinkExpirationTime(0);
            }
            else if (this.Node.LinkExpirationDate == null ||
                !this.Node.LinkExpirationDate.Value.ToUniversalTime().Equals(this.ExpirationDateCalendarDatePicker.Date.Value.ToUniversalTime()))
            {
                this.Node.SetLinkExpirationTime(this.ExpirationDateCalendarDatePicker.Date.Value.ToUniversalTime().ToUnixTimeSeconds());
            }
        }

        private void OnEnableSharedFolderSwitchToggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            if (toggle == null) return;

            this.ViewModel.EnableSharedFolder(toggle.IsOn);

            if (!toggle.IsOn)
                toggle.IsOn = this.ViewModel.FolderNode.IsOutShare;
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeCommandBar();

        private void ChangeCommandBar()
        {
            if (this.PivotControl?.SelectedItem?.Equals(this.SharePivot) == true)
            {
                this.MainTopCommandBar.Visibility = Visibility.Collapsed;
                this.MainBottomCommandBar.Visibility = Visibility.Collapsed;

                var sharePivotCommandBarVisibility = this.ViewModel.FolderNode.IsOutShare ?
                    Visibility.Visible : Visibility.Collapsed;
                this.SharePivotTopCommandBar.Visibility = sharePivotCommandBarVisibility;
                this.SharePivotBottomCommandBar.Visibility = sharePivotCommandBarVisibility;

                return;
            }

            this.MainTopCommandBar.Visibility = Visibility.Visible;
            this.MainBottomCommandBar.Visibility = Visibility.Visible;

            this.SharePivotTopCommandBar.Visibility = Visibility.Collapsed;
            this.SharePivotBottomCommandBar.Visibility = Visibility.Collapsed;
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            if (this.ViewModel.Node is FolderNodeViewModel == false) return;

            var folderNode = this.ViewModel.Node as FolderNodeViewModel;

            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // First save the current selected items to restore them after enable the multi select
            var selectedItems = folderNode.ContactsList.ItemCollection.SelectedItems.ToList();

            this.ListViewContacts.SelectionMode = ListViewSelectionMode.Multiple;

            // Update the selected items
            foreach (var item in selectedItems)
                this.ListViewContacts.SelectedItems.Add(item);

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        private void OnMultiSelectDisabled(object sender, EventArgs e)
        {
            if (this.ViewModel.Node is FolderNodeViewModel == false) return;

            var folderNode = this.ViewModel.Node as FolderNodeViewModel;

            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // If there is only one selected item save it to restore it after disable the multi select mode
            IMegaContact selectedItem = null;
            if (folderNode.ContactsList.ItemCollection.OnlyOneSelectedItem)
                selectedItem = folderNode.ContactsList.ItemCollection.SelectedItems.First();

            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
                this.ListViewContacts.SelectionMode = ListViewSelectionMode.Extended;
            else
                this.ListViewContacts.SelectionMode = ListViewSelectionMode.Single;

            // Restore the selected item
            this.ListViewContacts.SelectedItem = folderNode.ContactsList.ItemCollection.FocusedItem = selectedItem;

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        /// <summary>
        /// Enable the behaviors of the active view
        /// </summary>
        private void EnableViewsBehaviors()
        {
            Interaction.GetBehaviors(this.ListViewContacts).Attach(this.ListViewContacts);
        }

        /// <summary>
        /// Disable the behaviors of the current active view
        /// </summary>
        private void DisableViewsBehaviors()
        {
            Interaction.GetBehaviors(this.ListViewContacts).Detach();
        }

        private void OnRightItemTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop ||
                this.ViewModel.Node is FolderNodeViewModel == false) return;

            var folderNode = this.ViewModel.Node as FolderNodeViewModel;

            IMegaContact itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaContact;
            if (itemTapped == null) return;

            folderNode.ContactsList.ItemCollection.FocusedItem = itemTapped;

            if (!folderNode.ContactsList.ItemCollection.IsMultiSelectActive)
                ((ListViewBase)sender).SelectedItems?.Clear();

            ((ListViewBase)sender).SelectedItems?.Add(itemTapped);
        }
    }
}
