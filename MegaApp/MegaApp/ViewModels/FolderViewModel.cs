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
            this.HomeSelectedCommand = new RelayCommand(BrowseToHome);
            this.ItemSelectedCommand = new RelayCommand<BreadcrumbEventArgs>(ItemSelected);
            this.RefreshCommand = new RelayCommand(Refresh);
            //this.RemoveItemCommand = new DelegateCommand(this.RemoveItem);
            //this.RenameItemCommand = new DelegateCommand(this.RenameItem);
            //this.DownloadItemCommand = new DelegateCommand(this.DownloadItem);
            //this.ImportItemCommand = new DelegateCommand(this.ImportItem);
            //this.CreateShortCutCommand = new DelegateCommand(this.CreateShortCut);
            //this.ChangeViewCommand = new DelegateCommand(this.ChangeView);
            //this.GetLinkCommand = new DelegateCommand(this.GetLink);
            //this.MultiSelectCommand = new DelegateCommand(this.MultiSelect);
            //this.ViewDetailsCommand = new DelegateCommand(this.ViewDetails);

            this.ChildNodes.CollectionChanged += ChildNodes_CollectionChanged;
            this.BreadCrumbs.CollectionChanged += BreadCrumbs_CollectionChanged;

            SetViewDefaults();

            SetEmptyContentTemplate(true);

            switch (containerType)
            {
                case ContainerType.CloudDrive:
                    this.CurrentDisplayMode = DriveDisplayMode.CloudDrive;
                    break;
                case ContainerType.RubbishBin:
                    this.CurrentDisplayMode = DriveDisplayMode.RubbishBin;
                    break;
                case ContainerType.InShares:
                    this.CurrentDisplayMode = DriveDisplayMode.InShares;
                    break;
                case ContainerType.OutShares:
                    this.CurrentDisplayMode = DriveDisplayMode.OutShares;
                    break;
                case ContainerType.ContactInShares:
                    this.CurrentDisplayMode = DriveDisplayMode.ContactInShares;
                    break;
                case ContainerType.FolderLink:
                    this.CurrentDisplayMode = DriveDisplayMode.FolderLink;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(containerType));
            }
        }

        void ChildNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("HasChildNodesBinding");
        }

        #region Commands

        public ICommand AddFolderCommand { get;}
        public ICommand HomeSelectedCommand { get; }
        public ICommand ItemSelectedCommand { get; }
        public ICommand RefreshCommand { get; }

        //public ICommand ChangeViewCommand { get; private set; }
        //public ICommand GetLinkCommand { get; private set; }
        //public ICommand RenameItemCommand { get; private set; }
        //public ICommand RemoveItemCommand { get; private set; }
        //public ICommand DownloadItemCommand { get; private set; }
        //public ICommand ImportItemCommand { get; private set; }
        //public ICommand CreateShortCutCommand { get; private set; }
        //public ICommand MultiSelectCommand { get; set; }
        //public ICommand ViewDetailsCommand { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns boolean value to indicatie if the current folder view has any child nodes
        /// </summary>
        /// <returns>True if there are child nodes, False if child node count is zero</returns>
        public bool HasChildNodes()
        {
            return this.ChildNodes.Count > 0;
        }

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

            await OnUiThread(() =>
            {
                this.ChildNodes.Clear();
            });
        }

        /// <summary>
        /// Load the mega nodes for this specific folder using the Mega SDK
        /// </summary>
        public async void LoadChildNodes()
        {
            // User must be online to perform this operation
            if ((this.Type != ContainerType.FolderLink) && !(await IsUserOnline()))
                return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // FolderRootNode should not be null
            if (this.FolderRootNode == null)
            {
                await OnUiThread(() =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_LoadNodesFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_LoadNodesFailed"),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
                return;
            }

            SetProgressIndication(true);

            // Process is started so we can set the empty content template to loading already
            SetEmptyContentTemplate(true);

            // Get the MNodes from the Mega SDK in the correct sorting order for the current folder
            MNodeList childList = NodeService.GetChildren(this.MegaSdk, this.FolderRootNode);

            if (childList == null)
            {
                await OnUiThread(() =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_LoadNodesFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_LoadNodesFailed"),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                    SetEmptyContentTemplate(false);
                });

                return;
            }

            // Clear the child nodes to make a fresh start
            ClearChildNodes();

            // Set the correct view for the main drive. Do this after the childs are cleared to speed things up
            //SetViewOnLoad();

            // Build the bread crumbs. Do this before loading the nodes so that the user can click on home
            await OnUiThread(() => BuildBreadCrumbs());

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
        public async void Refresh()
        {
            if (!await NetworkService.IsNetworkAvailable(true)) return;

            FileService.ClearFiles(
             NodeService.GetFiles(this.ChildNodes,
                Path.Combine(ApplicationData.Current.LocalFolder.Path,
                ResourceService.AppResources.GetString("AR_ThumbnailsDirectory"))));

            if (this.FolderRootNode == null)
            {
                switch (this.Type)
                {
                    case ContainerType.RubbishBin:
                        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, App.AppInformation, this.MegaSdk.getRubbishNode(), this.Type);
                        break;

                    case ContainerType.CloudDrive:
                    case ContainerType.FolderLink:
                        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, App.AppInformation, this.MegaSdk.getRootNode(), this.Type);
                        break;
                }
            }

            LoadChildNodes();
        }

        public async void AddFolder()
        {
            if (!await IsUserOnline()) return;

            // Only 1 CustomInputDialog should be open at the same time.
            if (App.AppInformation.PickerOrAsyncDialogIsOpen) return;

            var inputDialog = new CustomInputDialog(
                ResourceService.UiResources.GetString("UI_AddFolder"),
                ResourceService.UiResources.GetString("UI_CreateFolder"),
                App.AppInformation);

            inputDialog.OkButtonTapped += (sender, args) =>
            {
                if (FolderRootNode == null)
                {
                    OnUiThread(() =>
                    {
                        new CustomMessageDialog(
                            ResourceService.AppMessages.GetString("AM_CreateFolderFailed_Title"),
                            ResourceService.AppMessages.GetString("AM_CreateFolderFailed"),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialogAsync();
                    });

                    return;
                }

                MegaSdk.createFolder(args.InputText, FolderRootNode.OriginalMNode,
                     new CreateFolderRequestListener());
            };
            inputDialog.ShowDialogAsync();
        }

        public void OnChildNodeTapped(IMegaNode node)
        {
            switch (node.Type)
            {
                case MNodeType.TYPE_UNKNOWN:
                    break;
                case MNodeType.TYPE_FILE:
                    // If the user is moving nodes don't process the file node
                    if (this.CurrentDisplayMode != DriveDisplayMode.CopyOrMoveItem)
                        ProcessFileNode(node);
                    break;
                case MNodeType.TYPE_FOLDER:
                    // If the user is moving nodes and the folder is one of the selected nodes don't navigate to it
                    if ((this.CurrentDisplayMode == DriveDisplayMode.CopyOrMoveItem) && (IsSelectedNode(node))) return;
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
                    EmptyInformationText = "";
                });
            }
            else
            {
                switch (Type)
                {
                    case ContainerType.CloudDrive:
                    case ContainerType.RubbishBin:
                        var megaRoot = MegaSdk.getRootNode();
                        var megaRubbishBin = MegaSdk.getRubbishNode();
                        if (FolderRootNode != null && megaRoot != null && FolderRootNode.Base64Handle.Equals(megaRoot.getBase64Handle()))
                        {
                            OnUiThread(() =>
                            {
                                //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListCloudDriveEmptyContent"];
                                EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyCloudDrive").ToLower();
                            });
                        }
                        else if (this.FolderRootNode != null && megaRubbishBin != null && this.FolderRootNode.Base64Handle.Equals(megaRubbishBin.getBase64Handle()))
                        {
                            OnUiThread(() =>
                            {
                                //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListRubbishBinEmptyContent"];
                                EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyRubbishBin").ToLower();
                            });
                        }
                        else
                        {
                            OnUiThread(() =>
                            {
                                //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListEmptyContent"];
                                EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyFolder").ToLower();
                            });
                        }
                        break;

                    case ContainerType.InShares:
                    case ContainerType.OutShares:
                        OnUiThread(() =>
                        {
                            //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaSharedFoldersListEmptyContent"];
                            EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptySharedFolders").ToLower();
                        });
                        break;

                    case ContainerType.ContactInShares:
                        break;

                    case ContainerType.Offline:
                        OnUiThread(() =>
                        {
                            //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListRubbishBinEmptyContent"];
                            EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyOffline").ToLower();
                        });
                        break;

                    case ContainerType.FolderLink:
                        OnUiThread(() =>
                        {
                            //EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListEmptyContent"];
                            EmptyInformationText = ResourceService.UiResources.GetString("UI_EmptyFolder").ToLower();
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

        public async void SetProgressIndication(bool onOff, string busyText = null)
        {
            await OnUiThread(() =>
            {
                this.IsBusy = onOff;
                this.BusyText = busyText;
            });
        }

        private async void CreateChildren(MNodeList childList, int listSize)
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
                if (this.CurrentDisplayMode == DriveDisplayMode.CopyOrMoveItem)
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
                    var waitHandleViewportNodes = new AutoResetEvent(false);
                    await OnUiThread(() =>
                    {
                        // If the task has been cancelled, stop processing
                        foreach (var megaNode in helperList.TakeWhile(megaNode => !this.LoadingCancelToken.IsCancellationRequested))
                        {
                            this.ChildNodes.Add(megaNode);
                        }
                        waitHandleViewportNodes.Set();
                    });
                    waitHandleViewportNodes.WaitOne();

                    helperList.Clear();
                    continue;
                }

                if (helperList.Count != backgroundItemCount || i <= viewportItemCount) continue;

                // Add the rest of the items in the background to the list
                var waitHandleBackgroundNodes = new AutoResetEvent(false);
                await OnUiThread(() =>
                {
                    // If the task has been cancelled, stop processing
                    foreach (var megaNode in helperList.TakeWhile(megaNode => !this.LoadingCancelToken.IsCancellationRequested))
                    {
                        this.ChildNodes.Add(megaNode);
                    }
                    waitHandleBackgroundNodes.Set();
                });
                waitHandleBackgroundNodes.WaitOne();

                helperList.Clear();
            }

            // Add any nodes that are left over
            var waitHandleRestNodes = new AutoResetEvent(false);
            await OnUiThread(() =>
            {
                // Show the user that processing the childnodes is done
                SetProgressIndication(false);

                // Set empty content to folder instead of loading view
                SetEmptyContentTemplate(false);

                // If the task has been cancelled, stop processing
                foreach (var megaNode in helperList.TakeWhile(megaNode => !this.LoadingCancelToken.IsCancellationRequested))
                {
                    this.ChildNodes.Add(megaNode);
                }
                waitHandleRestNodes.Set();
            });
            waitHandleRestNodes.WaitOne();

            await OnUiThread(() => OnPropertyChanged("HasChildNodesBinding"));
        }

        private void InitializePerformanceParameters(out int viewportItemCount, out int backgroundItemCount)
        {
            viewportItemCount = 0;
            backgroundItemCount = 0;

            // Each view has different performance options
            switch (this.ViewMode)
            {
                case ViewMode.ListView:
                    viewportItemCount = 256;
                    backgroundItemCount = 1024;
                    break;
                case ViewMode.LargeThumbnails:
                    viewportItemCount = 128;
                    backgroundItemCount = 512;
                    break;
                case ViewMode.SmallThumbnails:
                    viewportItemCount = 72;
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

        private void SetViewDefaults()
        {
            //this.VirtualizationStrategy = new StackVirtualizationStrategyDefinition()
            //{
            //    Orientation = Orientation.Vertical
            //};

            this.NodeTemplateSelector = new NodeTemplateSelector()
            {
                FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListFileItemContent"],
                FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListFolderItemContent"]
            };

            this.ViewMode = ViewMode.ListView;
            //this.NextViewButtonPathData = VisualResources.LargeThumbnailViewPathData;
            //this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["DefaultCheckBoxStyle"];
        }

        public void BuildBreadCrumbs()
        {
            this.BreadCrumbs.Clear();

            // Top root nodes have no breadcrumbs
            if (this.FolderRootNode == null || this.FolderRootNode.Type == MNodeType.TYPE_ROOT || this.FolderRootNode.Type == MNodeType.TYPE_RUBBISH) return;

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

        void BreadCrumbs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
        public DriveDisplayMode CurrentDisplayMode { get; set; }
        public DriveDisplayMode PreviousDisplayMode { get; set; }
        public List<IMegaNode> SelectedNodes { get; set; }

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

        public ViewMode ViewMode { get; set; }

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

        private Style _multiSelectCheckBoxStyle;
        public Style MultiSelectCheckBoxStyle
        {
            get { return _multiSelectCheckBoxStyle; }
            private set { SetField(ref _multiSelectCheckBoxStyle, value); }
        }

        private bool _isMultiSelectActive;
        public bool IsMultiSelectActive
        {
            get { return _isMultiSelectActive; }
            set { SetField(ref _isMultiSelectActive, value); }
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

        
    }
}
