using mega;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels.UserControls
{
    public class SharedFolderContentPanelViewModel : BaseViewModel
    {
        #region Properties

        private SharedFolderNodeViewModel _sharedFolderNode;
        public SharedFolderNodeViewModel SharedFolderNode
        {
            get { return _sharedFolderNode; }
            set
            {
                if (!SetField(ref _sharedFolderNode, value)) return;

                if(_sharedFolderNode?.OriginalMNode != null)
                {
                    OnPropertyChanged(nameof(this.SectionNameText),
                        nameof(this.HasReadWritePermissions),
                        nameof(this.HasFullAccessPermissions));

                    var sharedFolder = (this._sharedFolderNode?.OriginalMNode?.isInShare() == true) ?
                        new FolderViewModel(ContainerType.InShares) : new FolderViewModel(ContainerType.OutShares);
                    sharedFolder.FolderRootNode = this.SharedFolderNode;

                    this.SharedFolder = sharedFolder;
                    this.SharedFolder.LoadChildNodes();
                }
            }
        }

        private FolderViewModel _sharedFolder;
        public FolderViewModel SharedFolder
        {
            get { return _sharedFolder; }
            set { SetField(ref _sharedFolder, value); }
        }

        public bool HasReadWritePermissions => this.SharedFolderNode?.AccessLevel == null ? false :
            (int)this.SharedFolderNode?.AccessLevel?.AccessType >= (int)MShareType.ACCESS_READWRITE;
            

        public bool HasFullAccessPermissions => this.SharedFolderNode?.AccessLevel == null ? false :
            (int)this.SharedFolderNode?.AccessLevel?.AccessType >= (int)MShareType.ACCESS_FULL;

        #endregion

        #region UiResources

        public string SectionNameText => 
            this._sharedFolderNode?.OriginalMNode?.isInShare() == true ?
            ResourceService.UiResources.GetString("UI_IncomingShares") :
            ResourceService.UiResources.GetString("UI_OutgoingShares");

        public string AddFolderText => ResourceService.UiResources.GetString("UI_NewFolder");
        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string RenameText => ResourceService.UiResources.GetString("UI_Rename");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");
        public string UploadText => ResourceService.UiResources.GetString("UI_Upload");

        #endregion

        #region VisualResources

        public string AddFolderPathData => ResourceService.VisualResources.GetString("VR_CreateFolderPathData");
        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");
        public string RenamePathData => ResourceService.VisualResources.GetString("VR_RenamePathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");
        public string UploadPathData => ResourceService.VisualResources.GetString("VR_UploadPathData");

        #endregion
    }
}
