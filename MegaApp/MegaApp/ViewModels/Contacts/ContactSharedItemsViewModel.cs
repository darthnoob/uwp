using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactSharedItemsViewModel : SharedItemsViewModel
    {
        public ContactSharedItemsViewModel(MUser contact)
        {
            this.Contact = contact;

            this.DownloadCommand = new RelayCommand(Download);
            this.LeaveSharedCommand = new RelayCommand(LeaveShared);

            this.GetIncomingSharedItems();
        }

        #region Commands

        public ICommand DownloadCommand { get; }
        public ICommand LeaveSharedCommand { get; }

        #endregion

        #region Methods

        public void GetIncomingSharedItems() => base.GetIncomingSharedItems(this.Contact);

        private void Download()
        {
            this.ItemCollection.FocusedItem.Download(TransferService.MegaTransfers);
        }

        private void LeaveShared()
        {
            this.ItemCollection.FocusedItem.RemoveAsync();
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

        #region UiResources

        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string LeaveSharedText => ResourceService.UiResources.GetString("UI_LeaveShared");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");
        
        #endregion

        #region VisualResources

        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string LeaveSharedPathData => ResourceService.VisualResources.GetString("VR_LeaveSharedPathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");
        
        #endregion
    }
}
