using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
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
        public ContactsManagerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Initialize(App.GlobalListener);

            this.ViewModel.MegaContacts.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ViewModel.MegaContacts.MultiSelectDisabled += OnMultiSelectDisabled;

            this.ViewModel.IncomingContactRequests.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ViewModel.IncomingContactRequests.MultiSelectDisabled += OnMultiSelectDisabled;

            this.ViewModel.OutgoingContactRequests.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ViewModel.OutgoingContactRequests.MultiSelectDisabled += OnMultiSelectDisabled;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.MegaContacts.MultiSelectEnabled -= OnMultiSelectEnabled;
            this.ViewModel.MegaContacts.MultiSelectDisabled -= OnMultiSelectDisabled;

            this.ViewModel.IncomingContactRequests.MultiSelectEnabled -= OnMultiSelectEnabled;
            this.ViewModel.IncomingContactRequests.MultiSelectDisabled -= OnMultiSelectDisabled;

            this.ViewModel.OutgoingContactRequests.MultiSelectEnabled -= OnMultiSelectEnabled;
            this.ViewModel.OutgoingContactRequests.MultiSelectDisabled -= OnMultiSelectDisabled;

            this.ViewModel.Deinitialize(App.GlobalListener);
            base.OnNavigatedFrom(e);
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.ContactsPivot))
            {
                // First save the current selected contacts to restore them after enable the multi select
                var selectedContacts = this.ViewModel.MegaContacts.List.SelectedItems.ToList();

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
                    selectedContactRequest =  this.ViewModel.IncomingContactRequests.List.SelectedItems.ToList();
                    this.ListViewIncomingContactRequests.SelectionMode = ListViewSelectionMode.Multiple;
                    foreach (var contactRequest in selectedContactRequest)
                        this.ListViewIncomingContactRequests.SelectedItems.Add(contactRequest);
                }                    

                if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.OutgoingPivot))
                {
                    selectedContactRequest = this.ViewModel.OutgoingContactRequests.List.SelectedItems.ToList();
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
            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
            {
                if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.ContactsPivot))
                    this.ListViewContacts.SelectionMode = ListViewSelectionMode.Extended;

                if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.IncomingPivot))
                    this.ListViewIncomingContactRequests.SelectionMode = ListViewSelectionMode.Extended;

                if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.OutgoingPivot))
                    this.ListViewOutgoingContactRequests.SelectionMode = ListViewSelectionMode.Extended;
            }
            else
            {
                if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.ContactsPivot))
                    this.ListViewContacts.SelectionMode = ListViewSelectionMode.Single;

                if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.IncomingPivot))
                    this.ListViewIncomingContactRequests.SelectionMode = ListViewSelectionMode.Single;

                if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.OutgoingPivot))
                    this.ListViewOutgoingContactRequests.SelectionMode = ListViewSelectionMode.Single;
            }
        }

        /// <summary>
        /// Enable the behaviors of the active view
        /// </summary>
        private void EnableViewsBehaviors()
        {
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.ContactsPivot))
                Interaction.GetBehaviors(this.ListViewContacts).Attach(this.ListViewContacts);

            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.IncomingPivot))
                Interaction.GetBehaviors(this.ListViewIncomingContactRequests).Attach(this.ListViewIncomingContactRequests);

            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.OutgoingPivot))
                Interaction.GetBehaviors(this.ListViewOutgoingContactRequests).Attach(this.ListViewOutgoingContactRequests);
        }

        /// <summary>
        /// Disable the behaviors of the current active view
        /// </summary>
        private void DisableViewsBehaviors()
        {
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.ContactsPivot))
                Interaction.GetBehaviors(this.ListViewContacts).Detach();

            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.IncomingPivot))
                Interaction.GetBehaviors(this.ListViewIncomingContactRequests).Detach();

            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.OutgoingPivot))
                Interaction.GetBehaviors(this.ListViewOutgoingContactRequests).Detach();
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

            this.ViewModel.MegaContacts.FocusedItem = itemTapped;
        }

        private void OnContactRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;

            IMegaContact itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaContact;
            if (itemTapped == null) return;

            this.ViewModel.MegaContacts.FocusedItem = itemTapped;

            if (!this.ViewModel.MegaContacts.IsMultiSelectActive)
                ((ListViewBase)sender).SelectedItems.Clear();

            ((ListViewBase)sender).SelectedItems.Add(itemTapped);
        }

        private void OnContactRequestTapped(object sender, TappedRoutedEventArgs e)
        {
            IMegaContactRequest itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaContactRequest;
            if (itemTapped == null) return;

            if (this.ViewModel.ActiveView is ContactRequestsListViewModel)
            {
                var activeView = this.ViewModel.ActiveView as ContactRequestsListViewModel;
                activeView.FocusedItem = itemTapped;
            }
        }

        private void OnContactRequestRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop) return;

            IMegaContactRequest itemTapped = ((FrameworkElement)e.OriginalSource)?.DataContext as IMegaContactRequest;
            if (itemTapped == null) return;

            if (this.ViewModel.ActiveView is ContactRequestsListViewModel)
            {
                var activeView = this.ViewModel.ActiveView as ContactRequestsListViewModel;
                activeView.FocusedItem = itemTapped;

                if (!activeView.IsMultiSelectActive)
                    ((ListViewBase)sender).SelectedItems.Clear();

                ((ListViewBase)sender).SelectedItems.Add(itemTapped);
            }
        }

        private void SelectAllCheckBoxTapped(object sender, TappedRoutedEventArgs e)
        {
            ListView listView = null;
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.ContactsPivot))
                listView = this.ListViewContacts;
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.IncomingPivot))
                listView = this.ListViewIncomingContactRequests;
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.OutgoingPivot))
                listView = this.ListViewOutgoingContactRequests;

            CheckBox checkBox = sender as CheckBox;
            if (checkBox?.IsChecked == true)
                listView?.SelectAll();
            else
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

            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout?.ShowAt(sortButton);
        }
    }
}
