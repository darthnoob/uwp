using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;
using MegaApp.ViewModels.SharedFolders;
using MegaApp.ViewModels.UserControls;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.UserControls
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseSharedFolderInformationPanel : UserControlEx<SharedFolderInformationPanelViewModel> { }

    public sealed partial class SharedFolderInformationPanel : BaseSharedFolderInformationPanel
    {
        public SharedFolderInformationPanel()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the Shared Folder.
        /// </summary>
        public SharedFolderNodeViewModel SharedFolder
        {
            get { return (SharedFolderNodeViewModel)GetValue(SharedFolderProperty); }
            set { SetValue(SharedFolderProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="SharedFolder" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SharedFolderProperty =
            DependencyProperty.Register(
                nameof(SharedFolder),
                typeof(SharedFolderNodeViewModel),
                typeof(SharedFolderInformationPanel),
                new PropertyMetadata(null, SharedFolderChangedCallback));

        private static void SharedFolderChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as SharedFolderInformationPanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnSharedFolderChanged((SharedFolderNodeViewModel)dpc.NewValue);
            }
        }

        private void OnSharedFolderChanged(SharedFolderNodeViewModel sharedFolder)
        {
            if (this.ViewModel?.SharedFolder?.ContactsList?.ItemCollection != null)
            {
                this.ViewModel.SharedFolder.ContactsList.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
                this.ViewModel.SharedFolder.ContactsList.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
            }

            if (sharedFolder?.ContactsList?.ItemCollection != null)
            {
                sharedFolder.ContactsList.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
                sharedFolder.ContactsList.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
            }

            this.ViewModel.SharedFolder = sharedFolder;
            this.LinkWithKeyRadioButton.IsChecked = true;

            if (this.ViewModel.IsInShare)
            {
                this.PivotControl.Items.Remove(this.LinkPivot);
                this.PivotControl.Items.Remove(this.SharePivot);
                this.PivotControl.SelectedItem = this.DetailsPivot;
                return;
            }

            if (!this.PivotControl.Items.Contains(this.LinkPivot))
                this.PivotControl.Items.Add(this.LinkPivot);
            if (!this.PivotControl.Items.Contains(this.SharePivot))
                this.PivotControl.Items.Add(this.SharePivot);
        }

        private void OnEnableLinkSwitchToggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            if (toggle != null)
                this.ViewModel.EnableLink(toggle.IsOn);
        }

        private void OnSetExpirationDateSwitchToggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            if (toggle != null)
            {
                this.ExpirationDateCalendarDatePicker.IsEnabled = toggle.IsOn;
                if (toggle.IsOn)
                    this.ExpirationDateCalendarDatePicker.Date = this.SharedFolder.LinkExpirationDate;
                else
                    this.ExpirationDateCalendarDatePicker.Date = null;
            }
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
                if (this.SharedFolder.LinkExpirationTime > 0)
                    this.SharedFolder.SetLinkExpirationTime(0);
            }
            else if (this.SharedFolder.LinkExpirationDate == null ||
                !this.SharedFolder.LinkExpirationDate.Value.ToUniversalTime().Equals(this.ExpirationDateCalendarDatePicker.Date.Value.ToUniversalTime()))
            {
                this.SharedFolder.SetLinkExpirationTime(this.ExpirationDateCalendarDatePicker.Date.Value.ToUniversalTime().ToUnixTimeSeconds());
            }
        }

        private void OnEnableSharedFolderSwitchToggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            if (toggle != null)
                this.ViewModel.EnableSharedFolder(toggle.IsOn);
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.PivotControl.SelectedItem.Equals(this.SharePivot))
            {
                this.MainTopCommandBar.Visibility = Visibility.Collapsed;
                this.MainBottomCommandBar.Visibility = Visibility.Collapsed;

                this.SharePivotTopCommandBar.Visibility = Visibility.Visible;
                this.SharePivotBottomCommandBar.Visibility = Visibility.Visible;
            }
            else
            {
                this.MainTopCommandBar.Visibility = Visibility.Visible;
                this.MainBottomCommandBar.Visibility = Visibility.Visible;

                this.SharePivotTopCommandBar.Visibility = Visibility.Collapsed;
                this.SharePivotBottomCommandBar.Visibility = Visibility.Collapsed;
            }
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // First save the current selected items to restore them after enable the multi select
            var selectedItems = this.ViewModel.SharedFolder.ContactsList.ItemCollection.SelectedItems.ToList();

            this.ListViewContacts.SelectionMode = ListViewSelectionMode.Multiple;

            // Update the selected items
            foreach (var item in selectedItems)
                this.ListViewContacts.SelectedItems.Add(item);

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        private void OnMultiSelectDisabled(object sender, EventArgs e)
        {
            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // If there is only one selected item save it to restore it after disable the multi select mode
            IMegaContact selectedItem = null;
            if (this.ViewModel.SharedFolder.ContactsList.ItemCollection.OnlyOneSelectedItem)
                selectedItem = this.ViewModel.SharedFolder.ContactsList.ItemCollection.SelectedItems.First();

            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
                this.ListViewContacts.SelectionMode = ListViewSelectionMode.Extended;
            else
                this.ListViewContacts.SelectionMode = ListViewSelectionMode.Single;

            // Restore the selected item
            this.ListViewContacts.SelectedItem = this.ViewModel.SharedFolder.ContactsList.ItemCollection.FocusedItem = selectedItem;

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
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;

            IMegaContact itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaContact;
            if (itemTapped == null) return;

            this.ViewModel.SharedFolder.ContactsList.ItemCollection.FocusedItem = itemTapped;

            if (!this.ViewModel.SharedFolder.ContactsList.ItemCollection.IsMultiSelectActive)
                ((ListViewBase)sender).SelectedItems?.Clear();

            ((ListViewBase)sender).SelectedItems?.Add(itemTapped);
        }
    }
}
