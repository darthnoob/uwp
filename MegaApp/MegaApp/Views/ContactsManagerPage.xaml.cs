using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Xaml.Interactivity;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;
using MegaApp.ViewModels.Contacts;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseContactsManagerPage : PageEx<ContactsManagerViewModel> { }

    public sealed partial class ContactsManagerPage : BaseContactsManagerPage
    {
        private const double ContactProfilePanelMinWidth = 886;

        public ContactsManagerPage()
        {
            this.InitializeComponent();

            this.ContactProfileSplitView.RegisterPropertyChangedCallback(
                SplitView.IsPaneOpenProperty, IsProfileViewOpenPropertyChanged);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Initialize();

            this.ViewModel.MegaContacts.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ViewModel.MegaContacts.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
            this.ViewModel.MegaContacts.ItemCollection.AllSelected += OnAllSelected;
            this.ViewModel.MegaContacts.OpenContactProfileEvent += OnOpenContactProfile;
            this.ViewModel.MegaContacts.CloseContactProfileEvent += OnCloseContactProfile;

            this.ViewModel.IncomingContactRequests.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ViewModel.IncomingContactRequests.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
            this.ViewModel.IncomingContactRequests.ItemCollection.AllSelected += OnAllSelected;

            this.ViewModel.OutgoingContactRequests.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ViewModel.OutgoingContactRequests.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
            this.ViewModel.OutgoingContactRequests.ItemCollection.AllSelected += OnAllSelected;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.MegaContacts.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
            this.ViewModel.MegaContacts.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
            this.ViewModel.MegaContacts.ItemCollection.AllSelected -= OnAllSelected;
            this.ViewModel.MegaContacts.OpenContactProfileEvent -= OnOpenContactProfile;
            this.ViewModel.MegaContacts.CloseContactProfileEvent -= OnCloseContactProfile;

            this.ViewModel.IncomingContactRequests.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
            this.ViewModel.IncomingContactRequests.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
            this.ViewModel.IncomingContactRequests.ItemCollection.AllSelected -= OnAllSelected;

            this.ViewModel.OutgoingContactRequests.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
            this.ViewModel.OutgoingContactRequests.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
            this.ViewModel.OutgoingContactRequests.ItemCollection.AllSelected -= OnAllSelected;

            this.ViewModel.Deinitialize();
            base.OnNavigatedFrom(e);
        }

        private void IsProfileViewOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (this.ContactProfileSplitView.IsPaneOpen)
            {
                if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop || this.ContactProfileSplitView.ActualWidth < 600)
                {
                    this.ContactProfileSplitView.OpenPaneLength = this.ContactProfileSplitView.ActualWidth;
                    AppService.SetAppViewBackButtonVisibility(true);
                    return;
                }

                this.ContactProfileSplitView.OpenPaneLength = ContactProfilePanelMinWidth;
            }

            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        public override bool CanGoBack
        {
            get
            {
                bool canGoBack = false;
                if (this.ViewModel != null)
                    canGoBack = this.ViewModel.IsPanelOpen;

                return canGoBack;
            }
        }

        public override void GoBack()
        {
            if (ContactProfileSplitView.IsPaneOpen)
                this.ViewModel.ClosePanels();
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            this.ViewModel.IsPanelOpen = false;

            // Needed to avoid strange behaviors during the view update
            DisableViewsBehaviors();

            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.ContactsPivot))
            {
                // First save the current selected contacts to restore them after enable the multi select
                var selectedContacts = this.ViewModel.MegaContacts.ItemCollection.SelectedItems.ToList();

                this.ListViewContacts.SelectionMode = ListViewSelectionMode.Multiple;

                // Update the selected contacts
                foreach (var contact in selectedContacts)
                    this.ListViewContacts.SelectedItems.Add(contact);
            }
            else
            {
                // For save the current selected contact requests to restore them after enable the multi select
                List<IMegaContactRequest> selectedContactRequest;

                if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.IncomingPivot))
                {
                    selectedContactRequest =  this.ViewModel.IncomingContactRequests.ItemCollection.SelectedItems.ToList();
                    this.ListViewIncomingContactRequests.SelectionMode = ListViewSelectionMode.Multiple;
                    foreach (var contactRequest in selectedContactRequest)
                        this.ListViewIncomingContactRequests.SelectedItems.Add(contactRequest);
                }                    

                if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.OutgoingPivot))
                {
                    selectedContactRequest = this.ViewModel.OutgoingContactRequests.ItemCollection.SelectedItems.ToList();
                    this.ListViewOutgoingContactRequests.SelectionMode = ListViewSelectionMode.Multiple;
                    foreach (var contactRequest in selectedContactRequest)
                        this.ListViewOutgoingContactRequests.SelectedItems.Add(contactRequest);
                }
            }            

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        private void OnMultiSelectDisabled(object sender, EventArgs e)
        {
            var listView = this.GetSelectedListView();
            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
                listView.SelectionMode = ListViewSelectionMode.Extended;
            else
                listView.SelectionMode = ListViewSelectionMode.Single;
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

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.IsPanelOpen = false;

            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.ContactsPivot))
                this.ViewModel.ActiveView = this.ViewModel.MegaContacts;

            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.IncomingPivot))
                this.ViewModel.ActiveView = this.ViewModel.IncomingContactRequests;

            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.OutgoingPivot))
                this.ViewModel.ActiveView = this.ViewModel.OutgoingContactRequests;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();
        }

        private void OnContactTapped(object sender, TappedRoutedEventArgs e)
        {
            IMegaContact itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaContact;
            if (itemTapped == null) return;

            this.ViewModel.MegaContacts.ItemCollection.FocusedItem = itemTapped;
        }

        private void OnContactRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            IMegaContact itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaContact;
            if (itemTapped == null) return;

            if (!(this.ViewModel.ActiveView is ContactsListViewModel)) return;

            this.ViewModel.MegaContacts.ItemCollection.FocusedItem = itemTapped;

            if (!this.ViewModel.MegaContacts.ItemCollection.IsMultiSelectActive)
                ((ListView)sender).SelectedItems?.Clear();

            ((ListView)sender).SelectedItems?.Add(itemTapped);
        }

        private void OnContactRequestTapped(object sender, TappedRoutedEventArgs e)
        {
            IMegaContactRequest itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaContactRequest;
            if (itemTapped == null) return;

            if (!(this.ViewModel.ActiveView is ContactRequestsListViewModel)) return;

            var activeView = this.ViewModel.ActiveView as ContactRequestsListViewModel;
            activeView.ItemCollection.FocusedItem = itemTapped;
        }

        private void OnContactRequestRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;

            IMegaContactRequest itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaContactRequest;
            if (itemTapped == null) return;

            if (!(this.ViewModel.ActiveView is ContactRequestsListViewModel)) return;

            var activeView = this.ViewModel.ActiveView as ContactRequestsListViewModel;
            activeView.ItemCollection.FocusedItem = itemTapped;

            if (!activeView.ItemCollection.IsMultiSelectActive)
                ((ListViewBase)sender).SelectedItems?.Clear();

            ((ListViewBase)sender).SelectedItems?.Add(itemTapped);
        }

        private ListView GetSelectedListView()
        {
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.ContactsPivot))
                return this.ListViewContacts;
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.IncomingPivot))
                return this.ListViewIncomingContactRequests;
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.OutgoingPivot))
                return this.ListViewOutgoingContactRequests;
            return null;
        }        

        private void OnSelectAllTapped(object sender, TappedRoutedEventArgs e)
        {
            var listView = this.GetSelectedListView();
            listView?.SelectAll();
        }

        private void OnDeselectAllTapped(object sender, TappedRoutedEventArgs e)
        {
            var listView = this.GetSelectedListView();
            listView?.SelectedItems.Clear();
        }

        private void OnSortClick(object sender, RoutedEventArgs e)
        {
            var sortButton = sender as Button;
            if (sortButton == null) return;

            MenuFlyout menuFlyout = null;
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.ContactsPivot))
                menuFlyout = DialogService.CreateContactsSortMenu(this.ViewModel.MegaContacts);
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.IncomingPivot))
                menuFlyout = DialogService.CreateContactRequestsSortMenu(this.ViewModel.IncomingContactRequests);
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.OutgoingPivot))
                menuFlyout = DialogService.CreateContactRequestsSortMenu(this.ViewModel.OutgoingContactRequests);

            if (menuFlyout == null) return;
            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(sortButton);
        }

        private void OnOpenContactProfile(object sender, EventArgs e)
        {
            this.ViewModel.IsPanelOpen = true;
        }

        private void OnCloseContactProfile(object sender, EventArgs e)
        {
            this.ViewModel.IsPanelOpen = false;
        }
    }
}
