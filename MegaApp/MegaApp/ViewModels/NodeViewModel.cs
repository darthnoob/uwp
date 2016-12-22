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
using System.Collections.Generic;
using System.Linq;

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

            ParentCollection = parentCollection;
            ChildCollection = childCollection;
        }

        #region Private Methods

        private void SetDefaultValues()
        {
            IsMultiSelected = false;
            DisplayMode = NodeDisplayMode.Normal;

            if (Type == MNodeType.TYPE_FOLDER) return;

            if (FileService.FileExists(ThumbnailPath))
            {
                IsDefaultImage = false;
                ThumbnailImageUri = new Uri(ThumbnailPath);
            }
            else
            {
                IsDefaultImage = true;
                DefaultImagePathData = ImageService.GetDefaultFileTypePathData(Name);
            }
        }

        private void GetThumbnail()
        {
            if (FileService.FileExists(ThumbnailPath))
            {
                IsDefaultImage = false;
                ThumbnailImageUri = new Uri(ThumbnailPath);
            }
            else if (Convert.ToBoolean(MegaSdk.isLoggedIn()) || ParentContainerType == ContainerType.FolderLink)
            {
                MegaSdk.getThumbnail(OriginalMNode, ThumbnailPath, new GetThumbnailRequestListener(this));
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
                    ResourceService.AppResources.GetString("AR_ThumbnailsDirectory"),
                    OriginalMNode.getBase64Handle());
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
            get { return Type == MNodeType.TYPE_FOLDER ? true : false; }
        }

        public bool IsImage => ImageService.IsImage(Name);

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

        public NodeActionResult Rename()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            // Only 1 CustomInputDialog should be open at the same time.
            if (App.AppInformation.PickerOrAsyncDialogIsOpen) return NodeActionResult.Cancelled;

            var settings = new CustomInputDialogSettings()
            {
                DefaultText = Name,
                SelectDefaultText = true,
                IgnoreExtensionInSelection = true,
            };

            var inputDialog = new CustomInputDialog(
                ResourceService.UiResources.GetString("UI_Rename"), 
                ResourceService.UiResources.GetString("UI_TypeNewName"), 
                App.AppInformation, settings);

            inputDialog.OkButtonTapped += (sender, args) =>
            {
                MegaSdk.renameNode(OriginalMNode, args.InputText, new RenameNodeRequestListener(this));
            };

            inputDialog.ShowDialog();

            return NodeActionResult.IsBusy;
        }
                
        public NodeActionResult Move(IMegaNode newParentNode)
        {
            return NodeActionResult.IsBusy;
        }

        public NodeActionResult Copy(IMegaNode newParentNode)
        {
            return NodeActionResult.IsBusy;
        }

        public async Task<NodeActionResult> MoveToRubbishBinAsync(bool isMultiSelect = false, AutoResetEvent waitEventRequest = null)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            if (OriginalMNode == null) return NodeActionResult.Failed;

            if (!isMultiSelect)
            {
                var result = await new CustomMessageDialog(
                    ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion"), Name),
                    App.AppInformation,
                    MessageDialogButtons.OkCancel).ShowDialogAsync();

                if (result == MessageDialogResult.CancelNo) return NodeActionResult.Cancelled;
            }

            MegaSdk.moveNode(OriginalMNode, MegaSdk.getRubbishNode(),
                new RemoveNodeRequestListener(this, isMultiSelect, waitEventRequest));

            return NodeActionResult.IsBusy;
        }

        public async Task<NodeActionResult> RemoveAsync(bool isMultiSelect = false, AutoResetEvent waitEventRequest = null)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            if (OriginalMNode == null) return NodeActionResult.Failed;

            if (!isMultiSelect)
            {
                var result = await new CustomMessageDialog(
                    ResourceService.AppMessages.GetString("AM_RemoveItemQuestion_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveItemQuestion"), Name),
                    App.AppInformation,
                    MessageDialogButtons.OkCancel).ShowDialogAsync();

                if (result == MessageDialogResult.CancelNo) return NodeActionResult.Cancelled;
            }

            MegaSdk.remove(OriginalMNode, 
                new DeleteNodeRequestListener(this, isMultiSelect, waitEventRequest));

            return NodeActionResult.IsBusy;
        }

        public NodeActionResult GetLink()
        {
            return NodeActionResult.IsBusy;
        }

        public async void Download(TransferQueue transferQueue)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            var downloadFolder = await FolderService.SelectFolder();
            if (downloadFolder != null)
            {
                if (await TransferService.CheckExternalDownloadPathAsync(downloadFolder.Path))
                {
                    Transfer.ExternalDownloadPath = downloadFolder.Path;
                    transferQueue.Add(Transfer);
                    Transfer.StartTransfer();
                }
            }
        }

        public void Update(MNode megaNode, ContainerType parentContainerType)
        {
            OriginalMNode = megaNode;
            Handle = megaNode.getHandle();
            Base64Handle = megaNode.getBase64Handle();
            Type = megaNode.getType();
            ParentContainerType = parentContainerType;
            Name = megaNode.getName();
            Size = MegaSdk.getSize(megaNode);
            SizeText = Size.ToStringAndSuffix();
            IsExported = megaNode.isExported();
            CreationTime = ConvertDateToString(megaNode.getCreationTime()).ToString("dd MMM yyyy");

            if (Type == MNodeType.TYPE_FILE)
                ModificationTime = ConvertDateToString(megaNode.getModificationTime()).ToString("dd MMM yyyy");
            else
                ModificationTime = CreationTime;

            //if (!App.MegaSdk.isInShare(megaNode) && ParentContainerType != ContainerType.PublicLink &&
            //    ParentContainerType != ContainerType.InShares && ParentContainerType != ContainerType.ContactInShares &&
            //    ParentContainerType != ContainerType.FolderLink)
            //    CheckAndUpdateSFO(megaNode);
            IsAvailableOffline = false;
            IsSelectedForOffline = false;
        }

        public void SetThumbnailImage()
        {
            if (Type == MNodeType.TYPE_FOLDER) return;

            if (ThumbnailImageUri != null && !IsDefaultImage) return;

            if (IsImage || OriginalMNode.hasThumbnail())
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

        public NodeDisplayMode DisplayMode { get; set; }

        private bool _isSelectedForOffline;
        public bool IsSelectedForOffline
        {
            get { return _isSelectedForOffline; }
            set
            {
                SetField(ref _isSelectedForOffline, value);
                IsSelectedForOfflineText = _isSelectedForOffline ? 
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
            ResourceService.AppResources.GetString("AR_DownloadsDirectory"), Name);

        #endregion
    }
}
