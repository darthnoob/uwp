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

namespace MegaApp.ViewModels
{
    /// <summary>
    /// Class that handles all process and operations of a section that contains MEGA nodes
    /// </summary>
    public class FolderViewModel : BaseSdkViewModel
    {
        public event EventHandler FolderNavigatedTo;

        public FolderViewModel(ContainerType containerType)
        {
            this.Type = containerType;

            this.FolderRootNode = null;
            this.IsBusy = false;
            this.BusyText = null;
            this.ChildNodes = new ObservableCollection<IMegaNode>();
            this.BreadCrumbs = new ObservableCollection<IBaseNode>();
            this.SelectedNodes = new List<IMegaNode>();
            this.IsMultiSelectActive = false;

            this.AddFolderCommand = new RelayCommand(AddFolder);
            this.ChangeViewCommand = new RelayCommand(ChangeView);
            this.CleanRubbishBinCommand = new RelayCommand(CleanRubbishBin);
            this.DownloadCommand = new RelayCommand(Download);
            this.MoveToRubbishBinCommand = new RelayCommand(MoveToRubbishBin);
            this.MultiSelectCommand = new RelayCommand(MultiSelect);
            this.HomeSelectedCommand = new RelayCommand(BrowseToHome);
            this.ItemSelectedCommand = new RelayCommand<BreadcrumbEventArgs>(ItemSelected);
            this.RefreshCommand = new RelayCommand(Refresh);
            this.RemoveCommand = new RelayCommand(Remove);
            this.RenameCommand = new RelayCommand(Rename);
            this.UploadCommand = new RelayCommand(Upload);

            //this.ImportItemCommand = new DelegateCommand(this.ImportItem);
            //this.CreateShortCutCommand = new DelegateCommand(this.CreateShortCut);            
            //this.GetLinkCommand = new DelegateCommand(this.GetLink);            
            //this.ViewDetailsCommand = new DelegateCommand(this.ViewDetails);

            this.ChildNodes.CollectionChanged += ChildNodesOnCollectionChanged;
            this.BreadCrumbs.CollectionChanged += BreadCrumbsOnCollectionChanged;

            SetViewDefaults();

            SetEmptyContentTemplate(true);

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

        private void ChildNodesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
            {
                foreach (var node in e.NewItems)
                    (node as NodeViewModel)?.SetThumbnailImage();
            }

            OnPropertyChanged("HasChildNodesBinding");
        }

        #region Commands

        public ICommand AddFolderCommand { get; private set; }
        public ICommand ChangeViewCommand { get; private set; }
        public ICommand CleanRubbishBinCommand { get; private set; }
        public ICommand DownloadCommand { get; private set; }
        public ICommand HomeSelectedCommand { get; private set; }
        public ICommand ItemSelectedCommand { get; private set; }
        public ICommand MoveToRubbishBinCommand { get; private set; }
        public ICommand MultiSelectCommand { get; set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand RemoveCommand { get; private set; }
        public ICommand RenameCommand { get; private set; }
        public ICommand UploadCommand { get; private set; }
        
        //public ICommand GetLinkCommand { get; private set; }        
        //public ICommand ImportItemCommand { get; private set; }
        //public ICommand CreateShortCutCommand { get; private set; }        
        //public ICommand ViewDetailsCommand { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns boolean value to indicatie if the current folder view has any child nodes
        /// </summary>
        /// <returns>True if there are child nodes, False if child node count is zero</returns>
        public bool HasChildNodes() => this.ChildNodes.Count > 0;

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

        public async void ClearChildNodes()
        {
            if (this.ChildNodes == null || !this.ChildNodes.Any()) return;

            await OnUiThread(() => this.ChildNodes.Clear());
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
            SetEmptyContentTemplate(true);

            // Get the MNodes from the Mega SDK in the correct sorting order for the current folder
            MNodeList childList = NodeService.GetChildren(this.MegaSdk, this.FolderRootNode);

            if (childList == null)
            {
                new CustomMessageDialog(
                    ResourceService.AppMessages.GetString("AM_LoadNodesFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_LoadNodesFailed"),
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();

                SetEmptyContentTemplate(false);

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
                            App.AppInformation, this.MegaSdk.getRubbishNode(), this.Type);
                        break;

                    case ContainerType.CloudDrive:
                    case ContainerType.FolderLink:
                        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, 
                            App.AppInformation, this.MegaSdk.getRootNode(), this.Type);
                        break;
                }
            }

