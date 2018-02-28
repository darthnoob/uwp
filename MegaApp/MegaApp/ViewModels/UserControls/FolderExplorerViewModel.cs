using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels.UserControls
{
    public class FolderExplorerViewModel : BaseUiViewModel
    {
        public FolderExplorerViewModel()
        {
            this.CopyFolderCommand = new RelayCommand(CopyFolder);
            this.DownloadFolderCommand = new RelayCommand(DownloadFolder);
            this.ImportFolderCommand = new RelayCommand(ImportFolder);
            this.InformationCommand = new RelayCommand(ShowFolderInformation);
            this.RenameFolderCommand = new RelayCommand(RenameFolder);
        }

        #region Commands

        public ICommand CopyFolderCommand { get; }
        public ICommand DownloadFolderCommand { get; }
        public ICommand ImportFolderCommand { get; }
        public ICommand InformationCommand { get; }
        public ICommand RenameFolderCommand { get; }

        #endregion

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

        private void CopyFolder()
        {

        }

        private void DownloadFolder()
        {
            var folder = this.Folder.FolderRootNode as NodeViewModel;
            folder?.Download(TransferService.MegaTransfers);
        }

        private void ImportFolder()
        {
            (this.Folder as FolderViewModel)?.ImportFolder();
        }

        private void ShowFolderInformation()
        {

        }

        private async void RenameFolder()
        {
            var folder = this.Folder.FolderRootNode as NodeViewModel;
            if (folder == null) return;
            await folder.RenameAsync();
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
            if (this.Folder is IncomingSharesViewModel || this.Folder is OutgoingSharesViewModel || 
                this.Folder?.FolderRootNode == null) return;

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

        public IBaseNode FolderRootNode;

        private BaseFolderViewModel _folder;
        public BaseFolderViewModel Folder
        {
            get { return _folder; }
            set
            {
                if (_folder != null)
                    this.Deinitialize();

                SetField(ref _folder, value);
                this.FolderRootNode = this._folder.FolderRootNode;
                OnPropertyChanged(nameof(this.ItemCollection),
                    nameof(this.FolderOptionsButtonVisibility));

                if (_folder != null)
                    this.Initialize();
            }
        }

        public CollectionViewModel<IBaseNode> ItemCollection => this.Folder?.ItemCollection;

        public string OrderTypeAndNumberOfItems => this.Folder?.OrderTypeAndNumberOfItems;

        public string OrderTypeAndNumberOfSelectedItems => this.Folder?.OrderTypeAndNumberOfSelectedItems;

        public bool IsRenameFolderOptionAvailable
        {
            get
            {
                if (this.Folder?.Type == ContainerType.FolderLink) return false;

                if (this.FolderRootNode is IncomingSharedFolderNodeViewModel)
                {
                    var folderRootNode = this.FolderRootNode as IncomingSharedFolderNodeViewModel;
                    if (folderRootNode?.AccessLevel != null && (int)folderRootNode.AccessLevel.AccessType < (int)MShareType.ACCESS_FULL)
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets the "Folder options" button visibility
        /// </summary>
        public Visibility FolderOptionsButtonVisibility
        {
            get
            {
                switch(this.Folder?.Type)
                {
                    case ContainerType.FolderLink:
                    case ContainerType.InShares:
                    case ContainerType.OutShares:
                    case ContainerType.ContactInShares:
                        return Visibility.Visible;                    
                }

                return Visibility.Collapsed;
            }
        }

        #endregion

        #region UiResources

        public string FolderOptionsText => ResourceService.UiResources.GetString("UI_FolderOptions");
        public string SelectOrDeselectAllText => ResourceService.UiResources.GetString("UI_SelectOrDeselectAll");

        #endregion

        #region VisualResources

        public string BreadcrumbHomeIcon => ResourceService.VisualResources.GetString("VR_FolderTypePath_default");

        #endregion
    }
}
