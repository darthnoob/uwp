using System;
using System.ComponentModel;
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
        private const double ShareToPanelMinWidth = 432;

        public NodeInformationPanel()
        {
            this.InitializeComponent();

            this.ViewModel.OpenShareToPanelEvent += (sender, args) =>
                this.SplitView.IsPaneOpen = true;

            this.ShareToPanelControl.ViewModel.ClosePanelEvent += (sender, args) =>
                this.SplitView.IsPaneOpen = false;

            this.SplitView.PaneClosed += (sender, args) =>
                this.ShareToPanelControl.ViewModel.MegaContacts.ItemCollection.ClearSelection();

            this.SplitView.RegisterPropertyChangedCallback(
                SplitView.IsPaneOpenProperty, IsSplitViewOpenPropertyChanged);
        }

        private void IsSplitViewOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (this.SplitView.IsPaneOpen)
            {
                if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop || this.SplitView.ActualWidth < 600)
                {
                    this.SplitView.OpenPaneLength = this.SplitView.ActualWidth;
                    AppService.SetAppViewBackButtonVisibility(true);
                    return;
                }

                this.SplitView.OpenPaneLength = ShareToPanelMinWidth;
                AppService.SetAppViewBackButtonVisibility(true);
                return;
            }

            AppService.SetAppViewBackButtonVisibility(false);
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
            if (this.ViewModel == null) return;

            if (this.ViewModel.Node != null)
            {
                this.ViewModel.Node.PropertyChanged -= OnNodePropertyChanged;
                this.ViewModel.Node.Parent.ShareEvent -= OnSharingEvent;

                var folderNode = this.ViewModel.Node as FolderNodeViewModel;
                if (folderNode?.ContactsList?.ItemCollection != null)
                {
                    folderNode.ContactsList.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
                    folderNode.ContactsList.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
                }
            }

            if (node != null)
            {
                node.PropertyChanged += OnNodePropertyChanged;
                node.Parent.ShareEvent += OnSharingEvent;

                var folderNode = node as FolderNodeViewModel;
                if (folderNode?.ContactsList?.ItemCollection != null)
                {
                    folderNode.ContactsList.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
                    folderNode.ContactsList.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
                }
            }

            this.ViewModel.Node = node;

            this.LinkWithKeyRadioButton.IsChecked = true;

            if (this.PivotControl.Items != null)
            {
                // Node is an InShare or child of a Folder Link
                if (this.ViewModel.IsInShare || this.ViewModel.IsFolderLinkChild)
                {
                    this.PivotControl.Items.Remove(this.LinkPivot);
                    this.PivotControl.Items.Remove(this.SharePivot);
                    this.PivotControl.SelectedItem = this.DetailsPivot;
                }
                // Node is a Folder or OutShare
                else if (this.ViewModel.IsFolder)
                {
                    if(!this.PivotControl.Items.Contains(this.LinkPivot))
                        this.PivotControl.Items.Add(this.LinkPivot);
                    if (!this.PivotControl.Items.Contains(this.SharePivot))
                        this.PivotControl.Items.Add(this.SharePivot);
                }
                else // Node is a File
                {
                    var changeSelectedItem = this.PivotControl.SelectedItem?.Equals(this.SharePivot) ?? true;

                    this.PivotControl.Items.Remove(this.SharePivot);

                    if (!this.PivotControl.Items.Contains(this.LinkPivot))
                        this.PivotControl.Items.Add(this.LinkPivot);

                    if (changeSelectedItem)
                        this.PivotControl.SelectedItem = this.DetailsPivot;
                }
            }

            ChangeCommandBar();
        }

        private void OnSharingEvent(object sender, EventArgs e)
        {
            this.PivotControl.SelectedItem = this.SharePivot;
        }

        private void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.ChangeCommandBar();
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

            toggle.IsOn = this.ViewModel.FolderNode.IsOutShare;
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeCommandBar();

        private void ChangeCommandBar()
        {
            if (this.PivotControl?.SelectedItem?.Equals(this.SharePivot) == true)
            {
                this.MainTopCommandBar.Visibility = Visibility.Collapsed;
                this.MainBottomCommandBar.Visibility = Visibility.Collapsed;

                var sharePivotCommandBarVisibility = this.ViewModel.FolderNode == null ? Visibility.Collapsed :
                    this.ViewModel.FolderNode.IsOutShare ? Visibility.Visible : Visibility.Collapsed;
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

            // Needed to avoid strange behaviors during the view update
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

            // Needed to avoid strange behaviors during the view update
            DisableViewsBehaviors();

            // If there is only one selected item save it to restore it after disable the multi select mode
            IMegaContact selectedItem = null;
            if (folderNode.ContactsList.ItemCollection.OnlyOneSelectedItem)
                selectedItem = folderNode.ContactsList.ItemCollection.SelectedItems.First();

            this.ListViewContacts.SelectionMode = 
                DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop ?
                ListViewSelectionMode.Extended : ListViewSelectionMode.Single;

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
            if (this.ViewModel.Node is FolderNodeViewModel == false) return;

            var folderNode = (FolderNodeViewModel)this.ViewModel.Node;

            IMegaContact itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaContact;
            if (itemTapped == null) return;

            folderNode.ContactsList.ItemCollection.FocusedItem = itemTapped;

            var view = (ListViewBase)sender;
            if (view == null) return;

            if (folderNode.ContactsList.ItemCollection.IsMultiSelectActive)
                view.SelectedItems?.Add(itemTapped);
            else
                view.SelectedItem = itemTapped;
        }
    }
}