            LoadChildNodes();
        }

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
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_CreateFolderFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_CreateFolderFailed"));
                return;
            }

            var createFolder = new CreateFolderRequestListenerAsync();
            var result = await createFolder.ExecuteAsync(() =>
            {
                this.MegaSdk.createFolder(folderName, this.FolderRootNode.OriginalMNode, createFolder);
            });

            if (result)
            {
                ToastService.ShowText(string.Format(
                    ResourceService.AppMessages.GetString("AM_CreateFolderSuccess"),
                    folderName));
                return;
            };

            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_CreateFolderFailed_Title"),
                ResourceService.AppMessages.GetString("AM_CreateFolderFailed"));
        }

        private void CleanRubbishBin()
        {
            if (this.Type != ContainerType.RubbishBin || this.ChildNodes.Count < 1) return;

            var customMessageDialog = new CustomMessageDialog(
                ResourceService.AppMessages.GetString("AM_CleanRubbishBin_Title"),
                ResourceService.AppMessages.GetString("AM_CleanRubbishBinQuestion"),
                App.AppInformation,
                MessageDialogButtons.OkCancel);

            customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
            {
                this.MegaSdk.cleanRubbishBin(new CleanRubbishBinRequestListener());
            };

            customMessageDialog.ShowDialog();
        }

        private void Download()
        {
            if (this.SelectedNodes == null || !this.SelectedNodes.Any())
            {
                this.FocusedNode?.Download(TransferService.MegaTransfers);
                return;
            };
            
            MultipleDownloadAsync(this.SelectedNodes);
        }

        private async void MultipleDownloadAsync(ICollection<IMegaNode> nodes)
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

            this.IsMultiSelectActive = false;
        }

        private async void MoveToRubbishBin()
        {
            if (this.SelectedNodes == null || !this.SelectedNodes.Any())
            {
                if(this.FocusedNode == null) return;
                await this.FocusedNode.MoveToRubbishBinAsync();
                return;
            };

            int count = this.SelectedNodes.Count;

            var result = await DialogService.ShowOkCancelAsync(
               ResourceService.AppMessages.GetString("AM_MoveToRubbishBinQuestion_Title"),
               string.Format(ResourceService.AppMessages.GetString("AM_MultiMoveToRubbishBinQuestion"), count));

            if (!result) return;

            MultipleMoveToRubbishBin(this.SelectedNodes);
        }

        private void MultipleMoveToRubbishBin(ICollection<IMegaNode> nodes)
        {
            if (nodes == null || nodes.Count < 1) return;

            Task.Run(async () =>
            {
                foreach (var node in this.SelectedNodes)
                {
                    await node.MoveToRubbishBinAsync(true);
                }

                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_MultiMoveToRubbishBinSucces_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_MultiMoveToRubbishBinSucces"), nodes.Count));
              
                this.IsMultiSelectActive = false;
            });
        }        

        /// <summary>
        /// Sets if multiselect is active or not.
        /// </summary>
        private void MultiSelect()
        {
            this.IsMultiSelectActive = !this.IsMultiSelectActive;
        }

        private async void Remove()
        {
            if (this.SelectedNodes?.Count > 1)
                await MultipleRemoveAsync();
            else if (this.SelectedNodes?.Count == 1)
                await this.SelectedNodes?.First()?.RemoveAsync();
            else
                await this.FocusedNode?.RemoveAsync();
        }

        private async Task MultipleRemoveAsync()
        {
            int count = this.SelectedNodes.Count;

            if (count < 1) return;

            var customMessageDialog = new CustomMessageDialog(
                ResourceService.AppMessages.GetString("AM_MultiSelectRemoveQuestion_Title"),
                string.Format(ResourceService.AppMessages.GetString("AM_MultiSelectRemoveQuestion"), count),
                App.AppInformation,
                MessageDialogButtons.OkCancel);

            customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
            {
                Task.Run(async () =>
                {
                    WaitHandle[] waitEventRequests = new WaitHandle[count];

                    int index = 0;

                    foreach (var node in this.SelectedNodes)
                    {
                        waitEventRequests[index] = new AutoResetEvent(false);
                        await node.RemoveAsync(true, (AutoResetEvent)waitEventRequests[index]);
                        index++;
                    }

                    WaitHandle.WaitAll(waitEventRequests);

                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_MultiRemoveSucces_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_MultiRemoveSucces"), count),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();

                    this.IsMultiSelectActive = false;
                });
            };

            await customMessageDialog.ShowDialogAsync();
        }

        /// <summary>
        /// Renames the focused node.
        /// </summary>
        private void Rename()
        {
            this.FocusedNode?.RenameAsync();
        }

        /// <summary>
        /// Select files for upload to cloud
        /// </summary>
        private async void Upload()
        {
            // Set upload directory only once for speed improvement and if not exists, create dir
            var uploadDir = AppService.GetUploadDirectoryPath(true);

            var pickedFiles = await FileService.SelectMultipleFiles();
            foreach (StorageFile file in pickedFiles)
            {
                if (file == null) continue; // To avoid null references

                try
                {
                    string tempUploadFilePath = Path.Combine(uploadDir, file.Name);
                    using (var fs = new FileStream(tempUploadFilePath, FileMode.Create))
                    {
                        // Set buffersize to avoid copy failure of large files
                        var stream = await file.OpenStreamForReadAsync();
                        await stream.CopyToAsync(fs, 8192);
                        await fs.FlushAsync();
                    }

                    var uploadTransfer = new TransferObjectModel(
                        this.FolderRootNode,
                        TransferType.Upload, 
                        tempUploadFilePath);

                    TransferService.MegaTransfers.Add(uploadTransfer);
                    uploadTransfer.StartTransfer();
                }
                catch (Exception)
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_PrepareFileForUploadFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_PrepareFileForUploadFailed"), file.Name));
                }
            }
        }

        public void OnChildNodeTapped(IMegaNode node)
        {
            switch (node.Type)
            {
                case MNodeType.TYPE_UNKNOWN:
                    break;
                case MNodeType.TYPE_FILE:
                    // If the user is moving nodes don't process the file node
                    if (this.CurrentViewState != FolderContentViewState.CopyOrMoveItem)
                        ProcessFileNode(node);
                    break;
                case MNodeType.TYPE_FOLDER:
                    // If the user is moving nodes and the folder is one of the selected nodes don't navigate to it
                    if ((this.CurrentViewState == FolderContentViewState.CopyOrMoveItem) && (IsSelectedNode(node))) return;
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
        private bool IsSelectedNode(IMegaNode node)
        {
            if ((this.SelectedNodes != null) && (this.SelectedNodes.Count > 0))
            {
                for (int index = 0; index < this.SelectedNodes.Count; index++)
                {
                    var selectedNode = this.SelectedNodes[index];
                    if ((selectedNode != null) && (node.OriginalMNode.getBase64Handle() == selectedNode.OriginalMNode.getBase64Handle()))
                    {
                        //Update the selected nodes list values
                        node.DisplayMode = NodeDisplayMode.SelectedForCopyOrMove;
                        this.SelectedNodes[index] = node;

                        return true;
                    }
                }
            }

            return false;
        }

        public void SetEmptyContentTemplate(bool isLoading)
        {
            if (isLoading)
            {
                OnUiThread(() =>
                {
                    //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListLoadingContent"];
                    this.EmptyInformationText = "";
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
                                //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListCloudDriveEmptyContent"];
                                this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyCloudDrive").ToLower();
                            });
                        }
                        else if (this.FolderRootNode != null && megaRubbishBin != null && this.FolderRootNode.Base64Handle.Equals(megaRubbishBin.getBase64Handle()))
                        {
                            OnUiThread(() =>
                            {
                                //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListRubbishBinEmptyContent"];
                                this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyRubbishBin").ToLower();
                            });
                        }
                        else
                        {
                            OnUiThread(() =>
                            {
                                //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListEmptyContent"];
                                this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyFolder").ToLower();
                            });
                        }
                        break;

                    case ContainerType.InShares:
                    case ContainerType.OutShares:
                        OnUiThread(() =>
                        {
                            //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaSharedFoldersListEmptyContent"];
                            this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptySharedFolders").ToLower();
                        });
                        break;

                    case ContainerType.ContactInShares:
                        break;

                    case ContainerType.Offline:
                        OnUiThread(() =>
                        {
                            //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListRubbishBinEmptyContent"];
                            this.EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyOffline").ToLower();
                        });
                        break;

                    case ContainerType.FolderLink:
                        OnUiThread(() =>
                        {
                            //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListEmptyContent"];
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

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, App.AppInformation, parentNode, this.Type, this.ChildNodes);

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

            MNode homeNode = null;

            switch (this.Type)
            {
                case ContainerType.CloudDrive:
                    homeNode = this.MegaSdk.getRootNode();
                    break;
                case ContainerType.RubbishBin:
                    homeNode = this.MegaSdk.getRubbishNode();
                    break;
            }

            if (homeNode == null) return;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, App.AppInformation, homeNode, this.Type, this.ChildNodes);
            OnFolderNavigatedTo();

            LoadChildNodes();
        }

        public void BrowseToFolder(IMegaNode node)
        {
            if (node == null) return;

            // Show the back button in desktop and tablet applications
            // Back button in mobile applications is automatic in the nav bar on screen
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            this.FolderRootNode = node;
            OnFolderNavigatedTo();

            LoadChildNodes();
        }

        public void ProcessFileNode(IMegaNode node)
        {

        }

        public void SetProgressIndication(bool onOff, string busyText = null)
        {
            OnUiThread(() =>
            {
                this.IsBusy = onOff;
                this.BusyText = busyText;
            });
        }

        private async Task CreateChildren(MNodeList childList, int listSize)
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

                var node = NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, childList.get(i), this.Type, this.ChildNodes);

                // If node creation failed for some reason, continue with the rest and leave this one
                if (node == null) continue;

                // If the user is moving nodes, check if the node had been selected to move 
                // and establish the corresponding display mode
                if (this.CurrentViewState == FolderContentViewState.CopyOrMoveItem)
                {
                    // Check if it is the only focused node
                    if ((this.FocusedNode != null) && (node.OriginalMNode.getBase64Handle() == this.FocusedNode.OriginalMNode.getBase64Handle()))
                    {
                        node.DisplayMode = NodeDisplayMode.SelectedForCopyOrMove;
                        this.FocusedNode = node;
                    }

                    // Check if it is one of the multiple selected nodes
                    IsSelectedNode(node);
                }

                helperList.Add(node);

                // First add the viewport items to show some data to the user will still loading
                if (i == viewportItemCount)
                {
                    await OnUiThread(() =>
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
                await OnUiThread(() =>
                {
                    // If the task has been cancelled, stop processing
                    foreach (var megaNode in helperList.TakeWhile(megaNode => !this.LoadingCancelToken.IsCancellationRequested))
                        this.ChildNodes.Add(megaNode);
                });

                helperList.Clear();
            }

            // Add any nodes that are left over
            await OnUiThread(() =>
            {
                // Show the user that processing the childnodes is done
                SetProgressIndication(false);

                // Set empty content to folder instead of loading view
                SetEmptyContentTemplate(false);

                // If the task has been cancelled, stop processing
                foreach (var megaNode in helperList.TakeWhile(megaNode => !this.LoadingCancelToken.IsCancellationRequested))
                    this.ChildNodes.Add(megaNode);

                OnPropertyChanged("HasChildNodesBinding");
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
            //this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["DefaultCheckBoxStyle"];
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
                    //this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["MultiSelectItemCheckBoxStyle"];
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
                this.BreadCrumbs.Insert(0, NodeService.CreateNew(this.MegaSdk, App.AppInformation, parentNode, this.Type));
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

        public IMegaNode FocusedNode { get; set; }
        public List<IMegaNode> SelectedNodes { get; set; }

        private FolderContentViewState _currentViewState;
        public FolderContentViewState CurrentViewState
        {
            get { return _currentViewState; }
            set { SetField(ref _currentViewState, value); }
        }

        private FolderContentViewState _previousViewState;
        public FolderContentViewState PreviousViewState
        {
            get { return _previousViewState; }
            set { SetField(ref _previousViewState, value); }
        }

        private ObservableCollection<IMegaNode> _childNodes;
        public ObservableCollection<IMegaNode> ChildNodes
        {
            get { return _childNodes; }
            set { SetField(ref _childNodes, value); }
        }

        public bool HasChildNodesBinding
        {
            get { return HasChildNodes(); }
        }

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

        private DataTemplateSelector _nodeTemplateSelector;
        public DataTemplateSelector NodeTemplateSelector
        {
            get { return _nodeTemplateSelector; }
            private set { SetField(ref _nodeTemplateSelector, value); }
        }

        //private Style _multiSelectCheckBoxStyle;
        //public Style MultiSelectCheckBoxStyle
        //{
        //    get { return _multiSelectCheckBoxStyle; }
        //    private set { SetField(ref _multiSelectCheckBoxStyle, value); }
        //}

        private bool _isMultiSelectActive;
        public bool IsMultiSelectActive
        {
            get { return _isMultiSelectActive; }
            set
            {
                SetField(ref _isMultiSelectActive, value);
                if (_isMultiSelectActive)
                {
                    if (this.CurrentViewState != FolderContentViewState.MultiSelect)
                        this.PreviousViewState = this.CurrentViewState;
                    this.CurrentViewState = FolderContentViewState.MultiSelect;
                }
                else
                {
                    this.CurrentViewState = this.PreviousViewState;
                    this.SelectedNodes.Clear();
                }
            }
        }

        private DataTemplate _emptyContentTemplate;
        public DataTemplate EmptyContentTemplate
        {
            get { return _emptyContentTemplate; }
            private set { SetField(ref _emptyContentTemplate, value); }
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

        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string MoveToRubbishBinText => ResourceService.UiResources.GetString("UI_MoveToRubbishBin");
        public string RemoveText => ResourceService.UiResources.GetString("UI_Remove");
        public string RenameText => ResourceService.UiResources.GetString("UI_Rename");

        #endregion
    }
}
