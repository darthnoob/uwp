using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using MegaApp.Database;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.Offline
{
    public abstract class OfflineNodeViewModel : BaseNodeViewModel, IOfflineNode
    {
        protected OfflineNodeViewModel(ObservableCollection<IBaseNode> parentCollection = null,
            ObservableCollection<IBaseNode> childCollection = null) : base(SdkService.MegaSdk)
        {
            this.ParentCollection = parentCollection;
            this.ChildCollection = childCollection;
        }

        #region IOfflineNode Interface

        #region Properties

        private string _nodePath;
        public string NodePath
        {
            get { return _nodePath; }
            set { SetField(ref _nodePath, value); }
        }

        #endregion

        #region Methods

        public async Task<NodeActionResult> RemoveAsync(bool isMultiRemove)
        {
            if (!isMultiRemove)
            {
                var result = await DialogService.ShowOkCancelAsync(
                    ResourceService.AppMessages.GetString("AM_RemoveItemQuestion_Title"),
                    ResourceService.AppMessages.GetString("AM_RemoveItemQuestion"));

                if (!result) return NodeActionResult.Cancelled;
            }

            await RemoveForOffline();

            return NodeActionResult.IsBusy;
        }

        public void SetThumbnailImage()
        {
            if (this.IsFolder) return;

            if (this.ThumbnailImageUri != null && !IsDefaultImage) return;

            if (this.IsImage)
            {
                if (FileService.FileExists(ThumbnailPath))
                {
                    this.IsDefaultImage = false;
                    this.ThumbnailImageUri = new Uri(ThumbnailPath);
                }
                else
                {
                    this.IsDefaultImage = true;
                    this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(this.Name);
                }
            }
        }

        public virtual void Open()
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Methods

        public async Task RemoveForOffline()
        {
            string parentNodePath = ((new DirectoryInfo(this.NodePath)).Parent).FullName;

            string sfoRootPath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                ResourceService.AppResources.GetString("AR_OfflineDirectory").Replace("\\", ""));

            if (this.IsFolder)
            {
                await RecursiveRemoveForOffline(parentNodePath, this.Name);
                FolderService.DeleteFolder(this.NodePath, true);
            }
            else
            {
                // Search if the file has a pending transfer for offline and cancel it on this case                
                TransferService.CancelPendingNodeOfflineTransfers(this.NodePath, this.IsFolder);

                FileService.DeleteFile(this.NodePath);
            }

            SavedForOfflineDB.DeleteNodeByLocalPath(this.NodePath);

            if (this.ParentCollection != null)
                this.ParentCollection.Remove((IMegaNode)this);
        }

        protected void SetDefaultValues()
        {
            this.IsMultiSelected = false;
            this.DisplayMode = NodeDisplayMode.Normal;

            if (this.IsFolder) return;

            var existingNode = SavedForOfflineDB.SelectNodeByLocalPath(this.NodePath);
            if (existingNode != null)
            {
                this.Base64Handle = existingNode.Base64Handle;

                if (FileService.FileExists(ThumbnailPath))
                {
                    this.IsDefaultImage = false;
                    this.ThumbnailImageUri = new Uri(ThumbnailPath);
                }
                else
                {
                    this.IsDefaultImage = true;
                    this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(this.Name);
                }
            }
        }

        private async Task RecursiveRemoveForOffline(string sfoPath, string nodeName)
        {
            string newSfoPath = Path.Combine(sfoPath, nodeName);

            if (FolderService.FolderExists(newSfoPath))
            {
                // Search if the folder has a pending transfer for offline and cancel it on this case            
                TransferService.CancelPendingNodeOfflineTransfers(string.Concat(newSfoPath, "\\"), this.IsFolder);

                IEnumerable<string> childFolders = Directory.GetDirectories(newSfoPath);
                if (childFolders != null)
                {
                    foreach (var folder in childFolders)
                    {
                        if (folder != null)
                        {
                            await RecursiveRemoveForOffline(newSfoPath, folder);
                            SavedForOfflineDB.DeleteNodeByLocalPath(Path.Combine(newSfoPath, folder));
                        }
                    }
                }

                IEnumerable<string> childFiles = Directory.GetFiles(newSfoPath);
                if (childFiles != null)
                {
                    foreach (var file in childFiles)
                    {
                        if (file != null)
                            SavedForOfflineDB.DeleteNodeByLocalPath(Path.Combine(newSfoPath, file));
                    }
                }
            }
        }

        #endregion
    }
}
