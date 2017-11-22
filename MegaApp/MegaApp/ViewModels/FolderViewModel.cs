using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GoedWare.Controls.Breadcrumb;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels
{
    /// <summary>
    /// Class that handles all process and operations of a section that contains MEGA nodes
    /// </summary>
    public class FolderViewModel : BaseSdkViewModel
    {
        public event EventHandler FolderNavigatedTo;

        public event EventHandler ChangeViewEvent;

        public event EventHandler AcceptCopyEvent;
        public event EventHandler AcceptMoveEvent;
        public event EventHandler CancelCopyOrMoveEvent;
        public event EventHandler CopyOrMoveEvent;

        public event EventHandler EnableMultiSelect;
        public event EventHandler DisableMultiSelect;

        public event EventHandler OpenNodeDetailsEvent;
        public event EventHandler CloseNodeDetailsEvent;

        public event EventHandler ChildNodesCollectionChanged;

        public FolderViewModel(ContainerType containerType)
        {
            this.Type = containerType;

            this.FolderRootNode = null;
            this.IsLoaded = false;
            this.IsBusy = false;
            this.BusyText = null;
            this.BreadCrumb = new BreadCrumbViewModel();
            this.ItemCollection = new CollectionViewModel<IMegaNode>();
            this.CopyOrMoveSelectedNodes = new List<IMegaNode>();

            this.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;

            this.AcceptCopyCommand = new RelayCommand(AcceptCopy);
            this.AcceptMoveCommand = new RelayCommand(AcceptMove);
            this.AddFolderCommand = new RelayCommand(AddFolder);
            this.CancelCopyOrMoveCommand = new RelayCommand(CancelCopyOrMove);
            this.ChangeViewCommand = new RelayCommand(ChangeView);
            this.CopyOrMoveCommand = new RelayCommand(CopyOrMove);
            this.DownloadCommand = new RelayCommand(Download);
            this.GetLinkCommand = new RelayCommand(GetLink);
            this.MultiSelectCommand = new RelayCommand(MultiSelect);
            this.HomeSelectedCommand = new RelayCommand(BrowseToHome);
            this.ItemSelectedCommand = new RelayCommand<BreadcrumbEventArgs>(ItemSelected);
            this.RefreshCommand = new RelayCommand(Refresh);
            this.RemoveCommand = new RelayCommand(Remove);
            this.RenameCommand = new RelayCommand(Rename);
            this.UploadCommand = new RelayCommand(Upload);
            this.SelectionChangedCommand = new RelayCommand(SelectionChanged);
            this.OpenNodeDetailsCommand = new RelayCommand(OpenNodeDetails);
            this.CloseNodeDetailsCommand = new RelayCommand(CloseNodeDetails);

            SetViewDefaults();

            SetEmptyContent(true);

            switch (containerType)
            {
                case ContainerType.CloudDrive:
                    this.CurrentViewState = FolderContentViewState.CloudDrive;
                    break;
                case ContainerType.RubbishBin:
                    this.CurrentViewState = FolderContentViewState.RubbishBin;
                    break;
                case ContainerType.InShares:
                    this.CurrentViewState = FolderContentViewState.InShares;
                    break;
                case ContainerType.OutShares:
                    this.CurrentViewState = FolderContentViewState.OutShares;
                    break;
                case ContainerType.ContactInShares:
                    this.CurrentViewState = FolderContentViewState.ContactInShares;
                    break;
                case ContainerType.FolderLink:
                    this.CurrentViewState = FolderContentViewState.FolderLink;
                    break;
                case ContainerType.CameraUploads:
                    this.CurrentViewState = FolderContentViewState.CameraUploads;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(containerType));
            }
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            if (this.CurrentViewState != FolderContentViewState.MultiSelect)
                this.PreviousViewState = this.CurrentViewState;

            this.CurrentViewState = FolderContentViewState.MultiSelect;
        }

        private void OnMultiSelectDisabled(object sender, EventArgs e)
        {
            if (this.PreviousViewState == FolderContentViewState.MultiSelect) return;
            this.CurrentViewState = this.PreviousViewState;
            this.PreviousViewState = FolderContentViewState.MultiSelect;
        }

        private void SelectionChanged()
        {
            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
                this.IsMultiSelectActive = (this.IsMultiSelectActive && this.ItemCollection.HasSelectedItems) ||
                    this.ItemCollection.MoreThanOneSelected;
            else
                this.IsMultiSelectActive = this.ItemCollection.HasSelectedItems;

            if (!this.ItemCollection.HasSelectedItems) return;

            var focusedNode = (NodeViewModel)this.ItemCollection.SelectedItems.Last();
            if (focusedNode is ImageNodeViewModel)
                (focusedNode as ImageNodeViewModel).InViewingRange = true;

            this.FocusedNode = focusedNode;
        }

        public void OpenNodeDetails()
        {
            OpenNodeDetailsEvent?.Invoke(this, EventArgs.Empty);
        }

        public void CloseNodeDetails()
        {
            CloseNodeDetailsEvent?.Invoke(this, EventArgs.Empty);
        }

        public void OnNodeAdded(object sender, MNode mNode)
        {
            if (mNode == null) return;

            var isProcessed = false;

            var parentNode = this.MegaSdk.getParentNode(mNode);

            var nodeToUpdateInView = ItemCollection.Items.FirstOrDefault(
                node => node.Base64Handle.Equals(mNode.getBase64Handle()));

            if (nodeToUpdateInView != null)
            {
                var isMoved = !FolderRootNode.Base64Handle.Equals(parentNode.getBase64Handle());

                if (isMoved)
                {
                    UiService.OnUiThread(() =>
                    {
                        try
                        {
                            ItemCollection.Items.Remove(nodeToUpdateInView);
                            ((FolderNodeViewModel)FolderRootNode).SetFolderInfo();
                            FolderService.UpdateFolders(this);
                        }
                        catch (Exception) { /* Dummy catch, supress possible exception */ }
                    });
                }
                else
                {
                    UiService.OnUiThread(() =>
                    {
                        try { nodeToUpdateInView.Update(mNode, true); }
                        catch (Exception) { /* Dummy catch, supress possible exception */ }
                    });
                    isProcessed = true;
                }

                this.ChildNodesCollectionChanged?.Invoke(this, EventArgs.Empty);
                UiService.OnUiThread(() => OnPropertyChanged("IsEmpty"));
            }

            if (parentNode == null || isProcessed) return;

            var isAddedInFolder = FolderRootNode.Base64Handle.Equals(parentNode.getBase64Handle());

            // If node is added in current folder, process the add action
            if (isAddedInFolder)
            {
                // Retrieve the index from the SDK
                // Substract -1 to get a valid list index
                var insertIndex = this.MegaSdk.getIndex(mNode,
                    (int)UiService.GetSortOrder(parentNode.getBase64Handle(), parentNode.getName())) - 1;

                // If the insert position is higher than the ChilNodes size insert in the last position
                if (insertIndex >= ItemCollection.Items.Count)
                {
                    UiService.OnUiThread(() =>
                    {
                        try
                        {
                            ItemCollection.Items.Add(NodeService.CreateNew(this.MegaSdk,
                                App.AppInformation, mNode, this));

                            ((FolderNodeViewModel)FolderRootNode).SetFolderInfo();
                            FolderService.UpdateFolders(this);
                        }
                        catch (Exception) { /* Dummy catch, supress possible exception */ }
                    });
                }
                // Insert the node at a specific position
                else
                {
                    // Insert position can never be less then zero
                    // Replace negative index with first possible index zero
                    if (insertIndex < 0) insertIndex = 0;
                        
                    UiService.OnUiThread(() =>
                    {
                        try
                        {
                            ItemCollection.Items.Insert(insertIndex,
                                NodeService.CreateNew(this.MegaSdk, App.AppInformation, mNode, this));

                            ((FolderNodeViewModel)FolderRootNode).SetFolderInfo();
                            FolderService.UpdateFolders(this);
                        }
                        catch (Exception) { /* Dummy catch, supress possible exception */ }
                    });
                }
            }

            if (nodeToUpdateInView != null)
            {
                UiService.OnUiThread(() =>
                {
                    try
                    {
                        nodeToUpdateInView.Update(parentNode, true);
                        var folderNode = nodeToUpdateInView as FolderNodeViewModel;
                        folderNode?.SetFolderInfo();
                    }
                    catch (Exception) { /* Dummy catch, supress possible exception */ }
                });
            }

            // Unconditional scenarios
            // Move/delete/add actions in subfolders
            UiService.OnUiThread(() =>
            {
                try { FolderService.UpdateFolders(this); }
                catch (Exception) { /* Dummy catch, supress possible exception */ }
            });

            this.ChildNodesCollectionChanged?.Invoke(this, EventArgs.Empty);
            UiService.OnUiThread(() => OnPropertyChanged("IsEmpty"));
        }

        public void OnNodeRemoved(object sender, MNode mNode)
        {
            if (mNode == null) return;

            var isProcessed = false;

            var nodeToRemoveFromView = ItemCollection.Items.FirstOrDefault(
                node => node.Base64Handle.Equals(mNode.getBase64Handle()));

            // If node is found in current view, process the remove action
            if (nodeToRemoveFromView != null)
            {
                UiService.OnUiThread(() =>
                {
                    try
                    {
                        ItemCollection.Items.Remove(nodeToRemoveFromView);
                        ((FolderNodeViewModel) FolderRootNode).SetFolderInfo();
                    }
                    catch (Exception) { /* Dummy catch, supress possible exception */ }
                });

                isProcessed = true;
            }

            if (!isProcessed)
            {
                // REMOVED in subfolder scenario

                var parentNode = this.MegaSdk.getParentNode(mNode);

                if (parentNode == null) return;

                var nodeToUpdateInView = ItemCollection.Items.FirstOrDefault(
                    node => node.Base64Handle.Equals(parentNode.getBase64Handle()));

                // If parent folder is found, process the update action
                if (nodeToUpdateInView != null)
                {
                    UiService.OnUiThread(() =>
                    {
                        try
                        {
                            nodeToUpdateInView.Update(parentNode, true);
                            var folderNode = nodeToUpdateInView as FolderNodeViewModel;
                            folderNode?.SetFolderInfo();
                        }
                        catch (Exception) { /* Dummy catch, supress possible exception */ }
                    });
                }
            }

            this.ChildNodesCollectionChanged?.Invoke(this, EventArgs.Empty);
            UiService.OnUiThread(() => OnPropertyChanged("IsEmpty"));
        }

        #region Commands

        public ICommand AcceptCopyCommand { get; }
        public ICommand AcceptMoveCommand { get; }
        public ICommand AddFolderCommand { get; private set; }
        public ICommand CancelCopyOrMoveCommand { get; }
        public ICommand ChangeViewCommand { get; }
        public ICommand CopyOrMoveCommand { get; }        
        public ICommand DownloadCommand { get; private set; }
        public ICommand GetLinkCommand { get; }
        public ICommand HomeSelectedCommand { get; }
        public ICommand ItemSelectedCommand { get; }
        public ICommand MultiSelectCommand { get; set; }
        public ICommand RefreshCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand RenameCommand { get; }
        public ICommand UploadCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand OpenNodeDetailsCommand { get; private set; }
        public ICommand CloseNodeDetailsCommand { get; }

        #endregion

        #region Public Methods

        public void SelectAll() => this.ItemCollection.SelectAll(true);
        public void DeselectAll() => this.ItemCollection.SelectAll(false);
        public void ClearChildNodes() => this.ItemCollection.Clear();

        /// <summary>
        /// Load the mega nodes for this specific folder using the Mega SDK
        /// </summary>
        public void LoadChildNodes()
        {
            // User must be online to perform this operation
            if ((this.Type != ContainerType.FolderLink) && !IsUserOnline())
                return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // FolderRootNode should not be null
            if (this.FolderRootNode == null)
            {
                new CustomMessageDialog(
                    ResourceService.AppMessages.GetString("AM_LoadNodesFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_LoadNodesFailed"),
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
                return;
            }

            SetProgressIndication(true);

            // Process is started so we can set the empty content template to loading already
            SetEmptyContent(true);

            GetCurrentOrderDirection();

            // Get the MNodes from the Mega SDK in the correct sorting order for the current folder
            MNodeList childList = GetChildren();

            if (childList == null)
            {
                new CustomMessageDialog(
                    ResourceService.AppMessages.GetString("AM_LoadNodesFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_LoadNodesFailed"),
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();

                SetEmptyContent(false);

                return;
            }

            // Clear the child nodes to make a fresh start
            OnUiThread(this.ItemCollection.Clear);

            // Set the correct view. Do this after the childs are cleared to speed things up
            SetViewOnLoad();

            // Build the bread crumbs. Do this before loading the nodes so that the user can click on home
            OnUiThread(() =>
            {
                this.BreadCrumb.Create(this);
                OnPropertyChanged("HasBreadCrumbPath");
            });

            // Create the option to cancel
            CreateLoadCancelOption();

            // Load and create the childnodes for the folder
            Task.Factory.StartNew(() =>
            {
                try
                {
                    CreateChildren(childList, childList.size());
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just exit this background process because a cancellation exception has been thrown
                }

            }, this.LoadingCancelToken, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
        }

        /// <summary>
        /// Cancel any running load process of this folder
        /// </summary>
        protected void CancelLoad()
        {
            if (this.LoadingCancelTokenSource != null && this.LoadingCancelToken.CanBeCanceled)
                this.LoadingCancelTokenSource.Cancel();
        }

        /// <summary>
        /// Refresh the current folder. Delete cached thumbnails and reload the nodes
        /// </summary>
        private async void Refresh()
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            CloseNodeDetails();

            FileService.ClearFiles(
                NodeService.GetFiles(this.ItemCollection.Items,
                Path.Combine(ApplicationData.Current.LocalFolder.Path,
                ResourceService.AppResources.GetString("AR_ThumbnailsDirectory"))));

            if (this.FolderRootNode == null)
            {
                switch (this.Type)
                {
                    case ContainerType.RubbishBin:
                        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, 
                            App.AppInformation, this.MegaSdk.getRubbishNode(), this);
                        break;

                    case ContainerType.CloudDrive:
                    case ContainerType.FolderLink:
                        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, 
                            App.AppInformation, this.MegaSdk.getRootNode(), this);
                        break;
                    case ContainerType.CameraUploads:
                        var cameraUploadsNode = await SdkService.GetCameraUploadRootNodeAsync();
                        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk,
                            App.AppInformation, cameraUploadsNode, this);
                        
                        break;
                }
            }

            LoadChildNodes();
        }

        private void AcceptCopy() => AcceptCopyEvent?.Invoke(this, EventArgs.Empty);

        private void AcceptMove() => AcceptMoveEvent?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Add a new sub-folder to the current folder
        /// </summary>
        private async void AddFolder()
        {
            if (!IsUserOnline()) return;

            var folderName = await DialogService.ShowInputDialogAsync(
                ResourceService.UiResources.GetString("UI_NewFolder"),
                ResourceService.UiResources.GetString("UI_TypeFolderName"));

            if (string.IsNullOrEmpty(folderName) || string.IsNullOrWhiteSpace(folderName)) return;

            if (this.FolderRootNode == null)
            {
                OnUiThread(async() =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_CreateFolderFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_CreateFolderFailed"));
                });
                return;
            }

            var createFolder = new CreateFolderRequestListenerAsync();
            var result = await createFolder.ExecuteAsync(() =>
            {
                this.MegaSdk.createFolder(folderName, this.FolderRootNode.OriginalMNode, createFolder);
            });

            if (!result)
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_CreateFolderFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_CreateFolderFailed"));
                });
            }
        }

        private void CancelCopyOrMove() => CancelCopyOrMoveEvent?.Invoke(this, EventArgs.Empty);

        private void CopyOrMove() => CopyOrMoveEvent?.Invoke(this, EventArgs.Empty);

        private async void Download()
        {
            if (!this.ItemCollection.HasSelectedItems) return;
            await MultipleDownloadAsync(this.ItemCollection.SelectedItems);
            this.IsMultiSelectActive = false;
        }

        private async Task MultipleDownloadAsync(ICollection<IMegaNode> nodes)
        {
            if (nodes == null || nodes.Count < 1) return;

            var downloadFolder = await FolderService.SelectFolder();
            if (downloadFolder != null)
            {
                if(await TransferService.CheckExternalDownloadPathAsync(downloadFolder.Path))
                {
                    foreach (var node in nodes)
                    {
                        node.Transfer.ExternalDownloadPath = downloadFolder.Path;
                        TransferService.MegaTransfers.Add(node.Transfer);
                        node.Transfer.StartTransfer();
                    }
                }
            }
        }

        private void GetLink()
        {
            if (!(bool)this.ItemCollection?.OnlyOneSelectedItem) return;
            this.ItemCollection.FocusedItem?.GetLinkAsync();
        }

        /// <summary>
        /// Sets if multiselect is active or not.
        /// </summary>
        private void MultiSelect() => this.IsMultiSelectActive = !this.IsMultiSelectActive;

        private async void Remove()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            int count = this.ItemCollection.SelectedItems.Count;

            string title, message;
            switch(this.Type)
            {
                case ContainerType.CloudDrive:
                case ContainerType.CameraUploads:
                case ContainerType.ContactInShares:
                case ContainerType.InShares:
                case ContainerType.OutShares:
                    title = ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion_Title");
                    message = this.ItemCollection.OnlyOneSelectedItem ?
                        string.Format(ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion"), this.ItemCollection.SelectedItems.First().Name) :
                        string.Format(ResourceService.AppMessages.GetString("AM_MultiMoveToRubbishBinQuestion"), count);
                    break;

                case ContainerType.RubbishBin:
                    title = ResourceService.AppMessages.GetString("AM_MultiSelectRemoveQuestion_Title");
                    message = string.Format(ResourceService.AppMessages.GetString("AM_MultiSelectRemoveQuestion"), count);
                    message = this.ItemCollection.OnlyOneSelectedItem ?
                        string.Format(ResourceService.AppMessages.GetString("AM_RemoveItemQuestion"), this.ItemCollection.SelectedItems.First().Name) :
                        string.Format(ResourceService.AppMessages.GetString("AM_MultiSelectRemoveQuestion"), count);
                    break;

                default:
                    return;
            }

            var result = await DialogService.ShowOkCancelAsync(title, message);

            if (!result) return;

            // Use a temp variable to avoid InvalidOperationException
            MultipleRemoveAsync(this.ItemCollection.SelectedItems.ToList());

            this.IsMultiSelectActive = false;
        }

        private async void MultipleRemoveAsync(ICollection<IMegaNode> nodes)
        {
            if (nodes == null || nodes.Count < 1) return;
            
            bool result = true;
            foreach (var node in nodes)
            {
                result = result & (await node.RemoveAsync(true));
            }

            if(!result)
            {
                string title, message;
                switch (this.Type)
                {
                    case ContainerType.CloudDrive:
                    case ContainerType.CameraUploads:
                    case ContainerType.ContactInShares:
                    case ContainerType.InShares:
                    case ContainerType.OutShares:
                        title = ResourceService.AppMessages.GetString("AM_MoveToRubbishBinFailed_Title");
                        message = ResourceService.AppMessages.GetString("AM_MoveToRubbishBinMultipleNodesFailed");
                        break;

                    case ContainerType.RubbishBin:
                        title = ResourceService.AppMessages.GetString("AM_RemoveFailed_Title");
                        message = ResourceService.AppMessages.GetString("AM_RemoveMultipleNodesFailed");
                        break;

                    default:
                        return;
                }

                OnUiThread(async () => await DialogService.ShowAlertAsync(title, message));
            }            
        }

        private async void Rename()
        {
            if (!this.ItemCollection.HasSelectedItems || this.ItemCollection.MoreThanOneSelected) return;

            var selectedNode = this.ItemCollection.SelectedItems.First();
            if (selectedNode == null) return;
            await selectedNode.RenameAsync();
        }

        /// <summary>
        /// Select files for upload to cloud
        /// </summary>
        private async void Upload()
        {
            // Set upload directory only once for speed improvement and if not exists, create dir
            var uploadDir = AppService.GetUploadDirectoryPath(true);

            // Create a dictionary to store the files and its corresponding transfer object.            
            var uploads = new Dictionary<StorageFile, TransferObjectModel>();

            // Pick up the files to upload
            var pickedFiles = await FileService.SelectMultipleFiles();

            // First create the transfers object and fill the dictionary
            foreach (StorageFile file in pickedFiles)
            {
                if (file == null) continue; // To avoid null references

                TransferObjectModel uploadTransfer = null;
                try
                {
                    uploadTransfer = new TransferObjectModel(
                        this.FolderRootNode, MTransferType.TYPE_UPLOAD,
                        Path.Combine(uploadDir, file.Name));

                    if(uploadTransfer != null)
                    {
                        uploadTransfer.DisplayName = file.Name;
                        uploadTransfer.TotalBytes = (await file.GetBasicPropertiesAsync()).Size;
                        uploadTransfer.PreparingUploadCancelToken = new CancellationTokenSource();
                        uploadTransfer.TransferState = MTransferState.STATE_NONE;
                        uploads.Add(file, uploadTransfer);
                        TransferService.MegaTransfers.Add(uploadTransfer);
                    }
                }
                catch (Exception e)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Transfer (UPLOAD) failed: " + file.Name, e);

                    OnUiThread(async () =>
                    {
                        if (uploadTransfer != null) uploadTransfer.TransferState = MTransferState.STATE_FAILED;
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_PrepareFileForUploadFailed_Title"),
                            string.Format(ResourceService.AppMessages.GetString("AM_PrepareFileForUploadFailed"), file.Name));
                    });
                }
            }

            // Second finish preparing transfers copying the files to the temporary upload folder
            foreach (var upload in uploads)
            {
                if (upload.Key == null || upload.Value == null) continue; // To avoid null references

                TransferObjectModel uploadTransfer = null;
                try
                {
                    uploadTransfer = upload.Value;

                    // If the upload isn´t already cancelled then copy the file to the temporary upload folder
                    if(uploadTransfer?.PreparingUploadCancelToken?.Token.IsCancellationRequested == false)
                    {
                        using (var fs = new FileStream(Path.Combine(uploadDir, upload.Key.Name), FileMode.Create))
                        {
                            // Set buffersize to avoid copy failure of large files
                            var stream = await upload.Key.OpenStreamForReadAsync();
                            await stream.CopyToAsync(fs, 8192, uploadTransfer.PreparingUploadCancelToken.Token);
                            await fs.FlushAsync(uploadTransfer.PreparingUploadCancelToken.Token);
                        }

                        uploadTransfer.StartTransfer();
                    }
                    else
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Transfer (UPLOAD) canceled: " + upload.Key.Name);
                        OnUiThread(() => uploadTransfer.TransferState = MTransferState.STATE_CANCELLED);
                    }
                }
                // If the upload is cancelled during the preparation process, 
                // changes the status and delete the corresponding temporary file
                catch (TaskCanceledException)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Transfer (UPLOAD) canceled: " + upload.Key.Name);
                    FileService.DeleteFile(uploadTransfer.TransferPath);
                    OnUiThread(() => uploadTransfer.TransferState = MTransferState.STATE_CANCELLED);
                }
                catch (Exception e)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Transfer (UPLOAD) failed: " + upload.Key.Name, e);
                    OnUiThread(async () =>
                    {
                        uploadTransfer.TransferState = MTransferState.STATE_FAILED;
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_PrepareFileForUploadFailed_Title"),
                            string.Format(ResourceService.AppMessages.GetString("AM_PrepareFileForUploadFailed"), upload.Key.Name));
                    });
                }
                finally
                {
                    uploadTransfer.PreparingUploadCancelToken = null;
                }
            }
        }

        public void OnChildNodeTapped(IMegaNode node)
        {
            // Needed to avoid process the node when the user is in MultiSelect mode and also after the
            // node is the last removed from selection and MultiSelect mode will be automatically disabled.
            if (this.CurrentViewState == FolderContentViewState.MultiSelect || this.ItemCollection.IsMultiSelectActive) return;
            
            if (this.PreviousViewState == FolderContentViewState.MultiSelect)
            {
                this.PreviousViewState = this.CurrentViewState;
                return;
            }

            switch (node.Type)
            {
                case MNodeType.TYPE_UNKNOWN:
                    break;
                case MNodeType.TYPE_FILE:
                    // If the user is moving nodes don't process the file node
                    if (this.CurrentViewState != FolderContentViewState.CopyOrMove)
                        ProcessFileNode(node);
                    break;
                case MNodeType.TYPE_FOLDER:
                    // If the user is moving nodes and the folder is one of the selected nodes don't navigate to it
                    if ((this.CurrentViewState == FolderContentViewState.CopyOrMove) && (IsCopyOrMoveSelectedNode(node))) return;
                    BrowseToFolder(node);
                    break;
                case MNodeType.TYPE_ROOT:
                    break;
                case MNodeType.TYPE_INCOMING:
                    break;
                case MNodeType.TYPE_RUBBISH:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Check if a node is in the selected nodes group for move, copy or any other action.
        /// </summary>        
        /// <param name="node">Node to check if is in the selected node list</param>        
        /// <returns>True if is a selected node or false in other case</returns>
        private bool IsCopyOrMoveSelectedNode(IMegaNode node)
        {
            if (CopyOrMoveSelectedNodes?.Count > 0)
            {
                var count = CopyOrMoveSelectedNodes.Count;
                for (int index = 0; index < count; index++)
                {
                    var selectedNode = CopyOrMoveSelectedNodes[index];
                    if (node.OriginalMNode.getBase64Handle() == selectedNode?.OriginalMNode.getBase64Handle())
                    {
                        //Update the selected nodes list values
                        node.DisplayMode = NodeDisplayMode.SelectedForCopyOrMove;
                        CopyOrMoveSelectedNodes[index] = node;

                        return true;
                    }
                }
            }

            return false;
        }

        protected void SetEmptyContent(bool isLoading)
        {
            if (isLoading)
            {
                OnUiThread(() =>
                {
                    this.EmptyInformationText = string.Empty;
                    this.EmptyStateHeaderText = string.Empty;
                    this.EmptyStateSubHeaderText = string.Empty;
                });
            }
            else
            {
                switch (this.Type)
                {
                    case ContainerType.CloudDrive:
                    case ContainerType.RubbishBin:
                        var megaRoot = this.MegaSdk.getRootNode();
                        var megaRubbishBin = this.MegaSdk.getRubbishNode();
                        if (this.FolderRootNode != null && megaRoot != null && this.FolderRootNode.Base64Handle.Equals(megaRoot.getBase64Handle()))
                        {
                            OnUiThread(() =>
                            {
                                this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyCloudDrive").ToLower();
                                this.EmptyStateHeaderText = ResourceService.EmptyStates.GetString("ES_CloudDriveHeader");
                                this.EmptyStateSubHeaderText = ResourceService.EmptyStates.GetString("ES_CloudDriveSubHeader");
                            });
                        }
                        else if (this.FolderRootNode != null && megaRubbishBin != null && this.FolderRootNode.Base64Handle.Equals(megaRubbishBin.getBase64Handle()))
                        {
                            OnUiThread(() =>
                            {
                                this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyRubbishBin").ToLower();
                                this.EmptyStateHeaderText = ResourceService.EmptyStates.GetString("ES_RubbishBinHeader");
                                this.EmptyStateSubHeaderText = ResourceService.EmptyStates.GetString("ES_RubbishBinSubHeader");
                            });
                        }
                        else
                        {
                            OnUiThread(() =>
                            {
                                this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyFolder").ToLower();
                                this.EmptyStateHeaderText = ResourceService.EmptyStates.GetString("ES_FolderHeader");

                                if (this.MegaSdk.isInRubbish(this.FolderRootNode.OriginalMNode))
                                    this.EmptyStateSubHeaderText = ResourceService.EmptyStates.GetString("ES_FolderRubbishBinSubHeader");
                                else
                                    this.EmptyStateSubHeaderText = ResourceService.EmptyStates.GetString("ES_FolderSubHeader");
                            });
                        }
                        break;

                    case ContainerType.CameraUploads:
                        break;

                    case ContainerType.ContactInShares:
                    case ContainerType.InShares:
                    case ContainerType.OutShares:
                        if (this is IncomingSharesViewModel)
                        {
                            OnUiThread(() =>
                            {
                                this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptySharedFolders").ToLower();
                                this.EmptyStateHeaderText = ResourceService.EmptyStates.GetString("ES_IncomingSharesHeader");
                                this.EmptyStateSubHeaderText = ResourceService.EmptyStates.GetString("ES_IncomingSharesSubHeader");
                            });
                            break;
                        }

                        if (this is OutgoingSharesViewModel)
                        {
                            OnUiThread(() =>
                            {
                                this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptySharedFolders").ToLower();
                                this.EmptyStateHeaderText = ResourceService.EmptyStates.GetString("ES_OutgoingSharesHeader");
                                this.EmptyStateSubHeaderText = ResourceService.EmptyStates.GetString("ES_OutgoingSharesSubHeader");
                            });
                            break;
                        }

                        OnUiThread(() =>
                        {
                            this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptySharedFolders").ToLower();
                            this.EmptyStateHeaderText = ResourceService.EmptyStates.GetString("ES_FolderHeader");
                            this.EmptyStateSubHeaderText = ResourceService.EmptyStates.GetString("ES_FolderSubHeader");
                        });
                        break;

                    case ContainerType.Offline:
                        OnUiThread(() =>
                        {
                            this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyOffline").ToLower();
                        });
                        break;

                    case ContainerType.FolderLink:
                        OnUiThread(() =>
                        {
                            this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyFolder").ToLower();
                        });
                        break;
                }
            }
        }

        public bool CanGoFolderUp()
        {
            if (this.FolderRootNode == null) return false;

            MNode parentNode = this.MegaSdk.getParentNode(this.FolderRootNode.OriginalMNode);
            if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN)
                return false;

            return true;
        }

        public virtual bool GoFolderUp()
        {
            if (this.FolderRootNode == null) return false;
            
            MNode parentNode = this.MegaSdk.getParentNode(this.FolderRootNode.OriginalMNode);
            if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN)
                return false;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, App.AppInformation, parentNode, this, this.ItemCollection.Items);

            LoadChildNodes();

            return true;
        }

        private void ItemSelected(BreadcrumbEventArgs e)
        {
            BrowseToFolder((IMegaNode)e.Item);
        }

        public virtual async void BrowseToHome()
        {
            if (this.FolderRootNode == null) return;

            CloseNodeDetails();

            MNode homeNode = null;
            FolderViewModel homeFolder = null;

            switch (this.Type)
            {
                case ContainerType.CloudDrive:
                    homeNode = this.MegaSdk.getRootNode();
                    homeFolder = new FolderViewModel(ContainerType.CloudDrive);
                    break;
                case ContainerType.RubbishBin:
                    homeNode = this.MegaSdk.getRubbishNode();
                    homeFolder = new FolderViewModel(ContainerType.RubbishBin);
                    break;
                case ContainerType.CameraUploads:
                    homeNode = await SdkService.GetCameraUploadRootNodeAsync();
                    homeFolder = new FolderViewModel(ContainerType.CameraUploads);
                    break;
            }

            if (homeNode == null) return;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, App.AppInformation, homeNode, homeFolder, this.ItemCollection.Items);
            OnFolderNavigatedTo();

            LoadChildNodes();
        }

        public void BrowseToFolder(IMegaNode node)
        {
            if (node == null) return;

            CloseNodeDetails();

            // Show the back button in desktop and tablet applications
            // Back button in mobile applications is automatic in the nav bar on screen
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            
            this.FolderRootNode = node;
            OnFolderNavigatedTo();

            LoadChildNodes();
        }

        public void ProcessFileNode(IMegaNode node)
        {
            if (node.IsImage)
            {
                // Navigate to the preview page
                OnUiThread(() =>
                {
                    this.FocusedNode = node;

                    var parameters = new Dictionary<NavigationParamType, object>();
                    parameters.Add(NavigationParamType.Data, this);

                    NavigateService.Instance.Navigate(typeof(PreviewImagePage), true,
                        NavigationObject.Create(this.GetType(), NavigationActionType.Default, parameters));
                });
            }
        }

        public void SetProgressIndication(bool onOff, string busyText = null)
        {
            OnUiThread(() =>
            {
                this.IsBusy = onOff;
                this.BusyText = busyText;
            });
        }

        private void CreateChildren(MNodeList childList, int listSize)
        {
            // Set the parameters for the performance for the different view types of a folder
            int viewportItemCount, backgroundItemCount;
            InitializePerformanceParameters(out viewportItemCount, out backgroundItemCount);

            // We will not add nodes one by one in the dispatcher but in groups
            List<IMegaNode> helperList;
            try { helperList = new List<IMegaNode>(1024); }
            catch (ArgumentOutOfRangeException) { helperList = new List<IMegaNode>(); }

            this.ItemCollection.DisableCollectionChangedDetection();

            for (int i = 0; i < listSize; i++)
            {
                // If the task has been cancelled, stop processing
                if (this.LoadingCancelToken.IsCancellationRequested)
                    this.LoadingCancelToken.ThrowIfCancellationRequested();

                // To avoid pass null values to CreateNew
                if (childList.get(i) == null) continue;

                var node = NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, childList.get(i), this, this.ItemCollection.Items);

                // If node creation failed for some reason, continue with the rest and leave this one
                if (node == null) continue;

                // If the user is moving nodes, check if the node had been selected to move 
                // and establish the corresponding display mode
                if (this.CurrentViewState == FolderContentViewState.CopyOrMove)
                {
                    // Check if it is one of the selected nodes
                    IsCopyOrMoveSelectedNode(node);
                }

                helperList.Add(node);

                // First add the viewport items to show some data to the user will still loading
                if (i == viewportItemCount)
                {
                    var viewPortList = helperList.ToList();
                    OnUiThread(() =>
                    {
                        // If the task has been cancelled, stop processing
                        foreach (var megaNode in viewPortList.TakeWhile(megaNode => !this.LoadingCancelToken.IsCancellationRequested))
                            this.ItemCollection.Items.Add(megaNode);
                        viewPortList.Clear();
                    });

                    helperList.Clear();
                    continue;
                }

                if (helperList.Count != backgroundItemCount || i <= viewportItemCount) continue;

                // Add the rest of the items in the background to the list
                var finalList = helperList.ToList();
                OnUiThread(() =>
                {
                    // If the task has been cancelled, stop processing
                    foreach (var megaNode in finalList.TakeWhile(megaNode => !this.LoadingCancelToken.IsCancellationRequested))
                        this.ItemCollection.Items.Add(megaNode);
                    finalList.Clear();
                });

                helperList.Clear();
            }

            // Add any nodes that are left over
            OnUiThread(() =>
            {
                // Show the user that processing the childnodes is done
                SetProgressIndication(false);

                // Set empty content to folder instead of loading view
                SetEmptyContent(false);

                // If the task has been cancelled, stop processing
                foreach (var megaNode in helperList.TakeWhile(megaNode => !this.LoadingCancelToken.IsCancellationRequested))
                    this.ItemCollection.Items.Add(megaNode);

                OnPropertyChanged("IsEmpty");
                this.IsLoaded = true;
            });

            this.ItemCollection.EnableCollectionChangedDetection();
        }

        private void InitializePerformanceParameters(out int viewportItemCount, out int backgroundItemCount)
        {
            viewportItemCount = 0;
            backgroundItemCount = 0;

            // Each view has different performance options
            switch (this.ViewMode)
            {
                case FolderContentViewMode.ListView:
                    viewportItemCount = 256;
                    backgroundItemCount = 1024;
                    break;
                case FolderContentViewMode.GridView:
                    viewportItemCount = 128;
                    backgroundItemCount = 512;
                    break;
            }
        }

        protected void CreateLoadCancelOption()
        {
            if (this.LoadingCancelTokenSource != null)
            {
                this.LoadingCancelTokenSource.Dispose();
                this.LoadingCancelTokenSource = null;
            }
            this.LoadingCancelTokenSource = new CancellationTokenSource();
            this.LoadingCancelToken = this.LoadingCancelTokenSource.Token;
        }

        /// <summary>
        /// Sets the view mode for the folder on load content.
        /// </summary>
        private void SetViewOnLoad()
        {
            if (this.FolderRootNode == null) return;

            SetView(UiService.GetViewMode(this.FolderRootNode.Base64Handle, this.FolderRootNode.Name));
        }

        /// <summary>
        /// Sets the default view mode for the folder content.
        /// </summary>
        private void SetViewDefaults()
        {
            OnUiThread(() =>
            {
                this.NodeTemplateSelector = new NodeTemplateSelector()
                {
                    FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListViewFileItemContent"],
                    FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListViewFolderItemContent"]
                };
            });

            this.ViewMode = FolderContentViewMode.ListView;
            this.NextViewButtonPathData = ResourceService.VisualResources.GetString("VR_GridViewPathData");
            this.NextViewButtonLabelText = ResourceService.UiResources.GetString("UI_GridView");
        }

        /// <summary>
        /// Changes the view mode for the folder content.
        /// </summary>
        private void ChangeView()
        {
            if (this.FolderRootNode == null) return;

            switch (this.ViewMode)
            {
                case FolderContentViewMode.ListView:
                    UiService.SetViewMode(this.FolderRootNode.Base64Handle, FolderContentViewMode.GridView);
                    SetView(FolderContentViewMode.GridView);
                    break;
                case FolderContentViewMode.GridView:
                    UiService.SetViewMode(this.FolderRootNode.Base64Handle, FolderContentViewMode.ListView);
                    SetView(FolderContentViewMode.ListView);
                    break;
            }

            this.ChangeViewEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the view mode for the folder content.
        /// </summary>
        /// <param name="viewMode">View mode to set.</param>
        public virtual void SetView(FolderContentViewMode viewMode)
        {
            switch (viewMode)
            {
                case FolderContentViewMode.GridView:
                    this.NodeTemplateSelector = new NodeTemplateSelector()
                    {
                        FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeGridViewFileItemContent"],
                        FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeGridViewFolderItemContent"]
                    };

                    this.ViewMode = FolderContentViewMode.GridView;
                    this.NextViewButtonPathData = ResourceService.VisualResources.GetString("VR_ListViewPathData");
                    this.NextViewButtonLabelText = ResourceService.UiResources.GetString("UI_ListView");
                    break;

                case FolderContentViewMode.ListView:
                    SetViewDefaults();
                    break;
            }
        }
        

        protected virtual void OnFolderNavigatedTo()
        {
            FolderNavigatedTo?.Invoke(this, EventArgs.Empty);
        }

        protected virtual MNodeList GetChildren()
        {
            return NodeService.GetChildren(this.MegaSdk, this.FolderRootNode);
        }

        private void GetCurrentOrderDirection()
        {
            if (this.FolderRootNode == null) return;

            var currentOrder = UiService.GetSortOrder(
                this.FolderRootNode.Base64Handle, this.FolderRootNode.Name);

            switch (currentOrder)
            {
                case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                case MSortOrderType.ORDER_CREATION_ASC:
                case MSortOrderType.ORDER_DEFAULT_ASC:
                case MSortOrderType.ORDER_MODIFICATION_ASC:
                case MSortOrderType.ORDER_SIZE_ASC:
                    this.ItemCollection.CurrentOrderDirection = SortOrderDirection.ORDER_ASCENDING;
                    break;

                case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                case MSortOrderType.ORDER_CREATION_DESC:
                case MSortOrderType.ORDER_DEFAULT_DESC:
                case MSortOrderType.ORDER_MODIFICATION_DESC:
                case MSortOrderType.ORDER_SIZE_DESC:
                    this.ItemCollection.CurrentOrderDirection = SortOrderDirection.ORDER_DESCENDING;
                    break;
            }
        }

        #endregion


        #region Properties

        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetField(ref _isLoaded, value); }
        }

        private string _emptyStateHeaderText;
        public string EmptyStateHeaderText
        {
            get { return _emptyStateHeaderText; }
            set { SetField(ref _emptyStateHeaderText, value); }
        }

        private string _emptyStateSubHeaderText;
        public string EmptyStateSubHeaderText
        {
            get { return _emptyStateSubHeaderText; }
            set { SetField(ref _emptyStateSubHeaderText, value); }
        }

        private IMegaNode _focusedNode;
        public IMegaNode FocusedNode
        {
            get { return _focusedNode; }
            set { SetField(ref _focusedNode, value); }
        }

        public BreadCrumbViewModel BreadCrumb { get; }

        public CollectionViewModel<IMegaNode> ItemCollection { get; }

        public bool HasBreadCrumbPath => this.BreadCrumb?.Items?.Count > 0;

        /// <summary>
        /// Property needed to store the selected nodes in a move/copy action 
        /// </summary>
        private List<IMegaNode> _copyOrMoveSelectedNodes;
        public List<IMegaNode> CopyOrMoveSelectedNodes
        {
            get { return _copyOrMoveSelectedNodes; }
            set { SetField(ref _copyOrMoveSelectedNodes, value); }
        }

        private FolderContentViewState _currentViewState;
        public FolderContentViewState CurrentViewState
        {
            get { return _currentViewState; }
            set
            {
                SetField(ref _currentViewState, value);
                OnPropertyChanged("IsFlyoutActionAvailable");
            }
        }

        private FolderContentViewState _previousViewState;
        public FolderContentViewState PreviousViewState
        {
            get { return _previousViewState; }
            set { SetField(ref _previousViewState, value); }
        }

        public bool IsFlyoutActionAvailable => this.CurrentViewState != FolderContentViewState.CopyOrMove;        

        public bool IsEmpty => !this.ItemCollection.HasItems;

        public ContainerType Type { get; private set; }

        private FolderContentViewMode _viewMode;
        public FolderContentViewMode ViewMode
        {
            get { return _viewMode; }
            set
            {
                SetField(ref _viewMode, value);
                OnPropertyChanged("IsListViewMode");
                OnPropertyChanged("IsGridViewMode");
            }
        }

        public bool IsListViewMode => this.ViewMode == FolderContentViewMode.ListView;
        public bool IsGridViewMode => this.ViewMode == FolderContentViewMode.GridView;

        private IMegaNode _folderRootNode;
        public IMegaNode FolderRootNode
        {
            get { return _folderRootNode; }
            set { SetField(ref _folderRootNode, value); }
        }

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        protected CancellationToken LoadingCancelToken { get; set; }

        private string _nextViewButtonPathData;
        public string NextViewButtonPathData
        {
            get { return _nextViewButtonPathData; }
            set { SetField(ref _nextViewButtonPathData, value); }
        }

        private string _nextViewButtonLabelText;
        public string NextViewButtonLabelText
        {
            get { return _nextViewButtonLabelText; }
            set { SetField(ref _nextViewButtonLabelText, value); }
        }

        private DataTemplateSelector _nodeTemplateSelector;
        public DataTemplateSelector NodeTemplateSelector
        {
            get { return _nodeTemplateSelector; }
            private set { SetField(ref _nodeTemplateSelector, value); }
        }

        private bool _isNodeDetailsViewVisible;
        public bool IsNodeDetailsViewVisible
        {
            get { return _isNodeDetailsViewVisible; }
            set { SetField(ref _isNodeDetailsViewVisible, value); }
        }

        private bool _isMultiSelectActive;
        public bool IsMultiSelectActive
        {
            get { return _isMultiSelectActive || this.ItemCollection.MoreThanOneSelected; }
            set
            {
                if (!SetField(ref _isMultiSelectActive, value)) return;

                if (_isMultiSelectActive)
                {
                    if (this.CurrentViewState != FolderContentViewState.MultiSelect)
                        this.PreviousViewState = this.CurrentViewState;

                    this.CurrentViewState = FolderContentViewState.MultiSelect;
                    EnableMultiSelect?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    if (this.PreviousViewState != FolderContentViewState.MultiSelect)
                    {
                        this.CurrentViewState = this.PreviousViewState;
                        this.PreviousViewState = FolderContentViewState.MultiSelect;
                    }
                        
                    this.ItemCollection.ClearSelection();
                    DisableMultiSelect?.Invoke(this, EventArgs.Empty);                    
                }
            }
        }

        private string _emptyInformationText;
        public string EmptyInformationText
        {
            get { return _emptyInformationText; }
            private set { SetField(ref _emptyInformationText, value); }
        }

        private string _busyText;
        public string BusyText
        {
            get { return _busyText; }
            private set
            {
                SetField(ref _busyText, value);
                this.HasBusyText = !string.IsNullOrEmpty(_busyText) && !string.IsNullOrWhiteSpace(_busyText);
            }
        }

        private bool _hasBusyText;
        public bool HasBusyText
        {
            get { return _hasBusyText; }
            private set { SetField(ref _hasBusyText, value); }
        }

        #endregion

        #region UiResources

        public string AddFolderText => ResourceService.UiResources.GetString("UI_NewFolder");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string CloseText => ResourceService.UiResources.GetString("UI_Close");
        public string CopyOrMoveText => CopyText + "/" + MoveText.ToLower();
        public string CopyText => ResourceService.UiResources.GetString("UI_Copy");
        public string DeselectAllText => ResourceService.UiResources.GetString("UI_DeselectAll");
        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string GridViewText => ResourceService.UiResources.GetString("UI_GridView");
        public string ListViewText => ResourceService.UiResources.GetString("UI_ListView");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string MoveText => ResourceService.UiResources.GetString("UI_Move");
        public string RefreshText => ResourceService.UiResources.GetString("UI_Refresh");
        public string RemoveText => ResourceService.UiResources.GetString("UI_Remove");
        public string RenameText => ResourceService.UiResources.GetString("UI_Rename");
        public string SelectAllText => ResourceService.UiResources.GetString("UI_SelectAll");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");
        public string UploadText => ResourceService.UiResources.GetString("UI_Upload");

        #endregion

        #region VisualResources

        public string AddFolderPathData => ResourceService.VisualResources.GetString("VR_CreateFolderPathData");
        public string BreadcrumbHomeMegaIcon => ResourceService.VisualResources.GetString("VR_BreadcrumbHomeMegaIcon");
        public string BreadcrumbHomeCloudDriveIcon => ResourceService.VisualResources.GetString("VR_MenuCloudPathData");
        public string BreadcrumbHomeRubbishBinIcon => ResourceService.VisualResources.GetString("VR_BreadcrumbHomeRubbishBinIcon");
        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string CopyOrMovePathData => ResourceService.VisualResources.GetString("VR_CopyOrMovePathData");
        public string CopyPathData => ResourceService.VisualResources.GetString("VR_CopyPathData");
        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");
        public string RubbishBinPathData => ResourceService.VisualResources.GetString("VR_RubbishBinPathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");
        public string UploadPathData => ResourceService.VisualResources.GetString("VR_UploadPathData");

        #endregion
        
    }
}
