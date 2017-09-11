using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
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

            this.InvertOrderCommand = new RelayCommand(InvertOrder);
            this.SelectionChangedCommand = new RelayCommand(SelectionChanged);

            this.CurrentOrder = MSortOrderType.ORDER_ALPHABETICAL_ASC;
        }

        #region Commands

        public ICommand InvertOrderCommand { get; }
        public ICommand SelectionChangedCommand { get; }

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

                        var node = NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation,
                            inSharedItems.get(i), null);

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

        private void SelectionChanged()
        {
            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
                this.IsMultiSelectActive = (this.IsMultiSelectActive && this.ItemCollection.OneOrMoreSelected) ||
                    this.ItemCollection.MoreThanOneSelected;
            else
                this.IsMultiSelectActive = this.IsMultiSelectActive && this.ItemCollection.OneOrMoreSelected;

            if (this.ItemCollection.HasSelectedItems)
            {
                this.ItemCollection.FocusedItem = this.ItemCollection.SelectedItems.Last();
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfSelectedItems));
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

        private bool _isMultiSelectActive;
        public bool IsMultiSelectActive
        {
            get { return _isMultiSelectActive || this.ItemCollection.MoreThanOneSelected; }
            set
            {
                if (!SetField(ref _isMultiSelectActive, value)) return;

                if (_isMultiSelectActive)
                {
                    //this.OnMultiSelectEnabled();
                }
                else
                {
                    this.ItemCollection.ClearSelection();
                    OnPropertyChanged(nameof(this.IsMultiSelectActive));
                    //this.OnMultiSelectDisabled();
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
    }
}
