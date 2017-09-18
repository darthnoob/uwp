using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class IncomingSharedFolderNodeViewModel : FolderNodeViewModel
    {
        public IncomingSharedFolderNodeViewModel(MNode megaNode, SharedItemsViewModel parent)
            : base(SdkService.MegaSdk, App.AppInformation, megaNode, null)
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

        public void Update(bool externalUpdate = false)
        {
            this.AccessLevel = (MShareType)SdkService.MegaSdk.getAccess(this.OriginalMNode);
            base.Update(this.OriginalMNode, externalUpdate);
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

        private void LeaveShared()
        {
            if (this.Parent.ItemCollection.IsMultiSelectActive)
            {
                if (this.Parent.LeaveSharedCommand.CanExecute(null))
                    this.Parent.LeaveSharedCommand.Execute(null);
                return;
            }

            this.RemoveAsync();
        }

        #endregion

        #region Properties

        private MShareType _accessLevel;
        public MShareType AccessLevel
        {
            get { return _accessLevel; }
            set
            {
                SetField(ref _accessLevel, value);
                OnPropertyChanged(nameof(this.AccessLevelText),
                    nameof(this.AccessLevelPathData));
            }
        }

        public string AccessLevelText
        {
            get
            {
                switch(this.AccessLevel)
                {
                    case MShareType.ACCESS_READ:
                        return ResourceService.UiResources.GetString("UI_PermissionReadOnly");
                    case MShareType.ACCESS_READWRITE:
                        return ResourceService.UiResources.GetString("UI_PermissionReadAndWrite");
                    case MShareType.ACCESS_FULL:
                        return ResourceService.UiResources.GetString("UI_PermissionFullAccess");
                    case MShareType.ACCESS_UNKNOWN:
                    case MShareType.ACCESS_OWNER:
                    default:
                        return string.Empty;
                }
            }
        }

        public string AccessLevelPathData
        {
            get
            {
                switch (this.AccessLevel)
                {
                    case MShareType.ACCESS_READ:
                        return ResourceService.VisualResources.GetString("VR_PermissionsReadOnlyPathData");
                    case MShareType.ACCESS_READWRITE:
                        return ResourceService.VisualResources.GetString("VR_PermissionsReadAndWritePathData");
                    case MShareType.ACCESS_FULL:
                        return ResourceService.VisualResources.GetString("VR_PermissionsFullAccessPathData");
                    case MShareType.ACCESS_UNKNOWN:
                    case MShareType.ACCESS_OWNER:
                    default:
                        return string.Empty;
                }
            }
        }

        private SharedItemsViewModel _parent;
        public new SharedItemsViewModel Parent
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
