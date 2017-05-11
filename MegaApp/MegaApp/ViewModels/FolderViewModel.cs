using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
            this.IsBusy = false;
            this.BusyText = null;
            this.ChildNodes = new ObservableCollection<IMegaNode>();
            this.BreadCrumbs = new ObservableCollection<IBaseNode>();
            this.SelectedNodes = new List<IMegaNode>();
            this.CopyOrMoveSelectedNodes = new List<IMegaNode>();

            this.AcceptCopyCommand = new RelayCommand(AcceptCopy);
            this.AcceptMoveCommand = new RelayCommand(AcceptMove);
            this.AddFolderCommand = new RelayCommand(AddFolder);
            this.CancelCopyOrMoveCommand = new RelayCommand(CancelCopyOrMove);
            this.ChangeViewCommand = new RelayCommand(ChangeView);
            this.CopyOrMoveCommand = new RelayCommand(CopyOrMove);
            this.DownloadCommand = new RelayCommand(Download);
            this.MultiSelectCommand = new RelayCommand(MultiSelect);
            this.HomeSelectedCommand = new RelayCommand(BrowseToHome);
            this.ItemSelectedCommand = new RelayCommand<BreadcrumbEventArgs>(ItemSelected);
            this.RefreshCommand = new RelayCommand(Refresh);
            this.RemoveCommand = new RelayCommand(Remove);
            this.UploadCommand = new RelayCommand(Upload);
            this.SelectionChangedCommand = new RelayCommand(SelectionChanged);
            this.OpenNodeDetailsCommand = new RelayCommand(OpenNodeDetails);
            this.CloseNodeDetailsCommand = new RelayCommand(CloseNodeDetails);

            //this.ImportItemCommand = new DelegateCommand(this.ImportItem);
            //this.CreateShortCutCommand = new DelegateCommand(this.CreateShortCut);            
            //this.GetLinkCommand = new DelegateCommand(this.GetLink);            

            this.ChildNodes.CollectionChanged += ChildNodesOnCollectionChanged;
            this.BreadCrumbs.CollectionChanged += BreadCrumbsOnCollectionChanged;

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
                default:
                    throw new ArgumentOutOfRangeException(nameof(containerType));
            }
        }

        private void SelectionChanged()
        {
            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
                this.IsMultiSelectActive = (this.IsMultiSelectActive && this.SelectedNodes.Count >= 1) || this.SelectedNodes.Count > 1;
            else
                this.IsMultiSelectActive = this.SelectedNodes.Count > 0;

            if(this.SelectedNodes?.Count > 0)
            {
                var focusedNode = (NodeViewModel)this.SelectedNodes.Last();
                if((focusedNode is ImageNodeViewModel) && (focusedNode as ImageNodeViewModel != null))
                    (focusedNode as ImageNodeViewModel).InViewingRange = true;

                this.FocusedNode = focusedNode;
            }
        }

        private void ChildNodesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                // Start a new task to avoid freeze the UI
                Task.Run(() =>
                {
                    foreach (var node in e.NewItems)
                        (node as NodeViewModel)?.SetThumbnailImage();
                });
            }

            this.ChildNodesCollectionChanged?.Invoke(this, EventArgs.Empty);

            OnPropertyChanged("IsEmpty");
        }

        public void OpenNodeDetails()
        {
            OpenNodeDetailsEvent?.Invoke(this, EventArgs.Empty);
        }

        public void CloseNodeDetails()
        {
            CloseNodeDetailsEvent?.Invoke(this, EventArgs.Empty);
        }

        #region Commands

        public ICommand AcceptCopyCommand { get; }
        public ICommand AcceptMoveCommand { get; }
        public ICommand AddFolderCommand { get; private set; }
        public ICommand CancelCopyOrMoveCommand { get; }
        public ICommand ChangeViewCommand { get; }
        public ICommand CopyOrMoveCommand { get; }        
        public ICommand DownloadCommand { get; private set; }
        public ICommand HomeSelectedCommand { get; }
        public ICommand ItemSelectedCommand { get; }
        public ICommand MultiSelectCommand { get; set; }
        public ICommand RefreshCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand UploadCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand OpenNodeDetailsCommand { get; private set; }
        public ICommand CloseNodeDetailsCommand { get; }

        //public ICommand GetLinkCommand { get; private set; }        
        //public ICommand ImportItemCommand { get; private set; }
        //public ICommand CreateShortCutCommand { get; private set; }        

        #endregion

        #region Public Methods

        public void SelectAll()
        {
            foreach (var childNode in this.ChildNodes)
            {
                childNode.IsMultiSelected = true;
            }
        }

        public void DeselectAll()
        {
            foreach (var childNode in this.ChildNodes)
            {
                childNode.IsMultiSelected = false;
            }
        }

        public void ClearChildNodes()
        {
            if (this.ChildNodes == null || !this.ChildNodes.Any()) return;

            OnUiThread(() => this.ChildNodes.Clear());
        }

        /// <summary>
        /// Load the mega nodes for this specific folder using the Mega SDK
        /// </summary>
        public async void LoadChildNodes()
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

            // Get the MNodes from the Mega SDK in the correct sorting order for the current folder
            MNodeList childList = NodeService.GetChildren(this.MegaSdk, this.FolderRootNode);

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
            ClearChildNodes();

            // Set the correct view. Do this after the childs are cleared to speed things up
            SetViewOnLoad();

            // Build the bread crumbs. Do this before loading the nodes so that the user can click on home
            OnUiThread(() => BuildBreadCrumbs());

            // Create the option to cancel
            CreateLoadCancelOption();

            // Load and create the childnodes for the folder
            await Task.Factory.StartNew(() =>
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
        public void CancelLoad()
        {
            if (this.LoadingCancelTokenSource != null && this.LoadingCancelToken.CanBeCanceled)
                this.LoadingCancelTokenSource.Cancel();
        }

        /// <summary>
        /// Refresh the current folder. Delete cached thumbnails and reload the nodes
        /// </summary>
        private void Refresh()
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            CloseNodeDetails();

            FileService.ClearFiles(
                NodeService.GetFiles(this.ChildNodes,
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
            if (this.SelectedNodes == null || !this.SelectedNodes.Any()) return;
            await MultipleDownloadAsync(this.SelectedNodes);
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

        /// <summary>
        /// Sets if multiselect is active or not.
        /// </summary>
        private void MultiSelect() => this.IsMultiSelectActive = !this.IsMultiSelectActive;

        private async void Remove()
        {
            if (this.SelectedNodes == null || !this.SelectedNodes.Any()) return;

            int count = this.SelectedNodes.Count;

            string title, message;
            switch(this.Type)
            {
                case ContainerType.CloudDrive:
                    title = ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion_Title");
                    message = string.Format(ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion"), count);
                    break;

                case ContainerType.RubbishBin:
                    title = ResourceService.AppMessages.GetString("AM_MultiSelectRemoveQuestion_Title");
                    message = string.Format(ResourceService.AppMessages.GetString("AM_MultiSelectRemoveQuestion"), count);
                    break;

                default:
                    return;
            }

            var result = await DialogService.ShowOkCancelAsync(title, message);

            if (!result) return;

            MultipleRemoveAsync(this.SelectedNodes.ToList());

            this.IsMultiSelectActive = false;
        }

        private void MultipleRemoveAsync(ICollection<IMegaNode> nodes)
        {
            if (nodes == null || nodes.Count < 1) return;

            Task.Run(async () =>
            {
                bool result = true;
                foreach (var node in nodes)
                {
                    result = result & await node.RemoveAsync(true);
                }

                if(!result)
                {
                    string title, message;
                    switch (this.Type)
                    {
                        case ContainerType.CloudDrive:
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
            });
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
                        uploadTransfer.PreparingUploadCancelToken = new CancellationTokenSource();
                        uploadTransfer.TransferState = MTransferState.STATE_NONE;
                        uploads.Add(file, uploadTransfer);
                        TransferService.MegaTransfers.Add(uploadTransfer);
                    }
                }
                catch (Exception)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Transfer (UPLOAD) failed: " + file.Name);

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
                catch (Exception)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Transfer (UPLOAD) failed: " + upload.Key.Name);
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
            if (this.CurrentViewState == FolderContentViewState.MultiSelect) return;
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

        public void SetEmptyContent(bool isLoading)
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

                    case ContainerType.InShares:
                    case ContainerType.OutShares:
                        OnUiThread(() =>
                        {
                            this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptySharedFolders").ToLower();
                        });
                        break;

                    case ContainerType.ContactInShares:
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

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, App.AppInformation, parentNode, this, this.ChildNodes);

            LoadChildNodes();

            return true;
        }

        private void ItemSelected(BreadcrumbEventArgs e)
        {
            BrowseToFolder((IMegaNode)e.Item);
        }

        public virtual void BrowseToHome()
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
            }

            if (homeNode == null) return;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, App.AppInformation, homeNode, homeFolder);
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

            for (int i = 0; i < listSize; i++)
            {
                // If the task has been cancelled, stop processing
                if (this.LoadingCancelToken.IsCancellationRequested)
                    this.LoadingCancelToken.ThrowIfCancellationRequested();

                // To avoid pass null values to CreateNew
                if (childList.get(i) == null) continue;

                var node = NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, childList.get(i), this, this.ChildNodes);

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
                    OnUiThread(() =>
                    {
                        // If the task has been cancelled, stop processing
                        foreach (var megaNode in helperList.TakeWhile(megaNode => !this.LoadingCancelToken.IsCancellationRequested))
                            this.ChildNodes.Add(megaNode);
                    });

                    helperList.Clear();
                    continue;
                }

                if (helperList.Count != backgroundItemCount || i <= viewportItemCount) continue;

                // Add the rest of the items in the background to the list
                OnUiThread(() =>
                {
                    // If the task has been cancelled, stop processing
                    foreach (var megaNode in helperList.TakeWhile(megaNode => !this.LoadingCancelToken.IsCancellationRequested))
                        this.ChildNodes.Add(megaNode);
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
                    this.ChildNodes.Add(megaNode);

                OnPropertyChanged("IsEmpty");
            });
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

        private void CreateLoadCancelOption()
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
        /// Sets the default view mode for the folder conten.
        /// </summary>
        private void SetViewDefaults()
        {
            this.NodeTemplateSelector = new NodeTemplateSelector()
            {
                FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListViewFileItemContent"],
                FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListViewFolderItemContent"]
            };

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
                    SetView(FolderContentViewMode.GridView);
                    UiService.SetViewMode(this.FolderRootNode.Base64Handle, FolderContentViewMode.GridView);
                    break;

                case FolderContentViewMode.GridView:
                    SetView(FolderContentViewMode.ListView);
                    UiService.SetViewMode(this.FolderRootNode.Base64Handle, FolderContentViewMode.ListView);
                    break;
            }

            this.ChangeViewEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the view mode for the folder content.
        /// </summary>
        /// <param name="viewMode">View mode to set.</param>
        public void SetView(FolderContentViewMode viewMode)
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

        public void BuildBreadCrumbs()
        {
            this.BreadCrumbs.Clear();

            // Top root nodes have no breadcrumbs
            if (this.FolderRootNode == null || 
                this.FolderRootNode.Type == MNodeType.TYPE_ROOT || 
                this.FolderRootNode.Type == MNodeType.TYPE_RUBBISH) return;

            this.BreadCrumbs.Add(this.FolderRootNode);

            MNode parentNode = this.MegaSdk.getParentNode(this.FolderRootNode.OriginalMNode);
            while ((parentNode != null) && (parentNode.getType() != MNodeType.TYPE_ROOT) &&
                (parentNode.getType() != MNodeType.TYPE_RUBBISH))
            {
                this.BreadCrumbs.Insert(0, NodeService.CreateNew(this.MegaSdk, App.AppInformation, parentNode, this));
                parentNode = this.MegaSdk.getParentNode(parentNode);
            }
        }

        protected virtual void OnFolderNavigatedTo()
        {
            FolderNavigatedTo?.Invoke(this, EventArgs.Empty);
        }

        private void BreadCrumbsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.FolderRootNode == null) return;

            string folderName = string.Empty;
            switch (this.FolderRootNode.Type)
            {
                case MNodeType.TYPE_ROOT:
                    folderName = ResourceService.UiResources.GetString("UI_CloudDriveName");
                    break;

                case MNodeType.TYPE_RUBBISH:
                    folderName = ResourceService.UiResources.GetString("UI_RubbishBinName");
                    break;

                case MNodeType.TYPE_FOLDER:
                    folderName = this.FolderRootNode.Name;
                    break;
            }

            this.ImportLinkBorderText = string.Format(ResourceService.UiResources.GetString("UI_ImportLinkBorderText"), folderName);
        }

        #endregion

        #region IBreadCrumb

        private ObservableCollection<IBaseNode> _breadCrumbs;
        public ObservableCollection<IBaseNode> BreadCrumbs
        {
            get { return _breadCrumbs; }
            set { SetField(ref _breadCrumbs, value); }
        }

        #endregion

        #region Properties

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

        private List<IMegaNode> _selectedNodes;
        public List<IMegaNode> SelectedNodes
        {
            get { return _selectedNodes; }
            set { SetField(ref _selectedNodes, value); }
        }

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

        private ObservableCollection<IMegaNode> _childNodes;
        public ObservableCollection<IMegaNode> ChildNodes
        {
            get { return _childNodes; }
            set { SetField(ref _childNodes, value); }
        }

        public bool IsEmpty => (this.ChildNodes.Count == 0);

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
        private CancellationToken LoadingCancelToken { get; set; }

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
            get { return _isMultiSelectActive || _selectedNodes.Count > 1; }
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
                        
                    SelectedNodes.Clear();
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

        /// <summary>
        /// Property needed show a dynamic import text.
        /// </summary>        
        private string _importLinkBorderText;
        public string ImportLinkBorderText
        {
            get { return _importLinkBorderText; }
            private set { SetField(ref _importLinkBorderText, value); }
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
