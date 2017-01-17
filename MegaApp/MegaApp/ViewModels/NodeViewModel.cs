using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
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

namespace MegaApp.ViewModels
{
    /// <summary>
    /// ViewModel of the main MEGA datatype (MNode)
    /// </summary>
    public abstract class NodeViewModel : BaseSdkViewModel, IMegaNode
    {
        // Offset DateTime value to calculate the correct creation and modification time
        private static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        protected NodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, FolderViewModel parent,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
        {
            Update(megaNode);
            SetDefaultValues();

            this.Parent = parent;
            this.ParentCollection = parentCollection;
            this.ChildCollection = childCollection;
            this.DownloadCommand = new RelayCommand(Download);
            this.RenameCommand = new RelayCommand(Rename);
            this.MoveToRubbishBinCommand = new RelayCommand(MoveToRubbishBin);
            this.RemoveCommand = new RelayCommand(Remove);
        }

        private void ParentOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Parent.CurrentViewState))
                OnPropertyChanged(nameof(this.Parent));
        }

        #region Commands

        public ICommand DownloadCommand { get; }
        public ICommand RenameCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand MoveToRubbishBinCommand { get; }

        #endregion

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

        private void GetThumbnail()
        {
            if (FileService.FileExists(this.ThumbnailPath))
            {
                this.IsDefaultImage = false;
                this.ThumbnailImageUri = new Uri(this.ThumbnailPath);
            }
            else if (Convert.ToBoolean(this.MegaSdk.isLoggedIn()) || this.ParentContainerType == ContainerType.FolderLink)
            {
                this.MegaSdk.getThumbnail(this.OriginalMNode, this.ThumbnailPath, new GetThumbnailRequestListener(this));
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
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_RenameNodeFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_RenameNodeFailed"));
                return;
            };
            
            OnUiThread(() => this.Name = newName);
        }
                
        public NodeActionResult Move(IMegaNode newParentNode)
        {
            return NodeActionResult.IsBusy;
        }

        public NodeActionResult Copy(IMegaNode newParentNode)
        {
            return NodeActionResult.IsBusy;
        }

        private async void MoveToRubbishBin()
        {
            if (this.Parent?.CurrentViewState == FolderContentViewState.MultiSelect)
            {
                if (this.Parent.MoveToRubbishBinCommand.CanExecute(null))
                    this.Parent.MoveToRubbishBinCommand.Execute(null);
                return;
            }
            await MoveToRubbishBinAsync();
        }

        public async Task MoveToRubbishBinAsync(bool isMultiSelect = false)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            if (this.OriginalMNode == null) return;

            if (!isMultiSelect)
            {
                var dialogResult = await DialogService.ShowOkCancelAsync(
                    ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion"), this.Name));

                if (!dialogResult) return;
            }

            var moveNode = new MoveNodeRequestListenerAsync();
            var result = await moveNode.ExecuteAsync(() =>
            {
                this.MegaSdk.moveNode(this.OriginalMNode, this.MegaSdk.getRubbishNode(), moveNode);
            });

            if (result) return;

            await DialogService.ShowAlertAsync(
                  ResourceService.AppMessages.GetString("AM_MoveToRubbishBinFailed_Title"),
                  string.Format(ResourceService.AppMessages.GetString("AM_MoveToRubbishBinFailed"), this.Name));
        }

        private async void Remove()
        {
            if (this.Parent?.CurrentViewState == FolderContentViewState.MultiSelect)
            {
                if (this.Parent.RemoveCommand.CanExecute(null))
                    this.Parent.RemoveCommand.Execute(null);
                return;
            }
            await RemoveAsync();
        }

        public async Task RemoveAsync(bool isMultiSelect = false)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            if (this.OriginalMNode == null) return;

            if (!isMultiSelect)
            {
                var dialogResult = await DialogService.ShowOkCancelAsync(
                    ResourceService.AppMessages.GetString("AM_RemoveItemQuestion_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveItemQuestion"), this.Name));

                if (!dialogResult) return;
            }

            var removeNode = new DeleteNodeRequestListenerAsync();
            var result = await removeNode.ExecuteAsync(() =>
            {
                this.MegaSdk.remove(this.OriginalMNode, removeNode);
            });

            if (result) return;

            await DialogService.ShowAlertAsync(
                  ResourceService.AppMessages.GetString("AM_RemoveNodeFailed_Title"),
                  string.Format(ResourceService.AppMessages.GetString("AM_RemoveNodeFailed"), this.Name));
        }

        public NodeActionResult GetLink()
        {
            return NodeActionResult.IsBusy;
        }

        private void Download()
        {
            if (this.Parent?.CurrentViewState == FolderContentViewState.MultiSelect)
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

        public void Update(MNode megaNode)
        {
            this.OriginalMNode = megaNode;
            this.Handle = megaNode.getHandle();
            this.Base64Handle = megaNode.getBase64Handle();
            this.Type = megaNode.getType();
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

        public ContainerType ParentContainerType { get; private set; }

        public NodeDisplayMode DisplayMode { get; set; }

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

        #region UiResources

        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string MoveToRubbishBinText => ResourceService.UiResources.GetString("UI_MoveToRubbishBin");
        public string RemoveText => ResourceService.UiResources.GetString("UI_Remove");
        public string RenameText => ResourceService.UiResources.GetString("UI_Rename");

        #endregion
    }
}
