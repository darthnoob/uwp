using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views.Dialogs;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactsListOutgoingSharedFolderViewModel : ContactsListViewModel
    {
        public ContactsListOutgoingSharedFolderViewModel(MNode sharedFolder)
        {
            this._sharedFolder = sharedFolder;

            this.AddContactToFolderCommand = new RelayCommand(AddContactToFolder);
            this.ChangePermissionsCommand = new RelayCommand<MShareType>(ChangePermissions);
            this.RemoveContactFromFolderCommand = new RelayCommand(RemoveContactFromFolder);
            this.SetFolderPermissionCommand = new RelayCommand(SetFolderPermission);
        }

        #region Commands

        public ICommand AddContactToFolderCommand { get; }
        public ICommand ChangePermissionsCommand { get; }
        public ICommand RemoveContactFromFolderCommand { get; }
        public ICommand SetFolderPermissionCommand { get; }

        #endregion

        #region Methods

        public override void Initialize()
        {
            if (App.GlobalListener == null) return;
            App.GlobalListener.ContactUpdated += this.OnContactUpdated;
            App.GlobalListener.OutSharedFolderAdded += this.OnOutSharedFolderUpdated;
            App.GlobalListener.OutSharedFolderRemoved += this.OnOutSharedFolderUpdated;
        }

        public override void Deinitialize()
        {
            if (App.GlobalListener == null) return;
            App.GlobalListener.ContactUpdated -= this.OnContactUpdated;
            App.GlobalListener.OutSharedFolderAdded -= this.OnOutSharedFolderUpdated;
            App.GlobalListener.OutSharedFolderRemoved -= this.OnOutSharedFolderUpdated;
        }

        private async void AddContactToFolder()
        {
            var shareFolderToDialog = new ShareFolderToDialog(this._sharedFolder.getName());
            var dialogResult = await shareFolderToDialog.ShowAsync();

            if (dialogResult != ContentDialogResult.Primary) return;

            var share = new ShareRequestListenerAsync();
            var result = await share.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.shareByEmail(this._sharedFolder,
                    shareFolderToDialog.ViewModel.ContactEmail,
                    (int)shareFolderToDialog.ViewModel.AccessLevel, share);
            });

            if(!result)
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_ShareFolderFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_ShareFolderFailed"));
                });
            }
        }

        /// <summary>
        /// Modify the selected contacts permissions over an outgoing shared folder
        /// </summary>
        /// <param name="newAccessLevel">New access level</param>
        private void ChangePermissions(MShareType newAccessLevel)
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            // Use a temp variable to avoid InvalidOperationException
            this.ChangePermissionsMultipleContacts(this.ItemCollection.SelectedItems.ToList(), newAccessLevel);

            this.ItemCollection.IsMultiSelectActive = false;
        }

        /// <summary>
        /// Modify the selected contacts permissions over an outgoing shared folder
        /// </summary>
        /// <param name="newAccessLevel">New access level</param>
        private async void ChangePermissionsMultipleContacts(ICollection<IMegaContact> contacts, MShareType newAccessLevel)
        {
            foreach (var contact in contacts)
                await (contact as ContactOutgoingSharedFolderViewModel).ChangePermissionsAsync(newAccessLevel);
        }

        /// <summary>
        /// Remove the selected contacts access from an outgoing shared folder
        /// </summary>
        private void RemoveContactFromFolder()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            // Use a temp variable to avoid InvalidOperationException
            this.RemoveMultipleContactsFromFolder(this.ItemCollection.SelectedItems.ToList());
        }

        /// <summary>
        /// Remove the selected contacts access from an outgoing shared folder
        /// </summary>
        private async void RemoveMultipleContactsFromFolder(ICollection<IMegaContact> contacts)
        {
            foreach (var contact in contacts)
                await ((ContactOutgoingSharedFolderViewModel)contact).RemoveContactFromFolderAsync();
        }

        private void OnOutSharedFolderUpdated(object sender, MNode node)
        {
            if (node.getHandle() != this._sharedFolder.getHandle()) return;

            var outSharesList = SdkService.MegaSdk.getOutShares(node);
            var outSharesListSize = outSharesList.size();

            // If the folder is no longer shared with one or more contacts of the list (REMOVE SCENARIO)
            if (outSharesListSize < this.ItemCollection.Items.Count)
            {
                List<ContactOutgoingSharedFolderViewModel> contactsToRemove = new List<ContactOutgoingSharedFolderViewModel>();
                foreach (var item in this.ItemCollection.Items)
                {
                    bool foundItem = false;
                    for (int i = 0; i < outSharesListSize; i++)
                    {
                        var contact = SdkService.MegaSdk.getContact(outSharesList.get(i).getUser());
                        if (item.Handle != contact.getHandle()) continue;
                        foundItem = true;
                        break;
                    }

                    if (!foundItem)
                        contactsToRemove.Add(item as ContactOutgoingSharedFolderViewModel);
                }

                foreach(var item in contactsToRemove)
                {
                    if (item != null)
                        OnUiThread(() => this.ItemCollection.Items.Remove(item));
                }
            }
            // If folder is shared with a new contact or permission have changed (ADD and UPDATE scenarios)
            else
            {
                for (int i = 0; i < outSharesListSize; i++)
                {
                    var contact = SdkService.MegaSdk.getContact(outSharesList.get(i)?.getUser());
                    if (contact == null)
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Contact is NULL");
                        continue;
                    }

                    var existingContact = this.ItemCollection.Items.FirstOrDefault(
                        c => c.Handle.Equals(contact.getHandle()));

                    // If the contact exists in the contact list (UPDATE SCENARIO)
                    if (existingContact != null && existingContact is ContactOutgoingSharedFolderViewModel)
                    {
                        (existingContact as ContactOutgoingSharedFolderViewModel).GetAccesLevel(outSharesList.get(i));
                    }
                    // If the shared folder is shared with a new contact (ADD SCENARIO)
                    else
                    {
                        var megaContact = new ContactOutgoingSharedFolderViewModel(outSharesList.get(i), this);

                        OnUiThread(() => this.ItemCollection.Items.Add(megaContact));

                        megaContact.GetContactFirstname();
                        megaContact.GetContactLastname();
                        megaContact.GetContactAvatarColor();
                        megaContact.GetContactAvatar();
                        megaContact.GetAccesLevel(outSharesList.get(i));
                    }
                }
            }
        }

        private async void SetFolderPermission()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            var dialog = new SetSharedFolderPermissionDialog();
            var dialogResult = await dialog.ShowAsync();

            if (dialogResult != ContentDialogResult.Primary) return;

            this.ChangePermissions(dialog.ViewModel.AccessLevel);
        }

        #endregion

        #region Properties

        private readonly MNode _sharedFolder;

        #endregion
    }
}
