using System;
using System.Collections.ObjectModel;
using System.IO;
using MegaApp.Database;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.Offline
{
    public class OfflineFolderNodeViewModel : OfflineNodeViewModel
    {
        public OfflineFolderNodeViewModel(DirectoryInfo folderInfo, OfflineFolderViewModel parent,
            ObservableCollection<IBaseNode> parentCollection = null, ObservableCollection<IBaseNode> childCollection = null)
            : base(parent, parentCollection, childCollection)
        {
            Update(folderInfo);
            SetFolderInfo();

            this.IsDefaultImage = true;
            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_FolderTypePath_default");
        }

        #region Properties

        private string _contents;
        public string Contents
        {
            get { return _contents; }
            set { SetField(ref _contents, value); }
        }

        #endregion

        #region Methods

        public void Update(DirectoryInfo folderInfo)
        {
            this.Base64Handle = "0";
            var existingNode = SavedForOfflineDB.SelectNodeByLocalPath(folderInfo.FullName);
            if (existingNode != null)
                this.Base64Handle = existingNode.Base64Handle;

            this.Name = folderInfo.Name;
            this.NodePath = folderInfo.FullName;
            this.Size = 0;
            this.IsFolder = true;
            this.CreationTime = folderInfo.CreationTime.DateToString();
            this.ModificationTime = folderInfo.LastWriteTime.DateToString();

            SetDefaultValues();
        }

        public void SetFolderInfo()
        {
            try
            {
                try { if (!Directory.Exists(this.NodePath)) Directory.CreateDirectory(this.NodePath); }
                catch (IOException) { /* DO NOTHING - Cannot create directory because already exists. */ }

                int childFolders = FolderService.GetNumChildFolders(this.NodePath);
                int childFiles = FolderService.GetNumChildFiles(this.NodePath, true);

                string infoString = ResourceService.UiResources.GetString("UI_EmptyFolder");
                if (childFolders > 0 && childFiles > 0)
                {
                    infoString = string.Format("{0} {1}, {2} {3}",
                        childFolders, childFolders == 1 ? this.SingleForderString : this.MultipleFordersString,
                        childFiles, childFiles == 1 ? this.SingleFileString : this.MultipleFilesString);
                }
                else if (childFolders > 0)
                {
                    infoString = string.Format("{0} {1}", childFolders,
                        childFolders == 1 ? this.SingleForderString : this.MultipleFordersString);
                }
                else if (childFiles > 0)
                {
                    infoString = string.Format("{0} {1}", childFiles,
                        childFiles == 1 ? this.SingleFileString : this.MultipleFilesString);
                }

                OnUiThread(() => this.Contents = infoString);
            }
            catch (Exception) { }
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
