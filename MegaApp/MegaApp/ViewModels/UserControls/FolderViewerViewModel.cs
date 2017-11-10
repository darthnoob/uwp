using System;
using mega;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.UserControls
{
    public class FolderViewerViewModel : BaseUiViewModel
    {
        #region Methods

        private void Initialize()
        {
            this.Folder.ItemCollection.ItemCollectionChanged += OnItemCollectionChanged;
            this.Folder.ItemCollection.SelectedItemsCollectionChanged += OnSelectedItemsCollectionChanged;

            this.Folder.ItemCollection.OrderInverted += OnOrderInverted;
        }

        private void Deinitialize()
        {
            this.Folder.ItemCollection.ItemCollectionChanged -= OnItemCollectionChanged;
            this.Folder.ItemCollection.SelectedItemsCollectionChanged -= OnSelectedItemsCollectionChanged;

            this.Folder.ItemCollection.OrderInverted -= OnOrderInverted;
        }

        private void OnItemCollectionChanged(object sender, EventArgs args)
        {
            OnUiThread(() =>
            {
                OnPropertyChanged(nameof(this.Folder.ItemCollection.Items),
                    nameof(this.OrderTypeAndNumberOfItems),
                    nameof(this.OrderTypeAndNumberOfSelectedItems));
            });
        }

        private void OnSelectedItemsCollectionChanged(object sender, EventArgs args)
        {
            OnUiThread(() =>
            {
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems),
                    nameof(this.OrderTypeAndNumberOfSelectedItems));
            });
        }

        private void OnOrderInverted(object sender, EventArgs args)
        {
            if (this.Folder?.FolderRootNode == null) return;

            var currentOrder = UiService.GetSortOrder(
                this.Folder.FolderRootNode.Base64Handle,
                this.Folder.FolderRootNode.Name);

            switch(currentOrder)
            {
                case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    SortBy(MSortOrderType.ORDER_ALPHABETICAL_DESC);
                    break;

                case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                    SortBy(MSortOrderType.ORDER_ALPHABETICAL_ASC);
                    break;

                case MSortOrderType.ORDER_CREATION_ASC:
                    SortBy(MSortOrderType.ORDER_CREATION_DESC);
                    break;

                case MSortOrderType.ORDER_CREATION_DESC:
                    SortBy(MSortOrderType.ORDER_CREATION_ASC);
                    break;

                case MSortOrderType.ORDER_DEFAULT_ASC:
                    SortBy(MSortOrderType.ORDER_DEFAULT_DESC);
                    break;

                case MSortOrderType.ORDER_DEFAULT_DESC:
                    SortBy(MSortOrderType.ORDER_DEFAULT_ASC);
                    break;

                case MSortOrderType.ORDER_MODIFICATION_ASC:
                    SortBy(MSortOrderType.ORDER_MODIFICATION_DESC);
                    break;

                case MSortOrderType.ORDER_MODIFICATION_DESC:
                    SortBy(MSortOrderType.ORDER_MODIFICATION_ASC);
                    break;

                case MSortOrderType.ORDER_SIZE_ASC:
                    SortBy(MSortOrderType.ORDER_SIZE_DESC);
                    break;

                case MSortOrderType.ORDER_SIZE_DESC:
                    SortBy(MSortOrderType.ORDER_SIZE_ASC);
                    break;
            }
        }

        public void SortBy(MSortOrderType sortOption)
        {
            if (this.Folder?.FolderRootNode == null) return;

            UiService.SetSortOrder(this.Folder.FolderRootNode.Base64Handle, sortOption);
            this.Folder.LoadChildNodes();
        }

        #endregion

        #region Properties

        private FolderViewModel _folder;
        public FolderViewModel Folder
        {
            get { return _folder; }
            set
            {
                if (_folder != null)
                    this.Deinitialize();

                SetField(ref _folder, value);
                OnPropertyChanged(nameof(this.ItemCollection));

                if (_folder != null)
                    this.Initialize();
            }
        }

        public CollectionViewModel<IMegaNode> ItemCollection => this.Folder?.ItemCollection;

        public string OrderTypeAndNumberOfItems
        {
            get
            {
                if (this.Folder?.FolderRootNode == null) return string.Empty;

                var numChildFolders = SdkService.MegaSdk.getNumChildFolders(this.Folder.FolderRootNode.OriginalMNode);
                var numChildFiles = SdkService.MegaSdk.getNumChildFiles(this.Folder.FolderRootNode.OriginalMNode);

                switch (UiService.GetSortOrder(this.Folder.FolderRootNode.Base64Handle, this.Folder.FolderRootNode.Name))
                {
                    case MSortOrderType.ORDER_DEFAULT_ASC:
                    case MSortOrderType.ORDER_DEFAULT_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_NodeListSortedByFiles"),
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

        public string OrderTypeAndNumberOfSelectedItems
        {
            get
            {
                if (this.Folder?.FolderRootNode == null) return string.Empty;

                switch (UiService.GetSortOrder(this.Folder.FolderRootNode.Base64Handle, this.Folder.FolderRootNode.Name))
                {
                    case MSortOrderType.ORDER_DEFAULT_ASC:
                    case MSortOrderType.ORDER_DEFAULT_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_NodeListSortedByFilesMultiSelect"),
                            this.Folder.ItemCollection.SelectedItems.Count, this.Folder.ItemCollection.Items.Count);

                    case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_NodeListSortedByNameMultiSelect"),
                            this.Folder.ItemCollection.SelectedItems.Count, this.Folder.ItemCollection.Items.Count);

                    case MSortOrderType.ORDER_CREATION_ASC:
                    case MSortOrderType.ORDER_CREATION_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_NodeListSortedByDateCreatedMultiSelect"),
                            this.Folder.ItemCollection.SelectedItems.Count, this.Folder.ItemCollection.Items.Count);

                    case MSortOrderType.ORDER_MODIFICATION_ASC:
                    case MSortOrderType.ORDER_MODIFICATION_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_NodeListSortedByDateModifiedMultiSelect"),
                            this.Folder.ItemCollection.SelectedItems.Count, this.Folder.ItemCollection.Items.Count);

                    case MSortOrderType.ORDER_SIZE_ASC:
                    case MSortOrderType.ORDER_SIZE_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_NodeListSortedBySizeMultiSelect"),
                            this.Folder.ItemCollection.SelectedItems.Count, this.Folder.ItemCollection.Items.Count);

                    default:
                        return string.Empty;
                }
            }
        }

        #endregion
    }
}
