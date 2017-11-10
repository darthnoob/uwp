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
                    OnPropertyChanged(nameof(this.SectionNameText));

                    this.SharedFolder = (this._sharedFolderNode?.OriginalMNode?.isInShare() == true) ?
                        new FolderViewModel(ContainerType.InShares) : new FolderViewModel(ContainerType.OutShares);
                    this.SharedFolder.FolderRootNode = this.SharedFolderNode;

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

        #endregion

        #region UiResources

        public string SectionNameText => 
            this._sharedFolderNode?.OriginalMNode?.isInShare() == true ?
            ResourceService.UiResources.GetString("UI_IncomingShares") :
            ResourceService.UiResources.GetString("UI_OutgoingShares");

        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");

        #endregion

        #region VisualResources

        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");

        #endregion
    }
}
