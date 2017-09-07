using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MegaApp.Interfaces;

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

        #endregion

        #region Public Methods

        public void EnableCollectionChangedDetection()
        {
            this.Items.CollectionChanged += OnItemsCollectionChanged;
            this.SelectedItems.CollectionChanged += OnSelectedItemsCollectionChanged;
        }

        public void DisableCollectionChangedDetection()
        {
            this.Items.CollectionChanged -= OnItemsCollectionChanged;
            this.SelectedItems.CollectionChanged -= OnSelectedItemsCollectionChanged;
        }

        public void SelectAll()
        {
            Select(true);
        }

        public void DeselectAll()
        {
            Select(false);
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

        private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(this.SelectedItems), nameof(this.HasSelectedItems),
                nameof(this.OnlyOneSelectedItem), nameof(this.OneOrMoreSelected), 
                nameof(this.MoreThanOneSelected), nameof(this.HasAllItemsSelected));

            this.OnSelectedItemsCollectionChanged();
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

        public bool HasItems => this.Items?.Count > 0;

        public bool HasSelectedItems => this.SelectedItems?.Count > 0;

        public bool OnlyOneSelectedItem => this.SelectedItems?.Count == 1;

        public bool OneOrMoreSelected => this.SelectedItems?.Count >= 1;

        public bool MoreThanOneSelected => this.SelectedItems?.Count > 1;

        public bool HasAllItemsSelected => this.SelectedItems?.Count == this.Items?.Count;

        #endregion
    }
}
