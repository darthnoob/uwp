using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels
{
    public class SharedFoldersViewModel : BaseSdkViewModel
    {
        public SharedFoldersViewModel()
        {
            this.IncomingShares = new IncomingSharesViewModel();
            this.OutgoingShares = new OutgoingSharesViewModel();
        }

        #region Methods

        public void Initialize()
        {
            this.IncomingShares.Initialize();
            this.OutgoingShares.Initialize();
        }

        public void Deinitialize()
        {
            this.IncomingShares.Deinitialize();
            this.OutgoingShares.Deinitialize();
        }

        #endregion

        #region Properties

        private SharedFoldersListViewModel _activeView;
        public SharedFoldersListViewModel ActiveView
        {
            get { return _activeView; }
            set { SetField(ref _activeView, value); }
        }

        private IncomingSharesViewModel _incomingShares;
        public IncomingSharesViewModel IncomingShares
        {
            get { return _incomingShares; }
            set { SetField(ref _incomingShares, value); }
        }

        private OutgoingSharesViewModel _outgoingShares;
        public OutgoingSharesViewModel OutgoingShares
        {
            get { return _outgoingShares; }
            set { SetField(ref _outgoingShares, value); }
        }

        #endregion

        #region UiResources

        public string IncomingSharesTitle => ResourceService.UiResources.GetString("UI_IncomingShares");
        public string OutgoingSharesTitle => ResourceService.UiResources.GetString("UI_OutgoingShares");

        #endregion
    }
}
