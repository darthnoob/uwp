using mega;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactSharedItemsViewModel : IncomingSharesViewModel
    {
        public ContactSharedItemsViewModel(MUser contact)
        {
            this.Contact = contact;

            this.Initialize(this.Contact);
        }

        #region Methods

        public void GetIncomingSharedItems() => base.GetIncomingSharedItems(this.Contact);

        #endregion

        #region Properties

        private MUser _contact;
        public MUser Contact
        {
            get { return _contact; }
            set { SetField(ref _contact, value); }
        }

        #endregion        
    }
}
