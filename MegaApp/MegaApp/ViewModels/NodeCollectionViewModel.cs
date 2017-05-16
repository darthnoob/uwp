using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MegaApp.Interfaces;

namespace MegaApp.ViewModels
{
    public class NodeCollectionViewModel: BaseSdkViewModel
    {
        public NodeCollectionViewModel()
        {
            this.Items = new ObservableCollection<IMegaNode>();
            this.Items.CollectionChanged += OnCollectionChanged;
            this.SelectedItems = new List<IMegaNode>();
        }

        #region Public Methods

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
                item.IsMultiSelected = onOff;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

            OnPropertyChanged("HasItems");
        }

        #endregion

        #region Properties

        private ObservableCollection<IMegaNode> _items;
        public ObservableCollection<IMegaNode> Items
        {
            get { return _items; }
            set { SetField(ref _items, value); }
        }

        private IList<IMegaNode> _selectedItems;
        public IList<IMegaNode> SelectedItems
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
