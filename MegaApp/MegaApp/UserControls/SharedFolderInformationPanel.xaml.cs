using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels.SharedFolders;
using MegaApp.ViewModels.UserControls;

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
    }
}
