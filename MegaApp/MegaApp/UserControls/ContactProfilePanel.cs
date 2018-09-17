using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using mega;
using Microsoft.Xaml.Interactivity;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.ViewModels.Contacts;

namespace MegaApp.UserControls
{
    public class ContactProfilePanel : ContentControl
    {
        /// <summary>
        /// Gets or sets the Contact
        /// </summary>
        /// <value>The header text</value>
        public ContactViewModel Contact
        {
            get { return (ContactViewModel)GetValue(ContactProperty); }
            set { SetValue(ContactProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="Contact" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContactProperty =
            DependencyProperty.Register(
                nameof(Contact),
                typeof(ContactViewModel),
                typeof(ContactProfilePanel),
                new PropertyMetadata(null, ContactChangedCallback));

        private static void ContactChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as ContactProfilePanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnContactChanged((ContactViewModel)dpc.OldValue,
                    (ContactViewModel)dpc.NewValue);
            }
        }

        #region Events

        public event EventHandler CloseEvent;

        #endregion

        #region Controls

        private AppBarButton _sortSharedItemsButton;
        private Button _removeContact;
        private ListView _sharedItems;
        private Pivot _pivotControl;
        private PivotItem _sharedItemsPivot;
        private Grid _sharedItemsTopCommandBarArea;

        #endregion

        public ContactProfilePanel()
        {
            this.DefaultStyleKey = typeof(ContactProfilePanel);
            this.Opacity = 1;
            this.Visibility = Visibility.Visible;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._sortSharedItemsButton = (AppBarButton)this.GetTemplateChild("PART_BtnSortSharedItems");
            this._removeContact = (Button)this.GetTemplateChild("PART_RemoveContactButton");
            this._sharedItems = (ListView)this.GetTemplateChild("PART_ListViewSharedItems");
            this._pivotControl = (Pivot)this.GetTemplateChild("PART_PivotControl");
            this._sharedItemsPivot = (PivotItem)this.GetTemplateChild("PART_SharedItemsPivot");
            this._sharedItemsTopCommandBarArea = (Grid)this.GetTemplateChild("PART_SharedItemsTopCommandBarArea");

            if (this._sortSharedItemsButton != null)
                this._sortSharedItemsButton.Tapped += OnSortSharedItemsButtonTapped;

            if (this._removeContact != null)
                this._removeContact.Tapped += (sender, args) => OnRemoveContactTapped();

            if (this._sharedItems != null)
            {
                this._sharedItems.Tapped += OnSharedItemTapped;
                this._sharedItems.RightTapped += OnSharedItemRightTapped;
            }

            if (this._pivotControl != null)
                this._pivotControl.SelectionChanged += OnPivotSelectionChanged;
        }

        protected void OnContactChanged(ContactViewModel oldContact, ContactViewModel newContact)
        {
            if(oldContact?.SharedItems?.ItemCollection != null)
            {
                oldContact.SharedItems.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
                oldContact.SharedItems.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
                oldContact.SharedItems.ItemCollection.AllSelected -= OnAllSelected;
            }

            if (newContact == null)
            {
                this.CloseEvent?.Invoke(this, EventArgs.Empty);
                return;
            }

            // If achievements is enabled, retrieve achievement data for the new contact
            if (AccountService.AccountAchievements.IsAchievementsEnabled)
            {
                // Only need data from the MAchievementClass.MEGA_ACHIEVEMENT_INVITE type
                var referralAward = AccountService.AccountAchievements.AwardedClasses.FirstOrDefault(
                    model => model.AchievementClass.HasValue &&
                             model.AchievementClass.Value == MAchievementClass.MEGA_ACHIEVEMENT_INVITE);

                // Find the achievements invite for current contact
                var referralContact = (ContactViewModel)referralAward?.Contacts.ItemCollection.Items.FirstOrDefault(
                    contact => contact.Handle == newContact.Handle);

                if (referralContact != null)
                {
                    // Copy achievement information to the new contact
                    newContact.CopyAchievementDetails(referralContact);
                }
            }

            if (newContact?.SharedItems?.ItemCollection != null)
            {
                newContact.SharedItems.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
                newContact.SharedItems.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
                newContact.SharedItems.ItemCollection.AllSelected += OnAllSelected;
            }
        }

        private async void OnRemoveContactTapped()
        {
            await this.Contact.RemoveContactAsync();
        }

        private void OnSortSharedItemsButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            var sortButton = sender as Button;
            if (sortButton == null) return;

            MenuFlyout menuFlyout = DialogService.CreateIncomingSharedItemsSortMenu(Contact.SharedItems, true);

            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(sortButton);
        }

        private void OnSharedItemTapped(object sender, TappedRoutedEventArgs e)
        {
            IMegaSharedFolderNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaSharedFolderNode;
            if (itemTapped == null) return;

            this.Contact.SharedItems.ItemCollection.FocusedItem = itemTapped;
        }

        private void OnSharedItemRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            IMegaSharedFolderNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaSharedFolderNode;
            if (itemTapped == null) return;

            this.Contact.SharedItems.ItemCollection.FocusedItem = itemTapped;

            var view = (ListViewBase)sender;
            if (view == null) return;

            if (this.Contact.SharedItems.ItemCollection.IsMultiSelectActive)
                view.SelectedItems?.Add(itemTapped);
            else
                view.SelectedItem = itemTapped;
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this._sharedItemsTopCommandBarArea != null)
            {
                this._sharedItemsTopCommandBarArea.Visibility =
                    this._pivotControl != null && this._sharedItemsPivot != null && this._pivotControl.SelectedItem.Equals(this._sharedItemsPivot) ?
                    Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            if (!(bool)((PivotItem)(this._pivotControl?.SelectedItem))?.Equals(this._sharedItemsPivot)) return;

            // Disable the view behaviors to avoid strange behaviors during the view update
            Interaction.GetBehaviors(this._sharedItems).Detach();

            // First save the current selection to restore after enable the multi select
            var selection = this.Contact.SharedItems.ItemCollection.SelectedItems.ToList();

            this._sharedItems.SelectionMode = ListViewSelectionMode.Multiple;

            // Update the selection
            foreach (var item in selection)
                this._sharedItems.SelectedItems.Add(item);

            // Restore the view behaviors again
            Interaction.GetBehaviors(this._sharedItems).Attach(this._sharedItems);
        }

        private void OnMultiSelectDisabled(object sender, EventArgs e)
        {
            if (!(bool)((PivotItem)(this._pivotControl?.SelectedItem))?.Equals(this._sharedItemsPivot)) return;

            this._sharedItems.SelectionMode = DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop ?
                ListViewSelectionMode.Extended : ListViewSelectionMode.Single;
        }

        private void OnAllSelected(object sender, bool value)
        {
            if (!(bool)((PivotItem)(this._pivotControl?.SelectedItem))?.Equals(this._sharedItemsPivot)) return;

            if (value)
                this._sharedItems?.SelectAll();
            else
                this._sharedItems?.SelectedItems.Clear();
        }
    }
}
