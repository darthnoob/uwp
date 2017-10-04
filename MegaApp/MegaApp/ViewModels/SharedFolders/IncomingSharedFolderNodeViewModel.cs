using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels
{
    public class IncomingSharedFolderNodeViewModel : SharedFolderNodeViewModel
    {
        public IncomingSharedFolderNodeViewModel(MNode megaNode, SharedFoldersListViewModel parent)
            : base(megaNode)
        {
            this.Parent = parent;

            this.DownloadCommand = new RelayCommand(Download);
            this.LeaveSharedCommand = new RelayCommand(LeaveShared);

            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_IncomingSharedFolderPathData");
            this.Update();            
        }

        #region Commands

        public new ICommand DownloadCommand { get; }
        public ICommand LeaveSharedCommand { get; }

        #endregion

        #region Methods

        public new void Update(bool externalUpdate = false)
        {
            base.Update(externalUpdate);
        }

        private void Download()
        {
            if (this.Parent.ItemCollection.IsMultiSelectActive)
            {
                if (this.Parent.DownloadCommand.CanExecute(null))
                    this.Parent.DownloadCommand.Execute(null);
                return;
            }

            this.Download(TransferService.MegaTransfers);
        }

        private async void LeaveShared()
        {
            if (this.Parent.ItemCollection.IsMultiSelectActive)
            {
                if (this.Parent.LeaveSharedCommand.CanExecute(null))
                    this.Parent.LeaveSharedCommand.Execute(null);
                return;
            }

            var dialogResult = await DialogService.ShowOkCancelAndWarningAsync(
                ResourceService.AppMessages.GetString("AM_LeaveSharedFolder_Title"),
                string.Format(ResourceService.AppMessages.GetString("AM_LeaveSharedFolderQuestion"), this.Name),
                ResourceService.AppMessages.GetString("AM_LeaveSharedFolderWarning"),
                ResourceService.UiResources.GetString("UI_Leave"), this.CancelText);

            if (!dialogResult) return;

            if (!await this.RemoveAsync())
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_LeaveSharedFolder_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_LeaveSharedFolderFailed"), this.Name));
                });
            }
        }

        #endregion

        #region Properties        

        private SharedFoldersListViewModel _parent;
        public new SharedFoldersListViewModel Parent
        {
            get { return _parent; }
            set { SetField(ref _parent, value); }
        }

        public bool AllowRename => (this.AccessLevel == MShareType.ACCESS_FULL) && !this.Parent.ItemCollection.IsMultiSelectActive;

        #endregion

        #region UiResources

        public string LeaveSharedText => ResourceService.UiResources.GetString("UI_LeaveShared");

        #endregion
    }
}
