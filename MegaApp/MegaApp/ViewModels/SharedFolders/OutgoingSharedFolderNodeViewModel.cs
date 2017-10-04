using mega;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels
{
    public class OutgoingSharedFolderNodeViewModel : SharedFolderNodeViewModel
    {
        public OutgoingSharedFolderNodeViewModel(MNode megaNode, SharedFoldersListViewModel parent)
            : base(megaNode)
        {
            this.Parent = parent;

            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_OutgoingSharedFolderPathData");
            this.Update();
        }

        #region Methods

        public async new void Update(bool externalUpdate = false)
        {
            base.Update(externalUpdate);

            var outShares = SdkService.MegaSdk.getOutShares(this.OriginalMNode);
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

        #endregion

        #region Properties

        private string _contactsText;
        public string ContactsText
        {
            get { return _contactsText; }
            set { SetField(ref _contactsText, value); }
        }

        private SharedFoldersListViewModel _parent;
        public new SharedFoldersListViewModel Parent
        {
            get { return _parent; }
            set { SetField(ref _parent, value); }
        }

        #endregion
    }
}
