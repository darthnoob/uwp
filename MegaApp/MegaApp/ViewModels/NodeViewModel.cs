using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels
{
    /// <summary>
    /// ViewModel of the main MEGA datatype (MNode)
    /// </summary>
    public abstract class NodeViewModel : BaseSdkViewModel, IMegaNode
    {
        // Offset DateTime value to calculate the correct creation and modification time
        private static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        protected NodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, FolderViewModel parent,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
        {
            this.AccessLevel = new AccessLevelViewModel();

            Update(megaNode);
            SetDefaultValues();

            this.Parent = parent;
            this.ParentCollection = parentCollection;
            this.ChildCollection = childCollection;

            this.CopyOrMoveCommand = new RelayCommand(CopyOrMove);
            this.DownloadCommand = new RelayCommand(Download);
            this.GetLinkCommand = new RelayCommand<bool>(GetLinkAsync);
            this.PreviewCommand = new RelayCommand(Preview);
            this.RemoveCommand = new RelayCommand(Remove);
            this.RenameCommand = new RelayCommand(Rename);
            this.OpenInformationPanelCommand = new RelayCommand(OpenInformationPanel);
        }

        private void ParentOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Parent.Folder))
            {
                OnPropertyChanged(nameof(this.Parent));
                OnPropertyChanged(nameof(this.NodeBinding));
            }
        }

        #region Commands

        public ICommand CopyOrMoveCommand { get; }
        public ICommand DownloadCommand { get; set; }
        public ICommand GetLinkCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand RenameCommand { get; }
        public virtual ICommand OpenInformationPanelCommand { get; }

        #endregion

        #region Private Methods

        private void SetDefaultValues()
        {
            this.IsMultiSelected = false;
            this.DisplayMode = NodeDisplayMode.Normal;

            if (this.Type == MNodeType.TYPE_FOLDER) return;

            if (FileService.FileExists(this.ThumbnailPath))
                this.ThumbnailImageUri = new Uri(this.ThumbnailPath);
            else
                this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(this.Name);
        }

        /// <summary>
        /// Get the thumbnail of the node
        /// </summary>
        private async void GetThumbnail()
        {
            if (FileService.FileExists(this.ThumbnailPath))
            {
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
                    UiService.OnUiThread(() => this.ThumbnailImageUri = new Uri(this.ThumbnailPath));
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

        public NodeViewModel NodeBinding => this;

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                SetField(ref _name, value);
                if(!this.IsFolder)
                    this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(this.Name);
            }
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

        private int _childFiles;
        public int ChildFiles
        {
            get { return _childFiles; }
            set { SetField(ref _childFiles, value); }
        }

        private int _childFolders;
        public int ChildFolders
        {
            get { return _childFolders; }
            set { SetField(ref _childFolders, value); }
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

        private Uri _thumbnailImageUri;
        public Uri ThumbnailImageUri
        {
            get { return _thumbnailImageUri; }
            set { SetField(ref _thumbnailImageUri, value); }
        }

        public virtual Uri PreviewImageUri
        {
            get
            {
                return ((this is ImageNodeViewModel) && (this as ImageNodeViewModel) != null) ?
                    (this as ImageNodeViewModel).PreviewImageUri : null;
            }

            set
            {
                if((this is ImageNodeViewModel) && (this as ImageNodeViewModel != null))
                    (this as ImageNodeViewModel).PreviewImageUri = value;
            }
        }

        private string _defaultImagePathData;
        public string DefaultImagePathData
        {
            get { return _defaultImagePathData; }
            set { SetField(ref _defaultImagePathData, value); }
        }

        #endregion

        #region IMegaNode Interface

        public async void Rename()
        {
            await RenameAsync();
        }

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
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_RenameNodeFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_RenameNodeFailed"));
                });
                return;
            };

            OnUiThread(() => this.Name = newName);
        }

        private void CopyOrMove()
        {
            // In case that copy or move a single node using the flyout menu and the selected 
            // nodes list is empty, we need add the current node to the selected nodes
            if (this.Parent != null && !this.Parent.ItemCollection.IsMultiSelectActive)
            {
                if (!this.Parent.ItemCollection.HasSelectedItems)
                    this.Parent.ItemCollection.SelectedItems.Add(this);
            }

            if (this.Parent != null && this.Parent.CopyOrMoveCommand.CanExecute(null))
                this.Parent.CopyOrMoveCommand.Execute(null);
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

        private void Preview()
        {
            this.Parent.FocusedNode = this;

            // Navigate to the preview page
            OnUiThread(() =>
            {
                var parameters = new Dictionary<NavigationParamType, object>();
                parameters.Add(NavigationParamType.Data, this.Parent);

                NavigateService.Instance.Navigate(typeof(PreviewImagePage), true,
                    NavigationObject.Create(this.GetType(), NavigationActionType.Default, parameters));
            });
        }

        private void OpenInformationPanel()
        {
            if(Parent != null)
            {
                if ((this is ImageNodeViewModel) && (this as ImageNodeViewModel != null))
                    (this as ImageNodeViewModel).InViewingRange = true;

                this.Parent.FocusedNode = this;

                if (this.Parent.OpenInformationPanelCommand.CanExecute(null))
                    this.Parent.OpenInformationPanelCommand.Execute(null);
            }
        }

        private async void Remove()
        {
            if (this.Parent != null && this.Parent.ItemCollection.MoreThanOneSelected)
            {
                if (this.Parent.RemoveCommand.CanExecute(null))
                    this.Parent.RemoveCommand.Execute(null);
                return;
            }
            await RemoveAsync();
        }

        public async Task<bool> RemoveAsync(bool isMultiSelect = false)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return false;

            if (this.OriginalMNode == null) return false;

            if (!isMultiSelect && !(this is IncomingSharedFolderNodeViewModel))
            {
                string title, message;
                switch (this.Parent?.Type)
                {
                    case ContainerType.CloudDrive:
                    case ContainerType.CameraUploads:
                    case ContainerType.ContactInShares:
                    case ContainerType.InShares:
                    case ContainerType.OutShares:
                        title = ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion_Title");
                        message = string.Format(ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion"), this.Name);
                        break;

                    case ContainerType.RubbishBin:
                        title = ResourceService.AppMessages.GetString("AM_RemoveItemQuestion_Title");
                        message = string.Format(ResourceService.AppMessages.GetString("AM_RemoveItemQuestion"), this.Name);
                        break;

                    default:
                        return false;
                }

                var dialogResult = await DialogService.ShowOkCancelAsync(title, message);

                if (!dialogResult) return true;
            }

            bool result;
            if(this is IncomingSharedFolderNodeViewModel)
            {
                var leaveShare = new RemoveNodeRequestListenerAsync();
                result = await leaveShare.ExecuteAsync(() =>
                    this.MegaSdk.remove(this.OriginalMNode, leaveShare));
            }
            else
            {
                switch (this.Parent?.Type)
                {
                    case ContainerType.CloudDrive:
                    case ContainerType.CameraUploads:
                    case ContainerType.ContactInShares:
                    case ContainerType.InShares:
                    case ContainerType.OutShares:
                        var moveNode = new MoveNodeRequestListenerAsync();
                        result = await moveNode.ExecuteAsync(() =>
                            this.MegaSdk.moveNode(this.OriginalMNode, this.MegaSdk.getRubbishNode(), moveNode));
                        break;

                    case ContainerType.RubbishBin:
                        var removeNode = new RemoveNodeRequestListenerAsync();
                        result = await removeNode.ExecuteAsync(() =>
                            this.MegaSdk.remove(this.OriginalMNode, removeNode));
                        break;

                    default:
                        return false;
                }

                if (result)
                    this.Parent.ClosePanels();
            }

            return result;
        }

        public async void GetLinkAsync(bool showLinkDialog = true)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            if (this.OriginalMNode.isExported())
            {
                this.ExportLink = this.OriginalMNode.getPublicLink(true);
            }
            else
            {
                var exportNode = new ExporNodeRequestListenerAsync();
                this.ExportLink = await exportNode.ExecuteAsync(() =>
                {
                    this.MegaSdk.exportNode(this.OriginalMNode, exportNode);
                });

                if (string.IsNullOrEmpty(this.ExportLink))
                {
                    OnUiThread(async () =>
                    {
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_GetLinkFailed_Title"),
                            ResourceService.AppMessages.GetString("AM_GetLinkFailed"));
                    });
                    return;
                };
            }

            this.IsExported = true;

            if (showLinkDialog)
                OnUiThread(() => DialogService.ShowShareLink(this));
        }

        public async void SetLinkExpirationTime(long expireTime)
        {
            // User must be online to perform this operation
            if (!IsUserOnline() || expireTime < 0) return;

            var exportNode = new ExporNodeRequestListenerAsync();
            this.ExportLink = await exportNode.ExecuteAsync(() =>
            {
                this.MegaSdk.exportNodeWithExpireTime(this.OriginalMNode, expireTime, exportNode);
            });

            if (string.IsNullOrEmpty(this.ExportLink))
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_SetLinkExpirationTimeFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_SetLinkExpirationTimeFailed"));
                });
                return;
            };

            this.IsExported = true;
        }

        public async void RemoveLink()
        {
            // User must be online to perform this operation
            if (!IsUserOnline() || !this.OriginalMNode.isExported()) return;

            var disableExportNode = new DisableExportRequestListenerAsync();
            var result = await disableExportNode.ExecuteAsync(() =>
            {
                this.MegaSdk.disableExport(this.OriginalMNode, disableExportNode);
            });

            if(!result)
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_RemoveLinkFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_RemoveLinkFailed"));
                });
                return;
            }

            this.IsExported = false;
            this.ExportLink = null;
        }

        private void Download()
        {
            if (this.Parent != null && this.Parent.ItemCollection.IsMultiSelectActive)
            {
                if(this.Parent.DownloadCommand.CanExecute(null))
                    this.Parent.DownloadCommand.Execute(null);
                return;
            }
            Download(TransferService.MegaTransfers);
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

        /// <summary>
        /// Update core data associated with the SDK MNode object
        /// </summary>
        /// <param name="megaNode">Node to update</param>
        /// <param name="externalUpdate">Indicates if is an update external to the app. For example from an `onNodesUpdate`</param>
        public virtual void Update(MNode megaNode, bool externalUpdate = false)
        {
            this.OriginalMNode = megaNode;
            this.Handle = megaNode.getHandle();
            this.Base64Handle = megaNode.getBase64Handle();
            this.Type = megaNode.getType();
            this.Name = megaNode.getName();
            this.Size = this.MegaSdk.getSize(megaNode);
            this.SizeText = this.Size.ToStringAndSuffix(1);
            this.CreationTime = ConvertDateToString(megaNode.getCreationTime()).ToString("dd MMM yyyy");
            this.TypeText = this.GetTypeText();
            this.LinkExpirationTime = megaNode.getExpirationTime();
            this.AccessLevel.AccessType = (MShareType)SdkService.MegaSdk.getAccess(megaNode);

            // Needed to filtering when the change is done inside the app or externally and is received by an `onNodesUpdate`
            if (!externalUpdate || megaNode.hasChanged((int)MNodeChangeType.CHANGE_TYPE_PUBLIC_LINK))
            {
                this.IsExported = megaNode.isExported();
                this.ExportLink = this.IsExported ? megaNode.getPublicLink(true) : null;
            }

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

        private string GetTypeText()
        {
            if (this.IsFolder)
                return this.FolderLabelText;

            string extension = Path.GetExtension(this.Name);
            if (string.IsNullOrEmpty(extension))
                return this.UnknownLabelText;

            extension = extension.TrimStart('.');
            char[] ext = extension.ToCharArray();
            ext[0] = char.ToUpper(ext[0]);

            switch (FileService.GetFileType(this.Name))
            {
                case FileType.TYPE_FILE:
                    return new string(ext) + "/" + this.FileLabelText;

                case FileType.TYPE_IMAGE:
                    return new string(ext) + "/" + this.ImageLabelText;

                case FileType.TYPE_AUDIO:
                    return new string(ext) + "/" + this.AudioLabelText;

                case FileType.TYPE_VIDEO:
                    return new string(ext) + "/" + this.VideoLabelText;

                case FileType.TYPE_UNKNOWN:
                default:
                    return new string(ext) + "/" + this.UnknownLabelText;
            }
        }

        public void SetThumbnailImage()
        {
            if (this.Type == MNodeType.TYPE_FOLDER) return;

            if (this.ThumbnailImageUri != null) return;

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

        private FolderViewModel _parent;
        public FolderViewModel Parent
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

        public ObservableCollection<IMegaNode> ParentCollection { get; set; }

        public ObservableCollection<IMegaNode> ChildCollection { get; set; }

        public MNodeType Type { get; private set; }

        private string _typeText;
        public string TypeText
        {
            get { return _typeText; }
            set { SetField(ref _typeText, value); }
        }

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

        private AccessLevelViewModel _accessLevel;
        /// <summary>
        /// Access level to the node
        /// </summary>
        public AccessLevelViewModel AccessLevel
        {
            get { return _accessLevel; }
            set
            {
                SetField(ref _accessLevel, value);
                OnPropertyChanged(nameof(this.HasReadPermissions), 
                    nameof(this.HasReadWritePermissions),
                    nameof(this.HasFullAccessPermissions));
            }
        }

        /// <summary>
        /// Specifies if the node has read permissions
        /// </summary>
        public bool HasReadPermissions => this.AccessLevel == null ? false :
            (int)this.AccessLevel?.AccessType >= (int)MShareType.ACCESS_READ;

        /// <summary>
        /// Specifies if the node has read & write permissions
        /// </summary>
        public bool HasReadWritePermissions => this.AccessLevel == null ? false :
            (int)this.AccessLevel?.AccessType >= (int)MShareType.ACCESS_READWRITE;

        /// <summary>
        /// Specifies if the node has full access permissions
        /// </summary>
        public bool HasFullAccessPermissions => this.AccessLevel == null ? false :
            (int)this.AccessLevel?.AccessType >= (int)MShareType.ACCESS_FULL;

        #endregion

        #region Properties

        public AccountDetailsViewModel AccountDetails => AccountService.AccountDetails;

        private string _exportLink;
        public string ExportLink
        {
            get { return _exportLink; }
            set { SetField(ref _exportLink, value); }
        }

        public bool LinkWithExpirationTime => (LinkExpirationTime > 0) ? true : false;

        private long _linkExpirationTime;
        public long LinkExpirationTime
        {
            get { return _linkExpirationTime; }
            set
            {
                SetField(ref _linkExpirationTime, value);
                OnPropertyChanged("LinkWithExpirationTime");
                OnPropertyChanged("LinkExpirationDate");
            }
        }

        public DateTimeOffset? LinkExpirationDate
        {
            get
            {
                DateTime? _linkExpirationDate;
                if (LinkExpirationTime > 0)
                    _linkExpirationDate = OriginalDateTime.AddSeconds(LinkExpirationTime);
                else
                    _linkExpirationDate = null;

                return _linkExpirationDate;
            }
        }

        public string LocalDownloadPath => Path.Combine(ApplicationData.Current.LocalFolder.Path,
            ResourceService.AppResources.GetString("AR_DownloadsDirectory"), this.Name);

        #endregion

        #region UiResources

        public string AddedLabelText => ResourceService.UiResources.GetString("UI_Added");
        public string AudioLabelText => ResourceService.UiResources.GetString("UI_Audio");
        public string DetailsText => ResourceService.UiResources.GetString("UI_Details");
        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string EnableOfflineViewText => ResourceService.UiResources.GetString("UI_EnableOfflineVIew");
        public string EnableLinkText => ResourceService.UiResources.GetString("UI_EnableLink");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string ClosePanelText => ResourceService.UiResources.GetString("UI_ClosePanel");
        public string CopyOrMoveText => CopyText + "/" + MoveText.ToLower();
        public string CopyText => ResourceService.UiResources.GetString("UI_Copy");
        public string FileLabelText => ResourceService.UiResources.GetString("UI_File");
        public string FilesLabelText => ResourceService.UiResources.GetString("UI_Files");
        public string FolderLabelText => ResourceService.UiResources.GetString("UI_Folder");
        public string FoldersLabelText => ResourceService.UiResources.GetString("UI_Folders");
        public string GetLinkText => ResourceService.UiResources.GetString("UI_GetLink");
        public string ImageLabelText => ResourceService.UiResources.GetString("UI_Image");
        public string InformationText => ResourceService.UiResources.GetString("UI_Information");
        public string LinkText => ResourceService.UiResources.GetString("UI_Link");
        public string ModifiedLabelText => ResourceService.UiResources.GetString("UI_Modified");
        public string MoveText => ResourceService.UiResources.GetString("UI_Move");
        public string PreviewText => ResourceService.UiResources.GetString("UI_Preview");
        public string RemoveText => ResourceService.UiResources.GetString("UI_Remove");
        public string RenameText => ResourceService.UiResources.GetString("UI_Rename");
        public string ShareText => ResourceService.UiResources.GetString("UI_Share");
        public string SizeLabelText => ResourceService.UiResources.GetString("UI_Size");
        public string TypeLabelText => ResourceService.UiResources.GetString("UI_Type");
        public string UnknownLabelText => ResourceService.UiResources.GetString("UI_Unknown");
        public string VideoLabelText => ResourceService.UiResources.GetString("UI_Video");

        public string SetLinkExpirationDateText => string.Format("{0} {1}",
            ResourceService.UiResources.GetString("UI_SetExpirationDate"),
            ResourceService.UiResources.GetString("UI_ProOnly"));

        #endregion

        #region VisualResources

        public string CopyOrMovePathData => ResourceService.VisualResources.GetString("VR_CopyOrMovePathData");
        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string LinkPathData => ResourceService.VisualResources.GetString("VR_LinkPathData");
        public string InformationPathData => ResourceService.VisualResources.GetString("VR_InformationPathData");
        public string PreviewImagePathData => ResourceService.VisualResources.GetString("VR_PreviewImagePathData");
        public string RenamePathData => ResourceService.VisualResources.GetString("VR_RenamePathData");
        public string RubbishBinPathData => ResourceService.VisualResources.GetString("VR_RubbishBinPathData");        

        #endregion
    }
}
