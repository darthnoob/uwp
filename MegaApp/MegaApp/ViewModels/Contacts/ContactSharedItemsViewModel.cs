using mega;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactSharedItemsViewModel : IncomingSharesViewModel
    {
        public ContactSharedItemsViewModel(MUser contact)
        {
            this.Contact = contact;

            this.GetIncomingSharedItems();

            this.ItemCollection.ItemCollectionChanged += (sender, args) => OnItemCollectionChanged();
            this.ItemCollection.SelectedItemsCollectionChanged += (sender, args) => OnSelectedItemsCollectionChanged();

            this.ItemCollection.OrderInverted += (sender, args) => SortBy(this.CurrentOrder, this.ItemCollection.CurrentOrderDirection);

            if (App.GlobalListener == null) return;
            App.GlobalListener.InSharedFolderAdded += this.OnInSharedFolderAdded;
            App.GlobalListener.InSharedFolderRemoved += this.OnInSharedFolderRemoved;
        }

        #region Methods

        public void GetIncomingSharedItems() => base.GetIncomingSharedItems(this.Contact);

        public void OnInSharedFolderAdded(object sender, MNode megaNode)
        {
            var user = MegaSdk.getUserFromInShare(megaNode);

            if (user.getEmail().Equals(this.Contact.getEmail()))
                base.OnSharedFolderAdded(sender, megaNode);
        }

        public void OnInSharedFolderRemoved(object sender, MNode megaNode)
        {
            var user = MegaSdk.getUserFromInShare(megaNode);

            if (user.getEmail().Equals(this.Contact.getEmail()))
                base.OnSharedFolderRemoved(sender, megaNode);
        }

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
