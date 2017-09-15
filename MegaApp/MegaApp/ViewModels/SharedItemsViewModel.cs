using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class SharedItemsViewModel : BaseSdkViewModel
    {
        public SharedItemsViewModel()
        {
            this.ItemCollection = new CollectionViewModel<IMegaNode>();
            this.ItemCollection.ItemCollectionChanged += (sender, args) => OnItemCollectionChanged();
            this.ItemCollection.SelectedItemsCollectionChanged += (sender, args) => OnSelectedItemsCollectionChanged();

            this.DownloadCommand = new RelayCommand(Download);
            this.LeaveSharedCommand = new RelayCommand(LeaveShared);

            this.InvertOrderCommand = new RelayCommand(InvertOrder);

            this.CurrentOrder = MSortOrderType.ORDER_ALPHABETICAL_ASC;
        }

        #region Commands

        public ICommand DownloadCommand { get; }
        public ICommand LeaveSharedCommand { get; }

        public ICommand InvertOrderCommand { get; }

        #endregion

        #region Methods

        protected async void GetIncomingSharedItems(MUser contact = null)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // Create the option to cancel
            CreateLoadCancelOption();

            await OnUiThreadAsync(() => this.ItemCollection.Clear());
            MNodeList inSharedItems = (contact != null) ?
                SdkService.MegaSdk.getInShares(contact) : SdkService.MegaSdk.getInShares();

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    var inSharedItemsListSize = inSharedItems.size();
                    for (int i = 0; i < inSharedItemsListSize; i++)
                    {
                        // If the task has been cancelled, stop processing
                        if (LoadingCancelToken.IsCancellationRequested)
                            LoadingCancelToken.ThrowIfCancellationRequested();

                        // To avoid null values
                        if (inSharedItems.get(i) == null) continue;

                        var node = NodeService.CreateNewSharedItem(SdkService.MegaSdk, App.AppInformation,
                            inSharedItems.get(i), this);

                        // If node creation failed for some reason, continue with the rest and leave this one
                        if (node == null) continue;

                        OnUiThread(() => this.ItemCollection.Items.Add(node));
                    }
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just exit this background process because a cancellation exception has been thrown
                }

            }, LoadingCancelToken, TaskCreationOptions.PreferFairness, TaskScheduler.Current);

            this.SortBy(this.CurrentOrder);
            OnItemCollectionChanged();
        }

        /// <summary>
        /// Cancel any running load process of contacts
        /// </summary>
        public void CancelLoad()
        {
            if (this.LoadingCancelTokenSource != null && LoadingCancelToken.CanBeCanceled)
                LoadingCancelTokenSource.Cancel();
        }

        private void CreateLoadCancelOption()
        {
            if (this.LoadingCancelTokenSource != null)
            {
                this.LoadingCancelTokenSource.Dispose();
                this.LoadingCancelTokenSource = null;
            }
            this.LoadingCancelTokenSource = new CancellationTokenSource();
            this.LoadingCancelToken = LoadingCancelTokenSource.Token;
        }

        public void SortBy(MSortOrderType sortOption)
        {
            switch (sortOption)
            {
                case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaNode>(
                            this.ItemCollection.Items.OrderBy(item => item.Name));
                    });
                    break;

                case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaNode>(
                            this.ItemCollection.Items.OrderByDescending(item => item.Name));
                    });
                    break;

                case MSortOrderType.ORDER_MODIFICATION_ASC:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaNode>(
                            this.ItemCollection.Items.OrderBy(item => item.ModificationTime));
                    });
                    break;

                case MSortOrderType.ORDER_MODIFICATION_DESC:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaNode>(
                            this.ItemCollection.Items.OrderByDescending(item => item.ModificationTime));
                    });
                    break;

                default:
                    return;
            }
        }

        private async void Download()
        {
            if (!this.ItemCollection.HasSelectedItems) return;
            await MultipleDownloadAsync(this.ItemCollection.SelectedItems);
            this.ItemCollection.IsMultiSelectActive = false;
        }

        private async Task MultipleDownloadAsync(ICollection<IMegaNode> nodes)
        {
            if (nodes?.Count < 1) return;

            var downloadFolder = await FolderService.SelectFolder();
            if (downloadFolder != null)
            {
                if (await TransferService.CheckExternalDownloadPathAsync(downloadFolder.Path))
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

        private void LeaveShared()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            foreach (var node in this.ItemCollection.SelectedItems)
                node.RemoveAsync();
        }

        private void InvertOrder()
        {
            switch (this.CurrentOrder)
            {
                case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    this.CurrentOrder = MSortOrderType.ORDER_ALPHABETICAL_DESC;
                    break;
                case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                    this.CurrentOrder = MSortOrderType.ORDER_ALPHABETICAL_ASC;
                    break;
                case MSortOrderType.ORDER_MODIFICATION_ASC:
                    this.CurrentOrder = MSortOrderType.ORDER_MODIFICATION_DESC;
                    break;
                case MSortOrderType.ORDER_MODIFICATION_DESC:
                    this.CurrentOrder = MSortOrderType.ORDER_MODIFICATION_ASC;
                    break;
                default:
                    return;
            }

            this.SortBy(this.CurrentOrder);
        }

        private void OnItemCollectionChanged()
        {
            OnUiThread(() =>
            {
                OnPropertyChanged(nameof(this.NumberOfSharedItems),
                    nameof(this.NumberOfSharedItemsText),
                    nameof(this.OrderTypeAndNumberOfItems),
                    nameof(this.OrderTypeAndNumberOfSelectedItems));
            });
        }

        private void OnSelectedItemsCollectionChanged()
        {
            OnUiThread(() =>
            {
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems),
                    nameof(this.OrderTypeAndNumberOfSelectedItems));
            });
        }

        #endregion

        #region Properties

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        private CancellationToken LoadingCancelToken { get; set; }

        private CollectionViewModel<IMegaNode> _itemCollection;
        /// <summary>
        /// Folders shared with or by the contact
        /// </summary>
        public CollectionViewModel<IMegaNode> ItemCollection
        {
            get { return _itemCollection; }
            set
            {
                SetField(ref _itemCollection, value);
                this.OnItemCollectionChanged();
            }
        }

        /// <summary>
        /// Number of folders shared with or by the contact
        /// </summary>
        public int NumberOfSharedItems => this.ItemCollection.Items.Count;

        /// <summary>
        /// Number of folders shared with or by the contact as a formatted text string
        /// </summary>
        public string NumberOfSharedItemsText => string.Format("{0} {1}", this.NumberOfSharedItems,
            this.NumberOfSharedItems == 1 ? ResourceService.UiResources.GetString("UI_SharedFolder").ToLower() :
            ResourceService.UiResources.GetString("UI_SharedFolders").ToLower());

        public string OrderTypeAndNumberOfItems
        {
            get
            {
                switch (this.CurrentOrder)
                {
                    case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByName"),
                            this.ItemCollection.Items.Count);

                    case MSortOrderType.ORDER_MODIFICATION_ASC:
                    case MSortOrderType.ORDER_MODIFICATION_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByLastModification"),
                            this.ItemCollection.Items.Count);

                    default:
                        return string.Empty;
                }
            }
        }

        public string OrderTypeAndNumberOfSelectedItems
        {
            get
            {
                switch (this.CurrentOrder)
                {
                    case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByNameMultiSelect"),
                            this.ItemCollection.SelectedItems.Count, this.ItemCollection.Items.Count);

                    case MSortOrderType.ORDER_MODIFICATION_ASC:
                    case MSortOrderType.ORDER_MODIFICATION_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByLastModificationMultiSelect"),
                            this.ItemCollection.SelectedItems.Count, this.ItemCollection.Items.Count);

                    default:
                        return string.Empty;
                }
            }
        }

        private MSortOrderType _currentOrder;
        public MSortOrderType CurrentOrder
        {
            get { return _currentOrder; }
            set
            {
                SetField(ref _currentOrder, value);

                OnPropertyChanged(nameof(this.IsCurrentOrderAscending),
                    nameof(this.OrderTypeAndNumberOfItems),
                    nameof(this.OrderTypeAndNumberOfSelectedItems));
            }
        }

        public bool IsCurrentOrderAscending
        {
            get
            {
                switch (this.CurrentOrder)
                {
                    case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    case MSortOrderType.ORDER_MODIFICATION_ASC:
                    default:
                        return true;

                    case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                    case MSortOrderType.ORDER_MODIFICATION_DESC:
                        return false;
                }
            }
        }

        #endregion

        #region UiResources

        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string LeaveSharedText => ResourceService.UiResources.GetString("UI_LeaveShared");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");

        #endregion

        #region VisualResources

        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string LeaveSharedPathData => ResourceService.VisualResources.GetString("VR_LeaveSharedPathData");
        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");

        #endregion
    }
}
