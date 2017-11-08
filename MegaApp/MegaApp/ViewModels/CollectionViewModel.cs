using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    /// <summary>
    /// View model to extend the possibilities of a collection of items
    /// </summary>
    /// <typeparam name="T">
    /// Type of the collection items. Supported types:
    /// - IMegaContact
    /// - IMegaContactRequest
    /// - IMegaNode
    /// </typeparam>
    public class CollectionViewModel<T> : BaseSdkViewModel
    {
        public CollectionViewModel()
        {
            this.Items = new ObservableCollection<T>();
            this.SelectedItems = new ObservableCollection<T>();

            this.MultiSelectCommand = new RelayCommand(MultiSelect);
            this.SelectAllCommand = new RelayCommand<bool>(SelectAll);
            this.SelectionChangedCommand = new RelayCommand(SelectionChanged);

            this.InvertOrderCommand = new RelayCommand(InvertOrder);

            this.EnableCollectionChangedDetection();            
        }

        #region Events

        /// <summary>
        /// Event triggered when the item collection changes
        /// </summary>
        public event EventHandler ItemCollectionChanged;

        /// <summary>
        /// Event invocator method called when the item collection changes
        /// </summary>
        protected void OnItemsCollectionChanged()
        {
            this.ItemCollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the selected items collection changes
        /// </summary>
        public event EventHandler SelectedItemsCollectionChanged;

        /// <summary>
        /// Event invocator method called when the selected items collection changes
        /// </summary>
        protected void OnSelectedItemsCollectionChanged()
        {
            this.SelectedItemsCollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the multi select scenario is enabled
        /// </summary>
        public event EventHandler MultiSelectEnabled;

        /// <summary>
        /// Event invocator method called when the multi select scenario is enabled
        /// </summary>
        protected virtual void OnMultiSelectEnabled()
        {
            this.MultiSelectEnabled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the multi select scenario is disabled
        /// </summary>
        public event EventHandler MultiSelectDisabled;

        /// <summary>
        /// Event invocator method called when the multi select scenario is disabled
        /// </summary>
        protected virtual void OnMultiSelectDisabled()
        {
            this.MultiSelectDisabled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when user choose to select/deselect all the items
        /// </summary>
        public event EventHandler<bool> AllSelected;

        /// <summary>
        /// Event invocator method called when user choose to select/deselect all the items
        /// </summary>
        protected virtual void OnAllSelected(bool value)
        {
            this.AllSelected?.Invoke(this, value);
        }

        /// <summary>
        /// Event triggered when user invert the order of items
        /// </summary>
        public event EventHandler OrderInverted;

        /// <summary>
        /// Event invocator method called when user invert the order of items
        /// </summary>
        protected virtual void OnOrderInverted()
        {
            this.OrderInverted?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Commands

        public ICommand MultiSelectCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand InvertOrderCommand { get; }

        #endregion

        #region Public Methods

        public void EnableCollectionChangedDetection()
        {
            this.Items.CollectionChanged += OnItemsCollectionChanged;
        }

        public void DisableCollectionChangedDetection()
        {
            this.Items.CollectionChanged -= OnItemsCollectionChanged;
        }

        public void SelectAll(bool value)
        {
            Select(value);
            this.OnAllSelected(value);

            if (!value) ClearSelection();
        }

        public void Clear()
        {
            if (this.Items == null || !this.Items.Any()) return;
            this.Items.Clear();
        }

        public void ClearSelection()
        {
            if (this.SelectedItems == null || !this.SelectedItems.Any()) return;
            this.SelectedItems.Clear();
        }

        protected void SelectionChanged()
        {
            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
                this.IsMultiSelectActive = (this.IsMultiSelectActive && this.HasSelectedItems) ||
                    this.MoreThanOneSelected;
            else
                this.IsMultiSelectActive = this.IsMultiSelectActive && this.HasSelectedItems;

            if (this.HasSelectedItems)
                this.FocusedItem = this.SelectedItems.Last();

            OnPropertyChanged(nameof(this.SelectedItems), nameof(this.HasSelectedItems),
                nameof(this.OnlyOneSelectedItem), nameof(this.MoreThanOneSelected),
                nameof(this.HasAllItemsSelected));

            this.OnSelectedItemsCollectionChanged();
        }

        private void InvertOrder()
        {
            switch (this.CurrentOrderDirection)
            {
                case SortOrderDirection.ORDER_ASCENDING:
                    this.CurrentOrderDirection = SortOrderDirection.ORDER_DESCENDING;
                    break;
                case SortOrderDirection.ORDER_DESCENDING:
                    this.CurrentOrderDirection = SortOrderDirection.ORDER_ASCENDING;
                    break;
                default:
                    return;
            }

            OnOrderInverted();
        }

        /// <summary>
        /// Sets if multiselect is active or not.
        /// </summary>
        protected void MultiSelect() => this.IsMultiSelectActive = !this.IsMultiSelectActive;

        #endregion

        #region Private Methods

        private void Select(bool onOff)
        {
            foreach (var item in this.Items)
            {
                if (item is IMegaContact)
                    (item as IMegaContact).IsMultiSelected = onOff;
                if (item is IMegaContactRequest)
                    (item as IMegaContactRequest).IsMultiSelected = onOff;
                if (item is IMegaNode)
                    (item as IMegaNode).IsMultiSelected = onOff;
            }
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                // Start a new task to avoid freeze the UI
                Task.Run(() =>
                {
                    foreach (var item in e.NewItems)
                    {
                        if (item is IMegaNode)
                            (item as IMegaNode)?.SetThumbnailImage();
                    }
                });
            }

            OnPropertyChanged(nameof(this.Items), nameof(this.HasItems), 
                nameof(this.HasAllItemsSelected));

            this.OnItemsCollectionChanged();
        }

        #endregion

        #region Properties

        private ObservableCollection<T> _items;
        public ObservableCollection<T> Items
        {
            get { return _items; }
            set { SetField(ref _items, value); }
        }

        private ObservableCollection<T> _selectedItems;
        public ObservableCollection<T> SelectedItems
        {
            get { return _selectedItems; }
            set { SetField(ref _selectedItems, value); }
        }

        private T _focusedItem;
        public T FocusedItem
        {
            get { return _focusedItem; }
            set { SetField(ref _focusedItem, value); }
        }

        private bool _isMultiSelectActive;
        public bool IsMultiSelectActive
        {
            get { return _isMultiSelectActive || this.MoreThanOneSelected; }
            set
            {
                if (!SetField(ref _isMultiSelectActive, value)) return;

                if (_isMultiSelectActive)
                {
                    this.OnMultiSelectEnabled();
                }
                else
                {
                    this.ClearSelection();
                    OnPropertyChanged(nameof(this.IsMultiSelectActive));
                    this.OnMultiSelectDisabled();
                }
            }
        }

        private SortOrderDirection _currentOrderDirection;
        public SortOrderDirection CurrentOrderDirection
        {
            get { return _currentOrderDirection; }
            set
            {
                SetField(ref _currentOrderDirection, value);
                OnPropertyChanged(nameof(this.IsCurrentOrderAscending));
            }
        }

        public bool IsCurrentOrderAscending =>
            this.CurrentOrderDirection == SortOrderDirection.ORDER_ASCENDING;

        public bool HasItems => this.Items?.Count > 0;

        public bool HasSelectedItems => this.SelectedItems?.Count > 0;

        public bool OnlyOneSelectedItem => this.SelectedItems?.Count == 1;

        public bool MoreThanOneSelected => this.SelectedItems?.Count > 1;

        public bool HasAllItemsSelected => this.SelectedItems?.Count == this.Items?.Count;

        #endregion
    }
}
