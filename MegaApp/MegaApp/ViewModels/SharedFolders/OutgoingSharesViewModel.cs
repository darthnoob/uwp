using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using mega;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.SharedFolders
{
    public class OutgoingSharesViewModel : SharedFoldersListViewModel
    {
        public void Initialize()
        {
            this.GetOutgoingSharedItems();

            this.ItemCollection.ItemCollectionChanged += (sender, args) => OnItemCollectionChanged();
            this.ItemCollection.SelectedItemsCollectionChanged += (sender, args) => OnSelectedItemsCollectionChanged();

            this.ItemCollection.OrderInverted += (sender, args) => SortBy(this.CurrentOrder, this.ItemCollection.CurrentOrderDirection);

            if (App.GlobalListener == null) return;
            App.GlobalListener.SharedItemUpdated += this.OnSharedItemsUpdated;
        }

        public void Deinitialize()
        {
            this.ItemCollection.ItemCollectionChanged -= (sender, args) => OnItemCollectionChanged();
            this.ItemCollection.SelectedItemsCollectionChanged -= (sender, args) => OnSelectedItemsCollectionChanged();

            this.ItemCollection.OrderInverted -= (sender, args) => SortBy(this.CurrentOrder, this.ItemCollection.CurrentOrderDirection);

            if (App.GlobalListener == null) return;
            App.GlobalListener.SharedItemUpdated -= this.OnSharedItemsUpdated;
        }

        protected async void GetOutgoingSharedItems()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // Create the option to cancel
            CreateLoadCancelOption();

            await OnUiThreadAsync(() => this.ItemCollection.Clear());
            MShareList outSharedItems = SdkService.MegaSdk.getOutShares();

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    ulong lastFolderHandle = 0;
                    var outSharedItemsListSize = outSharedItems.size();
                    for (int i = 0; i < outSharedItemsListSize; i++)
                    {
                        // If the task has been cancelled, stop processing
                        if (LoadingCancelToken.IsCancellationRequested)
                            LoadingCancelToken.ThrowIfCancellationRequested();

                        var item = outSharedItems.get(i);

                        // To avoid null values and repeated values (folders shared with more than one user)
                        if (item == null || lastFolderHandle == item.getNodeHandle()) continue;

                        lastFolderHandle = item.getNodeHandle();

                        var node = NodeService.CreateNewSharedFolder(SdkService.MegaSdk, App.AppInformation,
                            SdkService.MegaSdk.getNodeByHandle(item.getNodeHandle()), this);

                        // If node creation failed for some reason, continue with the rest and leave this one
                        if (node == null) continue;

                        OnUiThread(() => this.ItemCollection.Items.Add((IMegaSharedFolderNode)node));
                    }
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just exit this background process because a cancellation exception has been thrown
                }

            }, LoadingCancelToken, TaskCreationOptions.PreferFairness, TaskScheduler.Current);

            this.SortBy(this.CurrentOrder, this.ItemCollection.CurrentOrderDirection);
            OnItemCollectionChanged();
        }

        public void SortBy(OutgoingSharesSortOrderType sortOption, SortOrderDirection sortDirection)
        {
            switch (sortOption)
            {
                case OutgoingSharesSortOrderType.ORDER_NAME:
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items = new ObservableCollection<IMegaSharedFolderNode>(this.ItemCollection.IsCurrentOrderAscending ? 
                            this.ItemCollection.Items.OrderBy(item => item.Name) : this.ItemCollection.Items.OrderByDescending(item => item.Name));
                    });
                    break;

                default:
                    return;
            }
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

        private void OnSharedItemsUpdated(object sender, MNode node)
        {
            this.GetOutgoingSharedItems();
        }

        #region Properties

        public string OrderTypeAndNumberOfItems
        {
            get
            {
                switch (this.CurrentOrder)
                {
                    case OutgoingSharesSortOrderType.ORDER_NAME:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByName"),
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
                    case OutgoingSharesSortOrderType.ORDER_NAME:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByNameMultiSelect"),
                            this.ItemCollection.SelectedItems.Count, this.ItemCollection.Items.Count);
                    default:
                        return string.Empty;
                }
            }
        }

        private OutgoingSharesSortOrderType _currentOrder;
        public OutgoingSharesSortOrderType CurrentOrder
        {
            get { return _currentOrder; }
            set
            {
                SetField(ref _currentOrder, value);

                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems),
                    nameof(this.OrderTypeAndNumberOfSelectedItems));
            }
        }

        #endregion

        #region EmptyStates

        public string EmptyStateHeaderText => ResourceService.EmptyStates.GetString("ES_OutgoingSharesHeader");
        public string EmptyStateSubHeaderText => ResourceService.EmptyStates.GetString("ES_OutgoingSharesSubHeader");

        #endregion
    }
}
