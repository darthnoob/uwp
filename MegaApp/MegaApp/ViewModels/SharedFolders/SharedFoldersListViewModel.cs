using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class SharedFoldersListViewModel : BaseSdkViewModel
    {
        public SharedFoldersListViewModel()
        {
            this.ItemCollection = new CollectionViewModel<IMegaSharedFolderNode>();
            
            this.DownloadCommand = new RelayCommand(Download);
            this.LeaveSharedCommand = new RelayCommand(LeaveShared);
        }

        #region Commands

        public ICommand DownloadCommand { get; }
        public ICommand LeaveSharedCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Cancel any running load process of contacts
        /// </summary>
        public void CancelLoad()
        {
            if (this.LoadingCancelTokenSource != null && LoadingCancelToken.CanBeCanceled)
                LoadingCancelTokenSource.Cancel();
        }

        protected void CreateLoadCancelOption()
        {
            if (this.LoadingCancelTokenSource != null)
            {
                this.LoadingCancelTokenSource.Dispose();
                this.LoadingCancelTokenSource = null;
            }
            this.LoadingCancelTokenSource = new CancellationTokenSource();
            this.LoadingCancelToken = LoadingCancelTokenSource.Token;
        }

        private async void Download()
        {
            if (!this.ItemCollection.HasSelectedItems) return;
            await MultipleDownloadAsync(this.ItemCollection.SelectedItems);
            this.ItemCollection.IsMultiSelectActive = false;
        }

        private async Task MultipleDownloadAsync(ICollection<IMegaSharedFolderNode> nodes)
        {
            if (nodes?.Count < 1) return;

            var downloadFolder = await FolderService.SelectFolder();
            if (downloadFolder != null)
            {
                if (await TransferService.CheckExternalDownloadPathAsync(downloadFolder.Path))
                {
                    foreach (var node in nodes)
                    {
                        node.Transfer.ExternalDownloadPath = downloadFolder.Path;
                        TransferService.MegaTransfers.Add(node.Transfer);
                        node.Transfer.StartTransfer();
                    }
                }
            }
        }

        private async void LeaveShared()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            if (this.ItemCollection.OnlyOneSelectedItem)
            {
                var node = this.ItemCollection.SelectedItems.First();

                var dialogResult = await DialogService.ShowOkCancelAndWarningAsync(
                    ResourceService.AppMessages.GetString("AM_LeaveSharedFolder_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_LeaveSharedFolderQuestion"), node.Name),
                    ResourceService.AppMessages.GetString("AM_LeaveSharedFolderWarning"),
                    this.LeaveText, this.CancelText);

                if (!dialogResult) return;

                if (!await node.RemoveAsync())
                {
                    OnUiThread(async () =>
                    {
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_LeaveSharedFolder_Title"),
                            string.Format(ResourceService.AppMessages.GetString("AM_LeaveSharedFolderFailed"), node.Name));
                    });
                }
            }
            else
            {
                var count = this.ItemCollection.SelectedItems.Count;

                var dialogResult = await DialogService.ShowOkCancelAndWarningAsync(
                    ResourceService.AppMessages.GetString("AM_LeaveMultipleSharedFolders_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_LeaveMultipleSharedFoldersQuestion"), count),
                    ResourceService.AppMessages.GetString("AM_LeaveSharedFolderWarning"),
                    this.LeaveText, this.CancelText);

                if (!dialogResult) return;

                // Use a temp variable to avoid InvalidOperationException
                LeaveMultipleSharedFolders(this.ItemCollection.SelectedItems.ToList());
            }
        }

        private async void LeaveMultipleSharedFolders(ICollection<IMegaSharedFolderNode> sharedFolders)
        {
            if (sharedFolders?.Count < 1) return;

            bool result = true;
            foreach (var node in sharedFolders)
                result = result & (await node.RemoveAsync(true));

            if (!result)
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_LeaveMultipleSharedFolder_Title"),
                        ResourceService.AppMessages.GetString("AM_RemoveMultipleContactsFailed"));
                });
            }
        }

        #endregion

        #region Properties

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        protected CancellationToken LoadingCancelToken { get; set; }

        private CollectionViewModel<IMegaSharedFolderNode> _itemCollection;
        /// <summary>
        /// Folders shared with or by the contact
        /// </summary>
        public CollectionViewModel<IMegaSharedFolderNode> ItemCollection
        {
            get { return _itemCollection; }
            set { SetField(ref _itemCollection, value); }
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

        #endregion

        #region UiResources

        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string LeaveSharedText => ResourceService.UiResources.GetString("UI_LeaveShared");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string SharedFoldersText => ResourceService.UiResources.GetString("UI_SharedFolders");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");

        private string LeaveText => ResourceService.UiResources.GetString("UI_Leave");

        #endregion

        #region VisualResources

        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string LeaveSharedPathData => ResourceService.VisualResources.GetString("VR_LeaveSharedPathData");
        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");

        #endregion
    }
}
