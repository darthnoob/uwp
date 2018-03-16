using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.Offline
{
    /// <summary>
    /// Class that handles all process and operations of a section that contains Offline nodes
    /// </summary>
    public class OfflineFolderViewModel : BaseFolderViewModel
    {
        public OfflineFolderViewModel() : base(SdkService.MegaSdk, ContainerType.Offline)
        {
            this.FolderRootNode = null;

            this.ItemCollection.SelectedItemsCollectionChanged += OnSelectedItemsCollectionChanged;

            this.RemoveFromOfflineCommand = new RelayCommand(RemoveFromOffline);
            SetViewDefaults();
        }

        #region Commands

        public ICommand RemoveFromOfflineCommand { get; }

        #endregion

        #region Properties

        public OfflineFolderViewModel Folder => this;

        public new IOfflineNode FolderRootNode
        {
            get { return base.FolderRootNode as IOfflineNode; }
            set { base.FolderRootNode = value; }
        }

        public override string OrderTypeAndNumberOfItems
        {
            get
            {
                if (this.FolderRootNode == null) return string.Empty;

                var numChildFolders = FolderService.GetNumChildFolders(this.FolderRootNode.NodePath);
                var numChildFiles = FolderService.GetNumChildFiles(this.FolderRootNode.NodePath, true);

                switch (UiService.GetSortOrder(this.FolderRootNode.Base64Handle, this.FolderRootNode.Name))
                {
                    case MSortOrderType.ORDER_DEFAULT_ASC:
                    case MSortOrderType.ORDER_DEFAULT_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_NodeListSortedByType"),
                            numChildFolders, numChildFiles);

                    case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_NodeListSortedByName"),
                            numChildFolders, numChildFiles);

                    case MSortOrderType.ORDER_CREATION_ASC:
                    case MSortOrderType.ORDER_CREATION_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_NodeListSortedByDateCreated"),
                            numChildFolders, numChildFiles);

                    case MSortOrderType.ORDER_MODIFICATION_ASC:
                    case MSortOrderType.ORDER_MODIFICATION_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_NodeListSortedByDateModified"),
                            numChildFolders, numChildFiles);

                    case MSortOrderType.ORDER_SIZE_ASC:
                    case MSortOrderType.ORDER_SIZE_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_NodeListSortedBySize"),
                            numChildFolders, numChildFiles);

                    default:
                        return string.Empty;
                }
            }
        }

        #endregion

        #region Methods

        public override void BrowseToHome()
        {
            if (this.FolderRootNode == null) return;

            ClosePanels();

            this.FolderRootNode = new OfflineFolderNodeViewModel(
                new DirectoryInfo(AppService.GetOfflineDirectoryPath()), this);

            OnFolderNavigatedTo();

            LoadChildNodes();
        }

        public void ClearChildNodes() => this.ItemCollection.Clear();
        
        public void DeselectAll() => this.ItemCollection.SelectAll(false);

        public void SelectAll() => this.ItemCollection.SelectAll(true);

        /// <summary>
        /// Load the nodes for this specific folder
        /// </summary>
        public override void LoadChildNodes()
        {
            // First cancel any other loading task that is busy
            CancelLoad();

            // FolderRootNode should not be null
            if (FolderRootNode == null)
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_LoadNodesFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_LoadNodesFailed"));
                });
                return;
            }

            SetProgressIndication(true);

            // Process is started so we can set the empty content template to loading already
            SetEmptyContent(true);

            // Clear the child nodes to make a fresh start
            ClearChildNodes();

            // Set the correct view for the main drive. Do this after the childs are cleared to speed things up
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
                    // We will not add nodes one by one in the dispatcher but in groups
                    List<IOfflineNode> helperList;
                    try { helperList = new List<IOfflineNode>(1024); }
                    catch (ArgumentOutOfRangeException) { helperList = new List<IOfflineNode>(); }

                    this.ItemCollection.DisableCollectionChangedDetection();

                    string[] childFolders = Directory.GetDirectories(FolderRootNode.NodePath);
                    foreach (var folder in childFolders)
                    {
                        var childNode = new OfflineFolderNodeViewModel(new DirectoryInfo(folder), this, this.ItemCollection.Items);
                        if (childNode == null) continue;

                        if (FolderService.IsEmptyFolder(childNode.NodePath))
                        {
                            FolderService.DeleteFolder(childNode.NodePath, true);
                            continue;
                        }

                        OnUiThread(() => this.ItemCollection.Items.Add(childNode));
                    }

                    string[] childFiles = Directory.GetFiles(FolderRootNode.NodePath);
                    foreach (var file in childFiles)
                    {
                        var fileInfo = new FileInfo(file);

                        if (FileService.IsPendingTransferFile(fileInfo.Name))
                        {
                            if (!(TransferService.MegaTransfers.Downloads.Count > 0))
                                FileService.DeleteFile(fileInfo.FullName);
                            continue;
                        }

                        var childNode = new OfflineFileNodeViewModel(fileInfo, this, this.ItemCollection.Items);
                        if (childNode == null) continue;

                        OnUiThread(() => this.ItemCollection.Items.Add(childNode));
                    }

                    this.ItemCollection.EnableCollectionChangedDetection();

                    this.OrderChildNodes();

                    // Show the user that processing the childnodes is done
                    SetProgressIndication(false);

                    // Set empty content to folder instead of loading view
                    SetEmptyContent(false);
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just exit this background process because a cancellation exception has been thrown
                }

            }, LoadingCancelToken, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
        }

        public override void OnChildNodeTapped(IBaseNode baseNode)
        {
            // Needed to avoid process the node when the user is in MultiSelect.
            if (this.ItemCollection.IsMultiSelectActive) return;
            if (!(baseNode is IOfflineNode)) return;

            var node = baseNode as IOfflineNode;
            if (node.IsFolder)
                BrowseToFolder(node);
            else
                node.Open();
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
                        this.NodeTemplateSelector = new OfflineNodeTemplateSelector()
                        {
                            FileItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeGridViewFileItemContent"],
                            FolderItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeGridViewFolderItemContent"]
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
                this.NodeTemplateSelector = new OfflineNodeTemplateSelector()
                {
                    FileItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeListViewFileItemContent"],
                    FolderItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeListViewFolderItemContent"]
                };
            });

            base.SetViewDefaults();
        }

        private void OrderChildNodes()
        {
            IOrderedEnumerable<IBaseNode> orderedNodes;

            switch (UiService.GetSortOrder(FolderRootNode.Base64Handle, FolderRootNode.Name))
            {
                case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    orderedNodes = this.ItemCollection.Items.OrderBy(node => node.Name);
                    break;

                case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                    orderedNodes = this.ItemCollection.Items.OrderByDescending(node => node.Name);
                    break;

                case MSortOrderType.ORDER_CREATION_ASC:
                    orderedNodes = this.ItemCollection.Items.OrderBy(node => node.CreationTime);
                    break;

                case MSortOrderType.ORDER_CREATION_DESC:
                    orderedNodes = this.ItemCollection.Items.OrderByDescending(node => node.CreationTime);
                    break;

                case MSortOrderType.ORDER_MODIFICATION_ASC:
                    orderedNodes = this.ItemCollection.Items.OrderBy(node => node.ModificationTime);
                    break;

                case MSortOrderType.ORDER_MODIFICATION_DESC:
                    orderedNodes = this.ItemCollection.Items.OrderByDescending(node => node.ModificationTime);
                    break;

                case MSortOrderType.ORDER_SIZE_ASC:
                    orderedNodes = this.ItemCollection.Items.OrderBy(node => node.Size);
                    break;

                case MSortOrderType.ORDER_SIZE_DESC:
                    orderedNodes = this.ItemCollection.Items.OrderByDescending(node => node.Size);
                    break;

                case MSortOrderType.ORDER_DEFAULT_DESC:
                    orderedNodes = this.ItemCollection.Items.OrderBy(node => node.IsFolder);
                    break;

                case MSortOrderType.ORDER_DEFAULT_ASC:
                case MSortOrderType.ORDER_NONE:
                default:
                    orderedNodes = this.ItemCollection.Items.OrderByDescending(node => node.IsFolder);
                    break;
            }

            OnUiThread(() => this.ItemCollection.Items = new ObservableCollection<IBaseNode>(orderedNodes));
        }

        private void OnSelectedItemsCollectionChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(this.Folder));
        }

        private async void RemoveFromOffline()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            int count = this.ItemCollection.SelectedItems.Count;
            var title = ResourceService.AppMessages.GetString("AM_RemoveFromOfflineQuestion_Title");
            var message = this.ItemCollection.OnlyOneSelectedItem ?
                string.Format(ResourceService.AppMessages.GetString("AM_RemoveFromOfflineQuestion"), this.ItemCollection.SelectedItems.First().Name) :
                string.Format(ResourceService.AppMessages.GetString("AM_MultiSelectRemoveFromOfflineQuestion"), count);

            var result = await DialogService.ShowOkCancelAsync(title, message);

            if (!result) return;

            // Use a temp variable to avoid InvalidOperationException
            MultipleRemoveFromOffline(this.ItemCollection.SelectedItems.ToList());

            this.ItemCollection.IsMultiSelectActive = false;
        }

        private async void MultipleRemoveFromOffline(ICollection<IBaseNode> nodes)
        {
            if (nodes == null || nodes.Count < 1) return;

            foreach (var n in nodes)
            {
                var node = n as IOfflineNode;
                if (node == null) continue;
                await node.RemoveFromOfflineAsync(true);
            }
        }

        private void SetEmptyContent(bool isLoading)
        {
            if (isLoading)
            {
                OnUiThread(() =>
                {
                    this.EmptyStateHeaderText = string.Empty;
                    this.EmptyStateSubHeaderText = string.Empty;
                });
            }
            else
            {
                OnUiThread(() =>
                {
                    this.EmptyStateHeaderText = ResourceService.EmptyStates.GetString("ES_OfflineHeader");
                    this.EmptyStateSubHeaderText = ResourceService.EmptyStates.GetString("ES_OfflineSubHeader");
                });
            }
        }

        #endregion

        #region UiResources

        public string RemoveFromOfflineText => ResourceService.UiResources.GetString("UI_RemoveFromOffline");

        #endregion
    }
}
