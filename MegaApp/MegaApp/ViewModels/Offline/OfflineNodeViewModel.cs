using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Database;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.Offline
{
    public abstract class OfflineNodeViewModel : BaseNodeViewModel, IOfflineNode
    {
        protected OfflineNodeViewModel(OfflineFolderViewModel parent,
            ObservableCollection<IBaseNode> parentCollection = null, ObservableCollection<IBaseNode> childCollection = null)
            : base(SdkService.MegaSdk)
        {
            this.Parent = parent;
            this.ParentCollection = parentCollection;
            this.ChildCollection = childCollection;

            this.RemoveFromOfflineCommand = new RelayCommand(RemoveFromOffline);
        }

        #region Commands

        public ICommand RemoveFromOfflineCommand { get; }

        #endregion

        #region IBaseNode Interface

        #region Methods

        public override void SetThumbnailImage()
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

        #endregion

        #endregion

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

        /// <summary>
        /// Remove the node from the offline section.
        /// </summary>
        /// <param name="isMultiSelect">Indicate if is removing multiple nodes.</param>
        /// <returns>NULL if the user cancel the action, TRUE if the node was successfully removed or FALSE in other case.</returns>
        public async Task<bool?> RemoveFromOfflineAsync(bool isMultiSelect = false)
        {
            bool? result = true;

            if (!isMultiSelect)
            {
                var userSelection = await DialogService.ShowOkCancelAsync(
                    ResourceService.AppMessages.GetString("AM_RemoveFromOfflineQuestion_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveFromOfflineQuestion"), this.Name));

                if (!userSelection) return null;
            }

            // Search if the node has pending transfers for offline and cancel them on this case                
            TransferService.CancelPendingNodeOfflineTransfers(this);

            var directoryInfo = new DirectoryInfo(this.NodePath).Parent;
            if (directoryInfo != null)
            {
                var parentFolderPath = directoryInfo.FullName;

                if (this.IsFolder)
                {
                    result &= OfflineService.RemoveFolderFromOfflineDB(this.NodePath);
                    result &= FolderService.DeleteFolder(this.NodePath, true);
                }
                else
                {
                    result &= SavedForOfflineDB.DeleteNodeByLocalPath(this.NodePath);
                    result &= FileService.DeleteFile(this.NodePath);
                }

                result &= OfflineService.CleanOfflineFolderNodePath(parentFolderPath);
            }

            result &= this.Parent?.ItemCollection?.Items?.Remove(this);

            return result;
        }

        public virtual void Open()
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Properties

        public OfflineNodeViewModel NodeBinding => this;

        private OfflineFolderViewModel _parent;
        public OfflineFolderViewModel Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != null)
                    _parent.PropertyChanged -= ParentOnPropertyChanged;

                SetField(ref _parent, value);

                if (_parent != null)
                    _parent.PropertyChanged += ParentOnPropertyChanged;
            }
        }

        #endregion

        #region Methods

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

        private async void RemoveFromOffline()
        {
            if (this.Parent != null && this.Parent.ItemCollection.MoreThanOneSelected)
            {
                if (this.Parent.RemoveFromOfflineCommand.CanExecute(null))
                    this.Parent.RemoveFromOfflineCommand.Execute(null);
                return;
            }

            var result = await this.RemoveFromOfflineAsync();
            if (result.HasValue && !result.Value)
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_RemoveFromOfflineFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_RemoveNodeFromOfflineFailed"), this.Name));
                });
            }
        }

        private void ParentOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(this.Parent.Folder)) return;
            OnPropertyChanged(nameof(this.Parent));
            OnPropertyChanged(nameof(this.NodeBinding));
        }

        #endregion

        #region UiResources

        public string RemoveFromOfflineText => ResourceService.UiResources.GetString("UI_RemoveFromOffline");

        #endregion
    }
}
