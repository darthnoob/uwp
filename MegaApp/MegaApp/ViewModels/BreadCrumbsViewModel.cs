using System.Collections.ObjectModel;
using System.Collections.Specialized;
using mega;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class BreadCrumbViewModel: BaseSdkViewModel
    {
        public BreadCrumbViewModel()
        {
            this.Items = new ObservableCollection<IBaseNode>();           
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

        #endregion

        #region Properties

        private ObservableCollection<IBaseNode> _items;
        public ObservableCollection<IBaseNode> Items
        {
            get { return _items; }
            set { SetField(ref _items, value); }
        }

        #endregion
    }
}
