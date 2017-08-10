using System.Collections.Generic;
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
            this.EnableCollectionChangedDetection();
            this.SelectedItems = new List<T>();
        }

        #region Public Methods

        public void EnableCollectionChangedDetection()
        {
            this.Items.CollectionChanged += OnCollectionChanged;
        }

        public void DisableCollectionChangedDetection()
        {
            this.Items.CollectionChanged -= OnCollectionChanged;
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

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

            OnPropertyChanged("HasItems");
        }

        #endregion

        #region Properties

        private ObservableCollection<T> _items;
        public ObservableCollection<T> Items
        {
            get { return _items; }
            set { SetField(ref _items, value); }
        }

        private IList<T> _selectedItems;
        public IList<T> SelectedItems
        {
            get { return _selectedItems; }
            set { SetField(ref _selectedItems, value); }
        }

        public bool HasItems => this.Items?.Count > 0;

        public bool HasSelectedItems => this.SelectedItems?.Count > 0;

        public bool OneOrMoreSelected => this.SelectedItems?.Count >= 1;

        public bool MoreThanOneSelected => this.SelectedItems?.Count > 1;

        #endregion
    }
}
