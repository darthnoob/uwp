using System;
using System.Collections.Generic;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.Views;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    public class GlobalListener: MGlobalListenerInterface
    {
        public event EventHandler<MNode> NodeAdded;
        public event EventHandler<MNode> NodeRemoved;

        public event EventHandler<MNode> InSharedFolderAdded;
        public event EventHandler<MNode> InSharedFolderRemoved;
        public event EventHandler<MNode> OutSharedFolderAdded;
        public event EventHandler<MNode> OutSharedFolderRemoved;

        public event EventHandler<MUser> ContactUpdated;
        public event EventHandler IncomingContactRequestUpdated;
        public event EventHandler OutgoingContactRequestUpdated;

        #region MGlobalListenerInterface

        public void onNodesUpdate(MegaSDK api, MNodeList nodes)
        {
            // Exit methods when node list is incorrect
            if (nodes == null || nodes.size() < 1) return;

            // Retrieve the listsize for performance reasons and store local
            int listSize = nodes.size();

            for (int i = 0; i < listSize; i++)
            {
                try
                {
                    // Get the specific node that has an update. If null exit the method
                    // and process no notification
                    MNode megaNode = nodes.get(i);
                    if (megaNode == null) return;

                    // Incoming shared folder
                    if (megaNode.isInShare())
                    {
                        if (megaNode.isRemoved()) // REMOVED Scenario
                            OnInSharedFolderRemoved(megaNode);
                        else // ADDED/UPDATED scenarios
                            OnInSharedFolderAdded(megaNode);
                    }
                    // Outgoing shared folder - ADDED/UPDATED scenarios
                    else if (megaNode.isOutShare())
                    {
                        OnOutSharedFolderAdded(megaNode);
                    }
                    else
                    {
                        // Outgoing shared folder - REMOVED Scenario
                        if (megaNode.hasChanged((int)MNodeChangeType.CHANGE_TYPE_OUTSHARE))
                            OnOutSharedFolderRemoved(megaNode);

                        // Normal node
                        if (megaNode.isRemoved()) // REMOVED Scenario
                            OnNodeRemoved(megaNode);
                        else // ADDED/UPDATED scenarions
                            OnNodeAdded(megaNode);
                    }
                }
                catch (Exception) { /* Dummy catch, suppress possible exception */ }
            }
        }

        public void onReloadNeeded(MegaSDK api)
        {
           // throw new NotImplementedException();
        }

        public void onAccountUpdate(MegaSDK api)
        {
            UiService.OnUiThread(() =>
            {
                var customMessageDialog = new CustomMessageDialog(
                    ResourceService.AppMessages.GetString("AM_AccountUpdated_Title"),
                    ResourceService.AppMessages.GetString("AM_AccountUpdate"),
                    App.AppInformation,
                    MessageDialogButtons.YesNo);

                customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                {
                    NavigateService.Instance.Navigate(typeof(MyAccountPage), false,
                        NavigationObject.Create(typeof(MainViewModel), NavigationActionType.Default));
                };

                customMessageDialog.ShowDialog();
            });
        }

        public void onContactRequestsUpdate(MegaSDK api, MContactRequestList requests)
        {
            // Exit methods when contact request list is incorrect
            if (requests == null || requests.size() < 1) return;

            try
            {
                bool isIncomingContactRequestUpdate = false;
                bool isOutgoingContactRequestUpdate = false;

                // Retrieve the listsize for performance reasons and store local
                int listSize = requests.size();

                for (int i = 0; i < listSize; i++)
                {
                    // Get the specific contact request that has an update. 
                    // If null exit the method and process no notification.
                    MContactRequest megaContactRequest = requests.get(i);
                    if (megaContactRequest == null) return;

                    if (megaContactRequest.isOutgoing())
                        isOutgoingContactRequestUpdate = true;
                    else
                        isIncomingContactRequestUpdate = true;
                }

                if (isIncomingContactRequestUpdate)
                    OnIncomingContactRequestUpdated();
                if (isOutgoingContactRequestUpdate)
                    OnOutgoingContactRequestUpdated();
            }
            catch (Exception) { /* Dummy catch, suppress possible exception */ }
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
                if (user.getHandle().Equals(api.getMyUser().getHandle()) && !Convert.ToBoolean(user.isOwnChange()))
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
                    OnContactUpdated(user);
                }
            }
        }

        public void onEvent(MegaSDK api, MEvent ev)
        {
            // If the account has been blocked
            if (ev.getType() == MEventType.EVENT_ACCOUNT_BLOCKED)
            {
                AccountService.IsAccountBlocked = true;

                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Blocked account: " + ev.getText());

                // A blocked account automatically triggers a logout
                AppService.LogoutActions();

                // Show the login page with the corresponding navigation parameter
                UiService.OnUiThread(() =>
                {
                    NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true,
                        NavigationObject.Create(typeof(MainViewModel), NavigationActionType.API_EBLOCKED,
                        new Dictionary<NavigationParamType, object>
                        {
                            { NavigationParamType.Number, ev.getNumber() },
                            { NavigationParamType.Text, ev.getText() }
                        }));
                });
            }
        }

        #endregion

        protected virtual void OnNodeAdded(MNode e) => NodeAdded?.Invoke(this, e);
        protected virtual void OnNodeRemoved(MNode e) => NodeRemoved?.Invoke(this, e);

        protected virtual void OnInSharedFolderAdded(MNode e) => InSharedFolderAdded?.Invoke(this, e);
        protected virtual void OnInSharedFolderRemoved(MNode e) => InSharedFolderRemoved?.Invoke(this, e);
        protected virtual void OnOutSharedFolderAdded(MNode e) => OutSharedFolderAdded?.Invoke(this, e);
        protected virtual void OnOutSharedFolderRemoved(MNode e) => OutSharedFolderRemoved?.Invoke(this, e);

        protected virtual void OnContactUpdated(MUser e) => ContactUpdated?.Invoke(this, e);

        protected virtual void OnIncomingContactRequestUpdated() => IncomingContactRequestUpdated?.Invoke(this, EventArgs.Empty);
        protected virtual void OnOutgoingContactRequestUpdated() => OutgoingContactRequestUpdated?.Invoke(this, EventArgs.Empty);
    }
}
