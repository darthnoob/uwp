using System;
using mega;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    public class GlobalListener: MGlobalListenerInterface
    {
        public event EventHandler<MNode> NodeAdded;
        public event EventHandler<MNode> NodeRemoved;
        
        #region MGlobalListenerInterface

        public void onNodesUpdate(MegaSDK api, MNodeList nodes)
        {
            // Exit methods when node list is incorrect
            if (nodes == null || nodes.size() < 1) return;

            try
            {
                // Retrieve the listsize for performance reasons and store local
                int listSize = nodes.size();

                for (int i = 0; i < listSize; i++)
                {
                    // Get the specific node that has an update. If null exit the method
                    // and process no notification
                    MNode megaNode = nodes.get(i);
                    if (megaNode == null) return;

                    if (megaNode.isRemoved())
                    {
                        // REMOVED Scenario
                        OnNodeRemoved(megaNode);
                    }
                    else
                    {
                        // ADDED / UPDATE scenarions
                        OnNodeAdded(megaNode);
                    }
                }                
            }
            catch (Exception) { /* Dummy catch, surpress possible exception */ }
        }

        public void onReloadNeeded(MegaSDK api)
        {
           // throw new NotImplementedException();
        }

        public void onAccountUpdate(MegaSDK api)
        {
            //UiService.OnUiThread(() =>
            //{
            //    var customMessageDialog = new CustomMessageDialog(
            //        AppMessages.AccountUpdated_Title,
            //        AppMessages.AccountUpdate,
            //        App.AppInformation,
            //        MessageDialogButtons.YesNo);

            //    customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
            //    {
            //        NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
            //    };                

            //    customMessageDialog.ShowDialog();
            //});
        }

        public void onContactRequestsUpdate(MegaSDK api, MContactRequestList requests)
        {            
            //foreach (var contacts in Contacts)
            //{
            //    UiService.OnUiThread(() =>
            //    {
            //        contacts.GetReceivedContactRequests();
            //        contacts.GetSentContactRequests();
            //    });
            //}
        }

        public async void onUsersUpdate(MegaSDK api, MUserList users)
        {
            // Exit methods when users list is incorrect
            if (users == null || users.size() < 1) return;

            // Retrieve the listsize for performance reasons and store local
            int listSize = users.size();

            for (int i = 0; i < listSize; i++)
            {
                MUser user = users.get(i);
                if (user == null) continue;

                // If the change is on the current user                
                if(user.getHandle().Equals(api.getMyUser().getHandle()) && !Convert.ToBoolean(user.isOwnChange()))
                {
                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_AVATAR) &&
                        !string.IsNullOrWhiteSpace(AccountService.UserData.AvatarPath))
                        AccountService.GetUserAvatar();

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_EMAIL))
                        await AccountService.GetUserEmail();

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_FIRSTNAME))
                        AccountService.GetUserFirstname();

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_LASTNAME))
                        AccountService.GetUserLastname();
                }
                else // If the change is on a contact
                {
            //        // If there are any ContactsViewModel active
            //        foreach (var contactViewModel in Contacts)
            //        {
            //            Contact existingContact = contactViewModel.MegaContactsList.FirstOrDefault(
            //                contact => contact.Handle.Equals(user.getHandle()));

            //            // If the contact exists in the contact list
            //            if(existingContact != null)
            //            {
            //                // If the contact is no longer a contact (REMOVE CONTACT SCENARIO)
            //                if (!existingContact.Visibility.Equals(user.getVisibility()) && 
            //                    !(user.getVisibility().Equals(MUserVisibility.VISIBILITY_VISIBLE)))
            //                {
            //                    UiService.OnUiThread(() =>
            //                        contactViewModel.MegaContactsList.Remove(existingContact));
            //                }
            //                // If the contact has been changed (UPDATE CONTACT SCENARIO) and is not an own change
            //                else if (!Convert.ToBoolean(user.isOwnChange())) 
            //                {
            //                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_AVATAR) &&
            //                        !String.IsNullOrWhiteSpace(existingContact.AvatarPath))
            //                    {
            //                        api.getUserAvatar(user, existingContact.AvatarPath, 
            //                            new GetContactAvatarRequestListener(existingContact));
            //                    }

            //                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_EMAIL))
            //                    {
            //                        UiService.OnUiThread(() =>
            //                            existingContact.Email = user.getEmail());
            //                    }

            //                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_FIRSTNAME))
            //                    {
            //                        api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_FIRSTNAME, 
            //                            new GetContactDataRequestListener(existingContact));
            //                    }

            //                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_LASTNAME))
            //                    {
            //                        api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_LASTNAME, 
            //                            new GetContactDataRequestListener(existingContact));
            //                    }
            //                }
            //            }
            //            // If is a new contact (ADD CONTACT SCENARIO - REQUEST ACCEPTED)
            //            else if (user.getVisibility().Equals(MUserVisibility.VISIBILITY_VISIBLE))
            //            {
            //                var _megaContact = new Contact()
            //                {
            //                    Handle = user.getHandle(),
            //                    Email = user.getEmail(),
            //                    Timestamp = user.getTimestamp(),
            //                    Visibility = user.getVisibility(),
            //                    AvatarColor = UiService.GetColorFromHex(App.MegaSdk.getUserAvatarColor(user))
            //                };

            //                UiService.OnUiThread(() =>
            //                    contactViewModel.MegaContactsList.Add(_megaContact));

            //                api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_FIRSTNAME, 
            //                    new GetContactDataRequestListener(_megaContact));
            //                api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_LASTNAME, 
            //                    new GetContactDataRequestListener(_megaContact));
            //                api.getUserAvatar(user, _megaContact.AvatarPath, 
            //                    new GetContactAvatarRequestListener(_megaContact));                            
            //            }
                    }

            //        // If there are any ContactDetailsViewModel active
            //        foreach (var contactDetailsViewModel in ContactsDetails)
            //        {
            //            // If the selected contact has been changed (UPDATE CONTACT SCENARIO)
            //            if (contactDetailsViewModel.SelectedContact.Handle.Equals(user.getHandle()))
            //            {                            
            //                if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_AVATAR) &&
            //                    !String.IsNullOrWhiteSpace(contactDetailsViewModel.SelectedContact.AvatarPath))
            //                {
            //                    api.getUserAvatar(user, contactDetailsViewModel.SelectedContact.AvatarPath,
            //                        new GetContactAvatarRequestListener(contactDetailsViewModel.SelectedContact));
            //                }

            //                if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_EMAIL))
            //                {
            //                    UiService.OnUiThread(() =>
            //                        contactDetailsViewModel.SelectedContact.Email = user.getEmail());
            //                }

            //                if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_FIRSTNAME))
            //                {
            //                    api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_FIRSTNAME,
            //                        new GetContactDataRequestListener(contactDetailsViewModel.SelectedContact));                                
            //                }

            //                if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_LASTNAME))
            //                {
            //                    api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_LASTNAME,
            //                        new GetContactDataRequestListener(contactDetailsViewModel.SelectedContact));
            //                }
            //            }
            //        }
            //    }
            }
        }

        #endregion

        protected virtual void OnNodeAdded(MNode e)
        {
            NodeAdded?.Invoke(this, e);
        }

        protected virtual void OnNodeRemoved(MNode e)
        {
            NodeRemoved?.Invoke(this, e);
        }
    }
}
