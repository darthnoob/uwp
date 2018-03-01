using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using mega;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.ViewModels.Offline;

namespace MegaApp.ViewModels
{
    public class BreadCrumbViewModel: BaseSdkViewModel
    {
        public BreadCrumbViewModel(MegaSDK megaSdk) : base(megaSdk)
        {
            this.Items = new ObservableCollection<IBaseNode>();
            this.Items.CollectionChanged += OnItemsCollectionChanged;
        }

        #region Public Methods

        public void Create(FolderViewModel folder)
        {
            this.Items.Clear();

            // Top root nodes have no breadcrumbs
            if (folder.FolderRootNode == null ||
                folder.FolderRootNode.Type == MNodeType.TYPE_ROOT ||
                folder.FolderRootNode.Type == MNodeType.TYPE_RUBBISH) return;

            this.Items.Add(folder.FolderRootNode);

            MNode parentNode = this.MegaSdk.getParentNode(folder.FolderRootNode.OriginalMNode);
            while (parentNode != null && 
                   parentNode.getType() != MNodeType.TYPE_ROOT &&
                   parentNode.getType() != MNodeType.TYPE_RUBBISH)
            {
                this.Items.Insert(0, NodeService.CreateNew(this.MegaSdk, App.AppInformation, parentNode, folder));
                parentNode = this.MegaSdk.getParentNode(parentNode);
            }
        }

        public void Create(OfflineFolderViewModel folder)
        {
            this.Items.Clear();

            // Top root nodes have no breadcrumbs
            if (folder.FolderRootNode == null ||
                FolderService.IsOfflineRootFolder(folder.FolderRootNode.NodePath)) return;

            this.Items.Add(folder.FolderRootNode);

            DirectoryInfo parentNode = (new DirectoryInfo(folder.FolderRootNode.NodePath)).Parent;
            while ((parentNode != null) && !FolderService.IsOfflineRootFolder(parentNode.FullName))
            {
                this.Items.Insert(0, new OfflineFolderNodeViewModel(parentNode));
                parentNode = (new DirectoryInfo(parentNode.FullName)).Parent;
            }
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(this.HasPath));
        }

        #endregion

        #region Properties

        private ObservableCollection<IBaseNode> _items;
        public ObservableCollection<IBaseNode> Items
        {
            get { return _items; }
            set { SetField(ref _items, value); }
        }

        public bool HasPath => this.Items?.Count > 0;

        #endregion
    }
}
