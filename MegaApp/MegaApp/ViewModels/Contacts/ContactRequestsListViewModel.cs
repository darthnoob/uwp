using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactRequestsListViewModel : BaseSdkViewModel
    {
        /// <summary>
        /// View model to manage a contact requests list
        /// </summary>
        /// <param name="isOutgoing">Indicate the contact request list is for outgoing requests or not</param>
        public ContactRequestsListViewModel(bool isOutgoing)
        {
            this.isOutgoing = isOutgoing;
            this.MegaContactRequestsList = new CollectionViewModel<IMegaContactRequest>();
        }

        #region Methods

        public void Initialize(GlobalListener globalListener)
        {
            this.GetMegaContactRequests();

            if (globalListener == null) return;
            if (isOutgoing)
                globalListener.OutgoingContactRequestUpdated += (sender, args) => this.GetMegaContactRequests();
            else
                globalListener.IncomingContactRequestUpdated += (sender, args) => this.GetMegaContactRequests();
        }

        public void Deinitialize(GlobalListener globalListener)
        {
            if (globalListener == null) return;
            if (isOutgoing)
                globalListener.OutgoingContactRequestUpdated -= (sender, args) => this.GetMegaContactRequests();
            else
                globalListener.IncomingContactRequestUpdated -= (sender, args) => this.GetMegaContactRequests();
        }

        public async void GetMegaContactRequests()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            await OnUiThreadAsync(() => this.MegaContactRequestsList.Clear());

            var contactRequestsList = isOutgoing ?
                SdkService.MegaSdk.getOutgoingContactRequests() : 
                SdkService.MegaSdk.getIncomingContactRequests();

            var contactRequestsListSize = contactRequestsList.size();

            for (int i = 0; i < contactRequestsListSize; i++)
            {
                // To avoid null values
                if (contactRequestsList.get(i) == null) continue;

                var contactRequest = new ContactRequestViewModel(contactRequestsList.get(i));
                OnUiThread(() => this.MegaContactRequestsList.Items.Add(contactRequest));
            }
        }

        #endregion

        #region Properties

        private bool isOutgoing { get; set; }

        private CollectionViewModel<IMegaContactRequest> _megaContactRequestsList;
        public CollectionViewModel<IMegaContactRequest> MegaContactRequestsList
        {
            get { return _megaContactRequestsList; }
            set { SetField(ref _megaContactRequestsList, value); }
        }

        #endregion
    }
}
