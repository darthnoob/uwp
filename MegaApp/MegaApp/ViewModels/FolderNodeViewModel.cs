using System;
using System.Collections.ObjectModel;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class FolderNodeViewModel: NodeViewModel
    {
        public FolderNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, ContainerType parentContainerType,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parentContainerType, parentCollection, childCollection)
        {
            SetFolderInfo();

            this.IsDefaultImage = true;
            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_FolderTypePath_default");
        }

        #region Public Methods

        public void SetFolderInfo()
        {
            int childFolders = this.MegaSdk.getNumChildFolders(this.OriginalMNode);
            int childFiles = this.MegaSdk.getNumChildFiles(this.OriginalMNode);

            OnUiThread(() =>
            {
                this.Information = String.Format("{0} {1} | {2} {3}",
                    childFolders, childFolders == 1 ? ResourceService.UiResources.GetString("UI_SingleFolder").ToLower() : ResourceService.UiResources.GetString("UI_MultipleFolders").ToLower(),
                    childFiles, childFiles == 1 ? ResourceService.UiResources.GetString("UI_SingleFile").ToLower() : ResourceService.UiResources.GetString("UI_MultipleFiles").ToLower());
            });
        }

        #endregion
    }
}
