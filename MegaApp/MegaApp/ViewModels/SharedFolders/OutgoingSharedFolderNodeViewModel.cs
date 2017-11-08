using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.SharedFolders
{
    public class OutgoingSharedFolderNodeViewModel : SharedFolderNodeViewModel
    {
        public OutgoingSharedFolderNodeViewModel(MNode megaNode, SharedFoldersListViewModel parent)
            : base(megaNode, parent)
        {
            this.RemoveSharedAccessCommand = new RelayCommand(RemoveSharedAccess);

            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_OutgoingSharedFolderPathData");
            this.Update(megaNode);
        }

        #region Commands

        public ICommand RemoveSharedAccessCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Update core data associated with the SDK MNode object
        /// </summary>
        /// <param name="megaNode">Node to update</param>
        /// <param name="externalUpdate">Indicates if is an update external to the app. For example from an `onNodesUpdate`</param>
        public override async void Update(MNode megaNode, bool externalUpdate = false)
        {
            base.Update(megaNode, externalUpdate);

            var outShares = SdkService.MegaSdk.getOutShares(megaNode);
            var outSharesSize = outShares.size();
            if (outSharesSize == 1)
            {
                var contact = SdkService.MegaSdk.getContact(outShares.get(0).getUser());
                var contactAttributeRequestListener = new GetUserAttributeRequestListenerAsync();
                var firstName = await contactAttributeRequestListener.ExecuteAsync(() =>
                    SdkService.MegaSdk.getUserAttribute(contact, (int)MUserAttrType.USER_ATTR_FIRSTNAME,
                    contactAttributeRequestListener));
                var lastName = await contactAttributeRequestListener.ExecuteAsync(() =>
                    SdkService.MegaSdk.getUserAttribute(contact, (int)MUserAttrType.USER_ATTR_LASTNAME,
                    contactAttributeRequestListener));

                OnUiThread(() =>
                {
                    this.ContactsText = (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName)) ?
                        contact.getEmail() : string.Format("{0} {1}", firstName, lastName);
                });
            }
            else
            {
                OnUiThread(() => this.ContactsText = string.Format(
                    ResourceService.UiResources.GetString("UI_NumberOfContacts"), outSharesSize));
            }
        }

        private async void RemoveSharedAccess()
        {
            if (this.Parent.ItemCollection.IsMultiSelectActive)
            {
                if (this.Parent.RemoveSharedAccessCommand.CanExecute(null))
                    this.Parent.RemoveSharedAccessCommand.Execute(null);
                return;
            }

            var dialogResult = await DialogService.ShowOkCancelAndWarningAsync(
                ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolder_Title"),
                string.Format(ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolderQuestion"), this.Name),
                ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolderWarning"),
                this.RemoveText, this.CancelText);

            if (!dialogResult) return;

            if(! await this.RemoveSharedAccessAsync())
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolder_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolderFailed"), this.Name));
                });
            }
        }

        #endregion

        #region Properties

        private string _contactsText;
        public string ContactsText
        {
            get { return _contactsText; }
            set { SetField(ref _contactsText, value); }
        }

        #endregion

        #region UiResources

        public string RemoveSharedAccessText => ResourceService.UiResources.GetString("UI_RemoveSharedAccess");

        #endregion
    }
}
