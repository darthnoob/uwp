using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    /// <summary>
    /// ViewModel of the main MEGA datatype (MNode)
    /// </summary>
    public abstract class NodeViewModel : BaseSdkViewModel, IMegaNode
    {
        // Offset DateTime value to calculate the correct creation and modification time
        private static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        protected NodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, ContainerType parentContainerType,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
        {
            Update(megaNode, parentContainerType);
            SetDefaultValues();

            this.ParentCollection = parentCollection;
            this.ChildCollection = childCollection;
        }

        #region Private Methods

        private void SetDefaultValues()
        {
            this.IsMultiSelected = false;
            this.DisplayMode = NodeDisplayMode.Normal;

            if (this.Type == MNodeType.TYPE_FOLDER) return;

            if (FileService.FileExists(this.ThumbnailPath))
            {
                this.IsDefaultImage = false;
                this.ThumbnailImageUri = new Uri(this.ThumbnailPath);
            }
            else
            {
                this.IsDefaultImage = true;
                this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(this.Name);
            }
        }

        /// <summary>
        /// Get the thumbnail of the node
        /// </summary>
        private async void GetThumbnail()
        {
            if (FileService.FileExists(this.ThumbnailPath))
            {
                this.IsDefaultImage = false;
                this.ThumbnailImageUri = new Uri(this.ThumbnailPath);
            }
            else if (Convert.ToBoolean(this.MegaSdk.isLoggedIn()) || this.ParentContainerType == ContainerType.FolderLink)
            {
                var getThumbnail = new GetThumbnailRequestListenerAsync();
                var result = await getThumbnail.ExecuteAsync(() =>
                {
                    this.MegaSdk.getThumbnail(this.OriginalMNode, 
                        this.ThumbnailPath, getThumbnail);
                });
                
                if(result)
                {
                    await UiService.OnUiThread(() =>
                    {
                        this.IsDefaultImage = false;
                        this.ThumbnailImageUri = new Uri(this.ThumbnailPath);
                    });
                }
            }
        }

        /// <summary>
        /// Convert the MEGA time to a C# DateTime object in local time
        /// </summary>
        /// <param name="time">MEGA time</param>
        /// <returns>DateTime object in local time</returns>
        private static DateTime ConvertDateToString(ulong time)
        {
            return OriginalDateTime.AddSeconds(time).ToLocalTime();
        }

        #endregion

        #region IBaseNode Interface

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        public string CreationTime { get; private set; }

        public string ModificationTime { get; private set; }

        public string ThumbnailPath
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    ResourceService.AppResources.GetString("AR_ThumbnailsDirectory"), this.OriginalMNode.getBase64Handle());
            }
        }

        private string _information;
        public string Information
        {
            get { return _information; }
            set { SetField(ref _information, value); }
        }

        public string Base64Handle { get; set; }

        public ulong Size { get; set; }

        private string _sizeText;
        public string SizeText
        {
            get { return _sizeText; }
            set { SetField(ref _sizeText, value); }
        }

        private bool _isMultiSelected;
        public bool IsMultiSelected
        {
            get { return _isMultiSelected; }
            set { SetField(ref _isMultiSelected, value); }
        }

        public bool IsFolder
        {
            get { return this.Type == MNodeType.TYPE_FOLDER ? true : false; }
        }

        public bool IsImage => ImageService.IsImage(this.Name);

        private bool _IsDefaultImage;
        public bool IsDefaultImage
        {
            get { return _IsDefaultImage; }
            set { SetField(ref _IsDefaultImage, value); }
        }

        private Uri _thumbnailImageUri;
        public Uri ThumbnailImageUri
        {
            get { return _thumbnailImageUri; }
            set { SetField(ref _thumbnailImageUri, value); }
        }

        private string _defaultImagePathData;
        public string DefaultImagePathData
        {
            get { return _defaultImagePathData; }
            set { SetField(ref _defaultImagePathData, value); }
        }

        #endregion

        #region IMegaNode Interface

        public async Task RenameAsync()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            var oldName = this.Name;

            var inputName = await DialogService.ShowInputDialogAsync(
                ResourceService.UiResources.GetString("UI_Rename"),
                ResourceService.UiResources.GetString("UI_TypeNewName"),
                new InputDialogSettings
                {
                    InputText = this.Name,
                    IsTextSelected = true,
                    IgnoreExtensionInSelection = true
                });
            
            if(string.IsNullOrEmpty(inputName) || string.IsNullOrWhiteSpace(inputName)) return;
            if (this.Name.Equals(inputName)) return;

            var rename = new RenameNodeRequestListenerAsync();
            var newName = await rename.ExecuteAsync(() =>
            {
                this.MegaSdk.renameNode(this.OriginalMNode, inputName, rename);
            });

            if (string.IsNullOrEmpty(newName))
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_RenameNodeFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_RenameNodeFailed"));
                return;
            };
            
            await OnUiThread(() => this.Name = newName);
        }

        /// <summary>
        /// Move the node from its current location to a new folder destination
        /// </summary>
        /// <param name="newParentNode">The root node of the destination folder</param>
        /// <returns>Result of the action</returns>
        public async Task<NodeActionResult> MoveAsync(IMegaNode newParentNode)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            if (MegaSdk.checkMove(OriginalMNode, newParentNode.OriginalMNode).getErrorCode() != MErrorType.API_OK)
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_MoveFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_MoveFailed"));

                return NodeActionResult.Failed;
            }

            var moveNode = new MoveNodeRequestListenerAsync();
            var result = await moveNode.ExecuteAsync(() =>
                MegaSdk.moveNode(OriginalMNode, newParentNode.OriginalMNode, moveNode));

            if (!result) return NodeActionResult.Failed;
            
            return NodeActionResult.Succeeded;
        }

        /// <summary>
        /// Copy the node from its current location to a new folder destination
        /// </summary>
        /// <param name="newParentNode">The root node of the destination folder</param>
        /// <returns>Result of the action</returns>
        public async Task<NodeActionResult> CopyAsync(IMegaNode newParentNode)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            var copyNode = new CopyNodeRequestListenerAsync();
            var result = await copyNode.ExecuteAsync(() =>
                MegaSdk.copyNode(OriginalMNode, newParentNode.OriginalMNode, copyNode));

            if (!result) return NodeActionResult.Failed;
            
            return NodeActionResult.Succeeded;
        }

        public async Task<NodeActionResult> MoveToRubbishBinAsync(bool isMultiSelect = false)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            if (this.OriginalMNode == null) return NodeActionResult.Failed;

            if (!isMultiSelect)
            {
                var dialogResult = await DialogService.ShowOkCancelAsync(
                    ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion"), this.Name));

                if (!dialogResult) return NodeActionResult.Cancelled;
            }

            var moveNode = new MoveNodeRequestListenerAsync();
            var result = await moveNode.ExecuteAsync(() =>
            {
                this.MegaSdk.moveNode(this.OriginalMNode, this.MegaSdk.getRubbishNode(), moveNode);
            });

            if (!result) return NodeActionResult.Failed;

            return NodeActionResult.Succeeded;
        }

        public async Task<NodeActionResult> RemoveAsync(bool isMultiSelect = false)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            if (this.OriginalMNode == null) return NodeActionResult.Failed;

            if (!isMultiSelect)
            {
                var dialogResult = await DialogService.ShowOkCancelAsync(
                    ResourceService.AppMessages.GetString("AM_RemoveItemQuestion_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveItemQuestion"), this.Name));

                if (!dialogResult) return NodeActionResult.Cancelled;
            }

            var removeNode = new RemoveNodeRequestListenerAsync();
            var result = await removeNode.ExecuteAsync(() =>
            {
                this.MegaSdk.remove(this.OriginalMNode, removeNode);
            });

            if (!result) return NodeActionResult.Failed;

            return NodeActionResult.Succeeded;
        }

        public NodeActionResult GetLink()
        {
            return NodeActionResult.IsBusy;
        }

        public async void Download(TransferQueue transferQueue)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;
            if (transferQueue == null) return;

            var downloadFolder = await FolderService.SelectFolder();
            if (downloadFolder == null) return;
            if (!await TransferService.CheckExternalDownloadPathAsync(downloadFolder.Path)) return;

            this.Transfer.ExternalDownloadPath = downloadFolder.Path;
            transferQueue.Add(this.Transfer);
            this.Transfer.StartTransfer();
        }

        public void Update(MNode megaNode, ContainerType parentContainerType)
        {
            this.OriginalMNode = megaNode;
            this.Handle = megaNode.getHandle();
            this.Base64Handle = megaNode.getBase64Handle();
            this.Type = megaNode.getType();
            this.ParentContainerType = parentContainerType;
            this.Name = megaNode.getName();
            this.Size = this.MegaSdk.getSize(megaNode);
            this.SizeText = this.Size.ToStringAndSuffix();
            this.IsExported = megaNode.isExported();
            this.CreationTime = ConvertDateToString(megaNode.getCreationTime()).ToString("dd MMM yyyy");

            if (this.Type == MNodeType.TYPE_FILE)
                this.ModificationTime = ConvertDateToString(megaNode.getModificationTime()).ToString("dd MMM yyyy");
            else
                this.ModificationTime = this.CreationTime;

            //if (!App.MegaSdk.isInShare(megaNode) && ParentContainerType != ContainerType.PublicLink &&
            //    ParentContainerType != ContainerType.InShares && ParentContainerType != ContainerType.ContactInShares &&
            //    ParentContainerType != ContainerType.FolderLink)
            //    CheckAndUpdateSFO(megaNode);
            this.IsAvailableOffline = false;
            this.IsSelectedForOffline = false;
        }

        public void SetThumbnailImage()
        {
            if (this.Type == MNodeType.TYPE_FOLDER) return;

            if (this.ThumbnailImageUri != null && !this.IsDefaultImage) return;

            if (this.IsImage || this.OriginalMNode.hasThumbnail())
            {
                GetThumbnail();
            }
        }

        public virtual void Open()
        {
            throw new NotImplementedException();
        }

        public ulong Handle { get; set; }

        public ObservableCollection<IMegaNode> ParentCollection { get; set; }

        public ObservableCollection<IMegaNode> ChildCollection { get; set; }

        public MNodeType Type { get; private set; }

        public ContainerType ParentContainerType { get; private set; }

        private NodeDisplayMode _displayMode;
        public NodeDisplayMode DisplayMode
        {
            get { return _displayMode; }
            set { SetField(ref _displayMode, value); }
        }

        private bool _isSelectedForOffline;
        public bool IsSelectedForOffline
        {
            get { return _isSelectedForOffline; }
            set
            {
                SetField(ref _isSelectedForOffline, value);
                this.IsSelectedForOfflineText = _isSelectedForOffline ? 
                    ResourceService.UiResources.GetString("UI_On") : ResourceService.UiResources.GetString("UI_Off");
            }
        }

        private string _isSelectedForOfflineText;
        public string IsSelectedForOfflineText
        {
            get { return _isSelectedForOfflineText; }
            set { SetField(ref _isSelectedForOfflineText, value); }
        }

        private bool _isAvailableOffline;
        public bool IsAvailableOffline
        {
            get { return _isAvailableOffline; }
            set { SetField(ref _isAvailableOffline, value); }
        }

        private bool _isExported;
        public bool IsExported
        {
            get { return _isExported; }
            set { SetField(ref _isExported, value); }
        }

        public TransferObjectModel Transfer { get; set; }

        public MNode OriginalMNode { get; private set; }

        #endregion

        #region Properties

        public string LocalDownloadPath => Path.Combine(ApplicationData.Current.LocalFolder.Path,
            ResourceService.AppResources.GetString("AR_DownloadsDirectory"), this.Name);

        #endregion
    }
}
