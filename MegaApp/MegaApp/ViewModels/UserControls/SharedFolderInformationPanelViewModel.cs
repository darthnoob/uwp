using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels.UserControls
{
    public class SharedFolderInformationPanelViewModel :  BaseViewModel
    {
        #region Properties

        private SharedFolderNodeViewModel _sharedFolder;
        public SharedFolderNodeViewModel SharedFolder
        {
            get { return _sharedFolder; }
            set
            {
                SetField(ref _sharedFolder, value);

                this.IsInShare = (this.SharedFolder?.OriginalMNode != null) ?
                    this.SharedFolder.OriginalMNode.isInShare() : false;
            }
        }

        public bool _isInShare;
        public bool IsInShare
        {
            get { return _isInShare; }
            set
            {
                SetField(ref _isInShare, value);
                OnPropertyChanged(nameof(this.CreatedLabelText),
                    nameof(this.ModifiedLabelText));
            }
        }

        #endregion

        #region UiResources

        public string ContentsLabelText => ResourceService.UiResources.GetString("UI_Contents");
        public string DetailsText => ResourceService.UiResources.GetString("UI_Details");
        public string FolderLocationLabelText => ResourceService.UiResources.GetString("UI_FolderLocation");
        public string FolderModifiedLabelText => ResourceService.UiResources.GetString("UI_FolderModified");
        public string InformationText => ResourceService.UiResources.GetString("UI_Information");
        public string LinkText => ResourceService.UiResources.GetString("UI_Link");
        public string OwnerLabelText => ResourceService.UiResources.GetString("UI_Owner");
        public string PermissionsLabelText => ResourceService.UiResources.GetString("UI_Permissions");
        public string SharedOnLabelText => ResourceService.UiResources.GetString("UI_SharedOn");
        public string SharedToLabelText => ResourceService.UiResources.GetString("UI_SharedTo");
        public string SizeLabelText => ResourceService.UiResources.GetString("UI_Size");

        public string CreatedLabelText => 
            (this.SharedFolder?.OriginalMNode != null && this.SharedFolder.OriginalMNode.isInShare()) ?
            ResourceService.UiResources.GetString("UI_SharedOn") :
            ResourceService.UiResources.GetString("UI_Created");

        public string ModifiedLabelText =>
            (this.SharedFolder?.OriginalMNode != null && this.SharedFolder.OriginalMNode.isInShare()) ?
            ResourceService.UiResources.GetString("UI_FolderModified") :
            ResourceService.UiResources.GetString("UI_Modified");

        #endregion
    }
}
