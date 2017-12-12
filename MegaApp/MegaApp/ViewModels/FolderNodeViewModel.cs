using System.Collections.ObjectModel;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class FolderNodeViewModel: NodeViewModel
    {
        public FolderNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, FolderViewModel parent,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parent, parentCollection, childCollection)
        {
            SetFolderInfo();
            Transfer = new TransferObjectModel(this, MTransferType.TYPE_DOWNLOAD, LocalDownloadPath);

            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_FolderTypePath_default");

            if (!megaNode.getName().ToLower().Equals("camera uploads")) return;
            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_FolderTypePath_photo");
        }

        #region Public Methods

        public void SetFolderInfo()
        {
            this.ChildFolders = this.MegaSdk.getNumChildFolders(this.OriginalMNode);
            this.ChildFiles = this.MegaSdk.getNumChildFiles(this.OriginalMNode);

            string infoString = ResourceService.UiResources.GetString("UI_EmptyFolder");
            if (this.ChildFolders > 0 && this.ChildFiles > 0)
            {
                infoString = string.Format("{0} {1}, {2} {3}",
                    this.ChildFolders, this.ChildFolders == 1 ? this.SingleForderString : this.MultipleFordersString,
                    this.ChildFiles, this.ChildFiles == 1 ? this.SingleFileString : this.MultipleFilesString);
            }
            else if (this.ChildFolders > 0)
            {
                infoString = string.Format("{0} {1}", this.ChildFolders, 
                    this.ChildFolders == 1 ? this.SingleForderString : this.MultipleFordersString);
            }
            else if (this.ChildFiles > 0)
            {
                infoString = string.Format("{0} {1}", this.ChildFiles,
                    this.ChildFiles == 1 ? this.SingleFileString: this.MultipleFilesString);
            }

            OnUiThread(() => this.Information = infoString);
        }

        #endregion

        #region UiResources

        private string SingleForderString => ResourceService.UiResources.GetString("UI_SingleFolder").ToLower();
        private string MultipleFordersString => ResourceService.UiResources.GetString("UI_MultipleFolders").ToLower();
        private string SingleFileString => ResourceService.UiResources.GetString("UI_SingleFile").ToLower();
        private string MultipleFilesString => ResourceService.UiResources.GetString("UI_MultipleFiles").ToLower();

        #endregion
    }
}
