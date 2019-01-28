using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
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
    public class FolderViewModel : BaseFolderViewModel
    {
        public FolderViewModel(MegaSDK megaSdk, ContainerType containerType, bool isForSelectFolder = false) 
            : base(megaSdk, containerType, isForSelectFolder)
        {
            this.VisiblePanel = PanelType.None;

            this.ItemCollection.SelectedItemsCollectionChanged += OnSelectedItemsCollectionChanged;

            this.AddFolderCommand = new RelayCommand(AddFolder);
            this.CopyOrMoveCommand = new RelayCommand(CopyOrMove);
            this.DownloadCommand = new RelayCommand(Download);
            this.GetLinkCommand = new RelayCommandAsync<bool>(GetLinkAsync);
            this.RemoveLinkCommand = new RelayCommandAsync<bool>(RemoveLinkAsync);
            this.RemoveCommand = new RelayCommand(Remove);
            this.RenameCommand = new RelayCommand(Rename);
            this.RestoreCommand = new RelayCommand(Restore);
            this.UploadCommand = new RelayCommand(Upload);
            this.ShareCommand = new RelayCommand(Share);
            this.ImportCommand = new RelayCommand(Import);

            SetEmptyContent(true);
            SetViewDefaults();
        }

        #region Events

        public event EventHandler ChildNodesCollectionChanged;

        /// <summary>
        /// Event triggered when a copy/move/import action over selected nodes is started
        /// </summary>
        public event EventHandler SelectedNodesActionStarted;

        /// <summary>
        /// Event invocator method called when a copy/move/import action over selected nodes is started
        /// </summary>
        protected virtual void OnSelectedNodesActionStarted()
        {
            this.SelectedNodesActionStarted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when a copy/move/import action over selected nodes is canceled
        /// </summary>
        public event EventHandler SelectedNodesActionCanceled;

        /// <summary>
        /// Event invocator method called when a copy/move/import action over selected nodes is canceled
        /// </summary>
        protected virtual void OnSelectedNodesActionCanceled()
        {
            this.SelectedNodesActionCanceled?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ShareEvent;

        #endregion

        private void OnSelectedItemsCollectionChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(this.Folder));
        }

        public override void ClosePanels()
        {
            if (this.VisiblePanel == PanelType.CopyMoveImport)
                this.OnSelectedNodesActionCanceled();

            base.ClosePanels();
        }

        /// <summary>
        /// Start and manage sharing of a folder in MEGA
        /// </summary>
        private void Share()
        {
            ShareEvent?.Invoke(this, EventArgs.Empty);
            this.VisiblePanel = PanelType.Information;
        }

        /// <summary>
        /// Method that should be called when a node is added or updated
        /// </summary>
        /// <param name="sender">Object that sent the notification</param>
        /// <param name="mNode">Node added or updated</param>
        public void OnNodeAdded(object sender, MNode mNode)
        {
            if (mNode == null) return;

            var isProcessed = false;

            var parentNode = this.MegaSdk.getParentNode(mNode);

            var nodeToUpdateInView = (IMegaNode)ItemCollection.Items.FirstOrDefault(
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

                var nodeToUpdateInView = (IMegaNode)ItemCollection.Items.FirstOrDefault(
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

        public void OnOutSharedFolderUpdated(object sender, MNode mNode)
        {
            var nodeToUpdateInView = (IMegaNode)ItemCollection.Items.FirstOrDefault(
                node => node.Base64Handle.Equals(mNode.getBase64Handle()));

            if (nodeToUpdateInView == null) return;

            UiService.OnUiThread(() =>
            {
                try { nodeToUpdateInView.Update(mNode, true); }
                catch (Exception) { /* Dummy catch, supress possible exception */ }
            });
        }
        

        #region Commands

        public ICommand AddFolderCommand { get; private set; }
        public ICommand CopyOrMoveCommand { get; }        
        public ICommand DownloadCommand { get; private set; }
        public ICommand GetLinkCommand { get; }
        public ICommand RemoveLinkCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand RenameCommand { get; }
        public ICommand RestoreCommand { get; }
        public ICommand UploadCommand { get; }
        public ICommand ShareCommand { get; set; }
        public ICommand ImportCommand { get; }

        #endregion

        #region Public Methods

        public void SelectAll() => this.ItemCollection.SelectAll(true);
        public void DeselectAll() => this.ItemCollection.SelectAll(false);
        public void ClearChildNodes() => this.ItemCollection.Clear();

        /// <summary>
        /// Load the mega nodes for this specific folder using the Mega SDK
        /// </summary>

        public override async void LoadChildNodes()
        {
            // User must be online to perform this operation
            if ((this.Type != ContainerType.FolderLink) && !await IsUserOnlineAsync())
                return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // FolderRootNode should not be null
            if (this.FolderRootNode == null)
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_LoadNodesFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_LoadNodesFailed"));
                return;
            }

            SetProgressIndication(true);

            // Process is started so we can set the empty content template to loading already
            SetEmptyContent(true);

            GetCurrentOrderDirection();

            // Get the MNodes from the Mega SDK in the correct sorting order for the current folder
            MNodeList childList = this.Type == ContainerType.CameraUploads ?
                NodeService.GetFileChildren(this.MegaSdk, this.FolderRootNode) : 
                this.IsForSelectFolder ? 
                NodeService.GetFolderChildren(this.MegaSdk, this.FolderRootNode) :
                NodeService.GetChildren(this.MegaSdk, this.FolderRootNode);

            if (childList == null)
            {
                SetEmptyContent(false);

                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_LoadNodesFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_LoadNodesFailed"));

                return;
            }

            // Clear the child nodes to make a fresh start
            OnUiThread(this.ItemCollection.Clear);

            // Set the correct view. Do this after the childs are cleared to speed things up
            SetViewOnLoad();

            // Build the bread crumbs. Do this before loading the nodes so that the user can click on home
            OnUiThread(() => this.BreadCrumb.Create(this));

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
        /// Add a new sub-folder to the current folder
        /// </summary>
        private async void AddFolder()
        {
            if (!await IsUserOnlineAsync()) return;

            if (this.FolderRootNode == null)
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_CreateFolderFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_CreateFolderFailed"));
                });
                return;
            }

            await DialogService.ShowInputAsyncActionDialogAsync(
                ResourceService.UiResources.GetString("UI_NewFolder"),
                ResourceService.UiResources.GetString("UI_TypeFolderName"),
                async (string folderName) =>
                {
                    if (string.IsNullOrWhiteSpace(folderName))
                        return false;

                    if (SdkService.ExistsNodeByName(this.FolderRootNode.OriginalMNode, folderName, true))
                    {
                        DialogService.SetInputDialogWarningMessage(ResourceService.AppMessages.GetString("AM_FolderAlreadyExists"));
                        return false;
                    }

                    var createFolder = new CreateFolderRequestListenerAsync();
                    var result = await createFolder.ExecuteAsync(() =>
                        this.MegaSdk.createFolder(folderName, this.FolderRootNode.OriginalMNode, createFolder));

                    if (!result)
                        DialogService.SetInputDialogWarningMessage(ResourceService.AppMessages.GetString("AM_CreateFolderFailed"));

                    return result;
                });
        }

        private void CopyOrMove()
        {
            if (this.ItemCollection.SelectedItems == null ||
                !this.ItemCollection.HasSelectedItems) return;

            this.VisiblePanel = PanelType.CopyMoveImport;

            foreach (var node in this.ItemCollection.SelectedItems)
                if (node != null) node.DisplayMode = NodeDisplayMode.SelectedNode;

            SelectedNodesService.SelectedNodes = this.ItemCollection.SelectedItems.ToList();
            this.ItemCollection.IsMultiSelectActive = false;

            this.OnSelectedNodesActionStarted();
        }

        /// <summary>
        /// Reset the the selected nodes
        /// </summary>
        public void ResetSelectedNodes()
        {
            this.ItemCollection.ClearSelection();
            SelectedNodesService.ClearSelectedNodes();
            this.VisiblePanel = PanelType.None;
        }

        private async void Download()
        {
            if (!this.ItemCollection.HasSelectedItems) return;
            await MultipleDownloadAsync(this.ItemCollection.SelectedItems);
            this.ItemCollection.IsMultiSelectActive = false;
        }

        private async Task MultipleDownloadAsync(ICollection<IBaseNode> nodes)
        {
            if (nodes == null || nodes.Count < 1) return;

            var downloadFolder = await FolderService.SelectFolder();
            if (downloadFolder != null)
            {
                if(await TransferService.CheckExternalDownloadPathAsync(downloadFolder.Path))
                {
                    foreach (var n in nodes)
                    {
                        var node = n as IMegaNode;
                        if (node == null) continue;
                        node.Transfer.ExternalDownloadPath = downloadFolder.Path;
                        TransferService.MegaTransfers.Add(node.Transfer);
                        node.Transfer.StartTransfer();
                    }
                }
            }

            // If is a folder link, navigate to the Cloud Drive page
            if (this.Type == ContainerType.FolderLink)
            {
                OnUiThread(() =>
                {
                    NavigateService.Instance.Navigate(typeof(CloudDrivePage), false,
                        NavigationObject.Create(this.GetType()));
                });
            }
        }

        protected virtual async Task<bool> GetLinkAsync()
        {
            if (!(bool)this.ItemCollection?.OnlyOneSelectedItem) return false;
            return await (this.ItemCollection.FocusedItem as IMegaNode)?.GetLinkAsync();
        }

        protected virtual async Task<bool> RemoveLinkAsync()
        {
            if (!(bool)this.ItemCollection?.OnlyOneSelectedItem) return false;
            return await (this.ItemCollection.FocusedItem as IMegaNode)?.RemoveLinkAsync();
        }

        private void Import()
        {
            if (this.ItemCollection.SelectedItems == null ||
                !this.ItemCollection.HasSelectedItems) return;

            this.VisiblePanel = PanelType.CopyMoveImport;

            foreach (var node in this.ItemCollection.SelectedItems)
                if (node != null) node.DisplayMode = NodeDisplayMode.SelectedNode;

            SelectedNodesService.SelectedNodes = this.ItemCollection.SelectedItems.ToList();
            this.ItemCollection.IsMultiSelectActive = false;

            this.OnSelectedNodesActionStarted();
        }

        public void ImportFolder()
        {
            this.VisiblePanel = PanelType.CopyMoveImport;

            SelectedNodesService.SelectedNodes.Clear();
            SelectedNodesService.SelectedNodes.Add(this.FolderRootNode);
            this.ItemCollection.IsMultiSelectActive = false;

            this.OnSelectedNodesActionStarted();
        }

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

            this.ItemCollection.IsMultiSelectActive = false;
        }

        private async void MultipleRemoveAsync(ICollection<IBaseNode> nodes)
        {
            if (nodes == null || nodes.Count < 1) return;
            
            bool result = true;
            foreach (var n in nodes)
            {
                var node = n as IMegaNode;
                if (node == null) continue;
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

            var selectedNode = this.ItemCollection.SelectedItems.First() as IMegaNode;
            if (selectedNode == null) return;
            await selectedNode.RenameAsync();
        }

        private void Restore()
        {
            if (this.Type != ContainerType.RubbishBin || !this.ItemCollection.HasSelectedItems) return;
            
            // Use a temp variable to avoid InvalidOperationException
            MultipleRestore(this.ItemCollection.SelectedItems.ToList());

            this.ItemCollection.IsMultiSelectActive = false;
        }

        private async void MultipleRestore(ICollection<IBaseNode> nodes)
        {
            if (nodes == null || nodes.Count < 1) return;

            bool result = true;
            foreach (var n in nodes)
            {
                var node = n as IMegaNode;
                if (node == null) continue;
                result = result & (await node.MoveAsync(node.RestoreNode) == NodeActionResult.Succeeded);
            }

            if (!result)
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_RestoreFromRubbishBinFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_RestoreMultiFromRubbishBinFailed"));
            }
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
                    uploadTransfer = new TransferObjectModel(this.MegaSdk,
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
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING,
                        string.Format("Transfer (UPLOAD) failed: '{0}'", file.Name), e);

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
                        // Try to get the modified date of the original file
                        DateTime modifiedDate = new DateTime();
                        await Task.Run(() =>
                        {
                            try { modifiedDate = File.GetLastWriteTime(upload.Key.Path); }
                            catch (Exception e)
                            {
                                LogService.Log(MLogLevel.LOG_LEVEL_WARNING,
                                    string.Format("Error getting the date modified of '{0}'", upload.Key.Name), e);
                            }
                        });

                        // Copy the original file to the upload cache folder
                        using (var fs = new FileStream(Path.Combine(uploadDir, upload.Key.Name), FileMode.Create))
                        {
                            // Set buffersize to avoid copy failure of large files
                            var stream = await upload.Key.OpenStreamForReadAsync();
                            await stream.CopyToAsync(fs, 8192, uploadTransfer.PreparingUploadCancelToken.Token);
                            await fs.FlushAsync(uploadTransfer.PreparingUploadCancelToken.Token);
                        }

                        // If have the original modified date, try to set it in the file copied in the upload cache
                        if (!modifiedDate.Equals(default(DateTime)))
                        {
                            await Task.Run(() =>
                            {
                                try { File.SetLastWriteTime(Path.Combine(uploadDir, upload.Key.Name), modifiedDate); }
                                catch (Exception e)
                                {
                                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING,
                                        string.Format("Error setting the date modified of '{0}'", upload.Key.Name), e);
                                }
                            });
                        }

                        // Check by fingerprint if the node is already uploaded to MEGA
                        var fileToUploadAction = await UploadService.CheckFileToUpload(
                            await StorageFile.GetFileFromPathAsync(Path.Combine(uploadDir, upload.Key.Name)),
                            FolderRootNode.OriginalMNode);
                        if (fileToUploadAction == UploadService.FileToUploadAction.UPLOAD)
                        {
                            uploadTransfer.StartTransfer();
                        }
                        else
                        {
                            switch (fileToUploadAction)
                            {
                                case UploadService.FileToUploadAction.COPY:
                                    LogService.Log(MLogLevel.LOG_LEVEL_INFO, 
                                        string.Format("UPLOAD: Remote copy of node '{0}'", upload.Key.Name));
                                    break;

                                case UploadService.FileToUploadAction.COPY_AND_RENAME:
                                    LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                                        string.Format("UPLOAD: Remote copy and rename of node '{0}'", upload.Key.Name));
                                    break;

                                case UploadService.FileToUploadAction.SAME_FILE_IN_FOLDER:
                                    LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                                        string.Format("UPLOAD: '{0}' already exists in folder", upload.Key.Name));
                                    ToastService.ShowTextNotification(
                                        string.Format(ResourceService.AppMessages.GetString("AM_FileAlreadyUploaded"),
                                        upload.Key.Name));
                                    break;
                            }

                            uploadTransfer.TransferedBytes = uploadTransfer.TotalBytes;
                            uploadTransfer.TransferState = MTransferState.STATE_COMPLETED;
                            TransferService.MoveMegaTransferToCompleted(TransferService.MegaTransfers, uploadTransfer);
                        }
                    }
                    else
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                            string.Format("Transfer (UPLOAD) canceled: '{0}'", upload.Key.Name));
                        OnUiThread(() => uploadTransfer.TransferState = MTransferState.STATE_CANCELLED);
                    }
                }
                // If the upload is cancelled during the preparation process, 
                // changes the status and delete the corresponding temporary file
                catch (TaskCanceledException)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                        string.Format("Transfer (UPLOAD) canceled: '{0}'", upload.Key.Name));
                    FileService.DeleteFile(uploadTransfer.TransferPath);
                    OnUiThread(() => uploadTransfer.TransferState = MTransferState.STATE_CANCELLED);
                }
                catch (Exception e)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR,
                        string.Format("Transfer (UPLOAD) failed: '{0}'", upload.Key.Name), e);
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

        public override void OnChildNodeTapped(IBaseNode baseNode)
        {
            // Needed to avoid process the node when the user is in MultiSelect.
            if (this.ItemCollection.IsMultiSelectActive) return;
            if (!(baseNode is IMegaNode)) return;

            var node = baseNode as IMegaNode;
            switch (node.Type)
            {
                case MNodeType.TYPE_FILE:
                    ProcessFileNode(node);
                    break;

                case MNodeType.TYPE_FOLDER:
                    BrowseToFolder(node);
                    break;

                case MNodeType.TYPE_UNKNOWN:
                case MNodeType.TYPE_ROOT:
                case MNodeType.TYPE_INCOMING:
                case MNodeType.TYPE_RUBBISH:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        public override async void BrowseToHome()
        {
            if (this.FolderRootNode == null) return;

            ClosePanels();

            MNode homeNode = null;
            FolderViewModel homeFolder = null;

            switch (this.Type)
            {
                case ContainerType.CloudDrive:
                    homeNode = this.MegaSdk.getRootNode();
                    homeFolder = new FolderViewModel(this.MegaSdk, ContainerType.CloudDrive);
                    break;
                case ContainerType.RubbishBin:
                    homeNode = this.MegaSdk.getRubbishNode();
                    homeFolder = new FolderViewModel(this.MegaSdk, ContainerType.RubbishBin);
                    break;
                case ContainerType.CameraUploads:
                    homeNode = await SdkService.GetCameraUploadRootNodeAsync();
                    homeFolder = new FolderViewModel(this.MegaSdk, ContainerType.CameraUploads);
                    break;
            }

            if (homeNode == null) return;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, App.AppInformation, homeNode, homeFolder, this.ItemCollection.Items);
            OnFolderNavigatedTo();

            LoadChildNodes();
        }

        public async void ProcessFileNode(IMegaNode node)
        {
            // User must be online to perform this operation
            if (!await IsUserOnlineAsync()) return;

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

        /// <summary>
        /// Sets the view mode for the folder content.
        /// </summary>
        /// <param name="viewMode">View mode to set.</param>
        protected override void SetView(FolderContentViewMode viewMode)
        {
            switch (viewMode)
            {
                case FolderContentViewMode.GridView:
                    OnUiThread(() =>
                    {
                        this.NodeTemplateSelector = new NodeTemplateSelector()
                        {
                            FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeGridViewFileItemContent"],
                            FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeGridViewFolderItemContent"]
                        };
                    });
                    break;

                case FolderContentViewMode.ListView:
                    SetViewDefaults();
                    break;
            }

            base.SetView(viewMode);
        }

        /// <summary>
        /// Sets the default view mode for the folder content.
        /// </summary>
        protected override void SetViewDefaults()
        {
            OnUiThread(() =>
            {
                this.NodeTemplateSelector = new NodeTemplateSelector()
                {
                    FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListViewFileItemContent"],
                    FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListViewFolderItemContent"]
                };
            });            

            base.SetViewDefaults();
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

                var node = NodeService.CreateNew(this.MegaSdk, App.AppInformation, childList.get(i), this, this.ItemCollection.Items);

                // If node creation failed for some reason, continue with the rest and leave this one
                if (node == null) continue;

                // If the user is moving nodes, check if the node had been selected to move 
                // and establish the corresponding display mode
                if (this.VisiblePanel == PanelType.CopyMoveImport)
                {
                    // Check if it is one of the selected nodes
                    SelectedNodesService.IsSelectedNode(node, true);
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

        public FolderViewModel Folder => this;

        public new IMegaNode FocusedNode
        {
            get { return base.FocusedNode as IMegaNode; }
            set { base.FocusedNode = value; }
        }

        public new IMegaNode FolderRootNode
        {
            get { return base.FolderRootNode as IMegaNode; }
            set { base.FolderRootNode = value; }
        }

        public bool IsEmpty => !this.ItemCollection.HasItems;

        public override string OrderTypeAndNumberOfItems
        {
            get
            {
                if (this.FolderRootNode == null) return string.Empty;

                var megaSdk = this.Type == ContainerType.FolderLink ?
                    SdkService.MegaSdkFolderLinks : SdkService.MegaSdk;

                this.numChildFolders = megaSdk.getNumChildFolders(this.FolderRootNode.OriginalMNode);
                this.numChildFiles = megaSdk.getNumChildFiles(this.FolderRootNode.OriginalMNode);

                return base.OrderTypeAndNumberOfItems;
            }
        }

        #endregion
            
        #region UiResources

        public string AddFolderText => ResourceService.UiResources.GetString("UI_NewFolder");
        public string CopyOrMoveText => this is IncomingSharesViewModel ? CopyText : CopyText + "/" + MoveText;
        public string CopyText => ResourceService.UiResources.GetString("UI_Copy");
        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string ImportText => ResourceService.UiResources.GetString("UI_Import");
        public string MoveText => ResourceService.UiResources.GetString("UI_Move");
        public string RemoveText => ResourceService.UiResources.GetString("UI_Remove");
        public string RenameText => ResourceService.UiResources.GetString("UI_Rename");
        public string RestoreText => ResourceService.UiResources.GetString("UI_Restore");
        public string UploadText => ResourceService.UiResources.GetString("UI_Upload");

        #endregion

        #region VisualResources

        public string AddFolderPathData => ResourceService.VisualResources.GetString("VR_CreateFolderPathData");
        public string BreadcrumbHomeMegaIcon => ResourceService.VisualResources.GetString("VR_BreadcrumbHomeMegaIcon");
        public string BreadcrumbHomeCloudDriveIcon => ResourceService.VisualResources.GetString("VR_MenuCloudPathData");
        public string BreadcrumbHomeRubbishBinIcon => ResourceService.VisualResources.GetString("VR_BreadcrumbHomeRubbishBinIcon");
        public string CopyOrMovePathData => ResourceService.VisualResources.GetString("VR_CopyOrMovePathData");
        public string CopyPathData => ResourceService.VisualResources.GetString("VR_CopyPathData");
        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string ImportPathData => ResourceService.VisualResources.GetString("VR_ImportPathData");
        public string RubbishBinPathData => ResourceService.VisualResources.GetString("VR_RubbishBinPathData");
        public string UploadPathData => ResourceService.VisualResources.GetString("VR_UploadPathData");

        #endregion
        
    }
}
