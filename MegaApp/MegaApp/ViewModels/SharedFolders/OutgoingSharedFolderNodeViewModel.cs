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

        public new void Update(bool externalUpdate = false)
        {
            base.Update(externalUpdate);

            var outShares = SdkService.MegaSdk.getOutShares(this.OriginalMNode);
            var outSharesSize = outShares.size();
            OnUiThread(() =>
            {
                this.ContactsText = outSharesSize == 1 ? outShares.get(0).getUser() : 
                    string.Format("{0} Contacts", outSharesSize);
            });
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
