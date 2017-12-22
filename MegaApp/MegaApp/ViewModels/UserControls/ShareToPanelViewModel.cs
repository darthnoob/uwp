using System;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.ViewModels.Contacts;
using MegaApp.MegaApi;

namespace MegaApp.ViewModels.UserControls
{
    public class ShareToPanelViewModel : BaseUiViewModel
    {
        public ShareToPanelViewModel()
        {
            this.CancelCommand = new RelayCommand(Cancel);
            this.ConfirmShareCommand = new RelayCommand(ConfirmShare);
        }

        #region Events

        /// <summary>
        /// Event triggered when user tap the cancel/close button.
        /// </summary>
        public EventHandler ClosePanelEvent;

        /// <summary>
        /// Event invocator method called when user tap the cancel/close button.
        /// </summary>
        protected virtual void OnClosePanelEvent()
        {
            ClosePanelEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Commands

        public ICommand CancelCommand { get; private set; }
        public ICommand ConfirmShareCommand { get; private set; }

        #endregion

        #region Methods

        private void Cancel()
        {
            this.OnClosePanelEvent();
        }

        private async void ConfirmShare()
        {
            if (!MegaContacts.ItemCollection.HasSelectedItems) return;

            this.OnClosePanelEvent();

            bool result = true;
            foreach (var contact in this.MegaContacts.ItemCollection.SelectedItems)
            {
                var share = new ShareRequestListenerAsync();
                result = result & await share.ExecuteAsync(() =>
                {
                    SdkService.MegaSdk.share(this.Node.OriginalMNode,
                        contact.MegaUser, (int)MShareType.ACCESS_READ, share);
                });
            }

            if (!result)
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_ShareFolderFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_ShareFolderFailed"));
                });
            }
        }

        #endregion

        #region Properties

        public ContactsListViewModel MegaContacts => ContactsService.MegaContacts;

        private NodeViewModel _node;
        public NodeViewModel Node
        {
            get { return _node; }
            set { SetField(ref _node, value); }
        }

        #endregion

        #region UiResources

        public string AddContactText => ResourceService.UiResources.GetString("UI_AddContact");
        public string AllContactsText => ResourceService.UiResources.GetString("UI_AllContacts");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string ConfirmShareText => ResourceService.UiResources.GetString("UI_ConfirmShare");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string ShareToText => ResourceService.UiResources.GetString("UI_ShareTo");

        #endregion

        #region VisualResources
        
        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string ConfirmPathData => ResourceService.VisualResources.GetString("VR_ConfirmPathData");
        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");

        #endregion
    }
}
