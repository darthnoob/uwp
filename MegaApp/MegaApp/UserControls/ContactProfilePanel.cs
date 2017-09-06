using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
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
                control.OnContactChanged((ContactViewModel)dpc.NewValue);
            }
        }

        private AppBarButton _sortSharedItemsButton;
        private Button _removeContact;
        private ListView _sharedItems;
        private Pivot _pivotControl;
        private PivotItem _contactProfilePivot;
        private PivotItem _sharedItemsPivot;
        private Grid _sharedItemsTopCommandBarArea;

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
            this._contactProfilePivot = (PivotItem)this.GetTemplateChild("PART_ContactProfilePivot");
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

        protected void OnContactChanged(ContactViewModel contact)
        {
            
        }

        private async void OnRemoveContactTapped()
        {
            await this.Contact.RemoveContactAsync();
        }

        private void OnSortSharedItemsButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            var sortButton = sender as Button;
            if (sortButton == null) return;

            MenuFlyout menuFlyout = DialogService.CreateContactSharedItemsSortMenu(Contact.SharedItems);

            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(sortButton);
        }

        private void OnSharedItemTapped(object sender, TappedRoutedEventArgs e)
        {
            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            this.Contact.SharedItems.ItemCollection.FocusedItem = itemTapped;
        }

        private void OnSharedItemRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;

            IMegaNode itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaNode;
            if (itemTapped == null) return;

            this.Contact.SharedItems.ItemCollection.FocusedItem = itemTapped;

            if (!this.Contact.SharedItems.IsMultiSelectActive)
                ((ListViewBase)sender).SelectedItems?.Clear();

            ((ListViewBase)sender).SelectedItems?.Add(itemTapped);
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._sharedItemsTopCommandBarArea.Visibility =
                this._pivotControl != null && this._pivotControl.SelectedItem.Equals(this._sharedItemsPivot) ?
                Visibility.Visible : Visibility.Collapsed;
        }
    }
}
