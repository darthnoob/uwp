using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
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
            this.LeaveShareCommand = new RelayCommand(LeaveShare);
            this.RemoveSharedAccessCommand = new RelayCommand(RemoveSharedAccess);

            this.CloseInformationPanelCommand = new RelayCommand(CloseInformationPanel);
            this.OpenInformationPanelCommand = new RelayCommand(OpenInformationPanel);
        }

        #region Commands

        public ICommand DownloadCommand { get; }
        public ICommand LeaveShareCommand { get; }
        public ICommand RemoveSharedAccessCommand { get; }

        public ICommand CloseInformationPanelCommand { get; }
        public ICommand OpenInformationPanelCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Cancel any running load process of contacts
        /// </summary>
        protected void CancelLoad()
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

        protected void OnSharedFolderAdded(object sender, MNode megaNode)
        {
            if (megaNode == null) return;

            var node = this.ItemCollection.Items.FirstOrDefault(
                n => n.Base64Handle.Equals(megaNode.getBase64Handle()));

            // If exists update it
            if (node != null)
            {
                try { OnUiThread(() => node.Update(megaNode, true)); }
                catch (Exception) { /* Dummy catch, supress possible exception */ }
            }
            else
            {
                try
                {
                    OnUiThread(() =>
                    {
                        this.ItemCollection.Items.Add(NodeService.CreateNewSharedFolder(
                            this.MegaSdk, App.AppInformation, megaNode, this));
                    });
                }
                catch (Exception) { /* Dummy catch, supress possible exception */ }
            }
        }

        protected void OnSharedFolderRemoved(object sender, MNode megaNode)
        {
            if (megaNode == null) return;

            var node = this.ItemCollection.Items.FirstOrDefault(
                n => n.Base64Handle.Equals(megaNode.getBase64Handle()));

            // If node is found in current view, process the remove action
            if (node != null)
            {
                try { OnUiThread(() => this.ItemCollection.Items.Remove(node)); }
                catch (Exception) { /* Dummy catch, supress possible exception */ }
            }
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

        private async void LeaveShare()
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

        private async void RemoveSharedAccess()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            if (this.ItemCollection.OnlyOneSelectedItem)
            {
                var node = this.ItemCollection.SelectedItems.First() as IMegaOutgoingSharedFolderNode;

                var dialogResult = await DialogService.ShowOkCancelAndWarningAsync(
                    ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolder_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolderQuestion"), node.Name),
                    ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolderWarning"),
                    this.RemoveText, this.CancelText);

                if (!dialogResult) return;

                if (!await node.RemoveSharedAccessAsync())
                {
                    OnUiThread(async () =>
                    {
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolder_Title"),
                            string.Format(ResourceService.AppMessages.GetString("AM_RemoveAccessSharedFolderFailed"), node.Name));
                    });
                }
            }
            else
            {
                var count = this.ItemCollection.SelectedItems.Count;

                var dialogResult = await DialogService.ShowOkCancelAndWarningAsync(
                    ResourceService.AppMessages.GetString("AM_RemoveAccessMultipleSharedFolders_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveAccessMultipleSharedFoldersQuestion"), count),
                    ResourceService.AppMessages.GetString("AM_RemoveAccessMultipleSharedFoldersWarning"),
                    this.RemoveText, this.CancelText);

                if (!dialogResult) return;

                // Use a temp variable to avoid InvalidOperationException
                RemoveAccessMultipleSharedFolders(this.ItemCollection.SelectedItems.ToList());
            }
        }

        private async void RemoveAccessMultipleSharedFolders(ICollection<IMegaSharedFolderNode> sharedFolders)
        {
            if (sharedFolders?.Count < 1) return;

            bool result = true;
            foreach (var node in sharedFolders)
                result = result & (await (node as IMegaOutgoingSharedFolderNode).RemoveSharedAccessAsync());

            if (!result)
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_RemoveAccessMultipleSharedFolders_Title"),
                        ResourceService.AppMessages.GetString("AM_RemoveAccessSharedMultipleSharedFailed"));
                });
            }
        }

        private void OpenInformationPanel() => this.IsPanelOpen = true;

        private void CloseInformationPanel() => this.IsPanelOpen = false;        

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
        public string NumberOfSharedItemsText => this.NumberOfSharedItems == 1 ? 
            ResourceService.UiResources.GetString("UI_OneSharedFolder").ToLower() :
            string.Format(ResourceService.UiResources.GetString("UI_NumberSharedFolders").ToLower(), this.NumberOfSharedItems);

        private bool _isPanelOpen;
        public bool IsPanelOpen
        {
            get { return _isPanelOpen; }
            set
            {
                SetField(ref _isPanelOpen, value);

                if (this.IsPanelOpen)
                {
                    this.ItemCollection.IsMultiSelectActive = false;
                    this.ItemCollection.IsOnlyAllowSingleSelectActive = true;
                }
                else
                {
                    this.ItemCollection.IsOnlyAllowSingleSelectActive = false;
                }
            }
        }

        #endregion

        #region UiResources

        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string ClosePanelText => ResourceService.UiResources.GetString("UI_ClosePanel");
        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string InformationText => ResourceService.UiResources.GetString("UI_Information");
        public string LeaveShareText => ResourceService.UiResources.GetString("UI_LeaveShare");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string RemoveText => ResourceService.UiResources.GetString("UI_Remove");
        public string RemoveSharedAccessText => ResourceService.UiResources.GetString("UI_RemoveSharedAccess");
        public string SharedFoldersText => ResourceService.UiResources.GetString("UI_SharedFolders");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");
        

        private string LeaveText => ResourceService.UiResources.GetString("UI_Leave");

        #endregion

        #region VisualResources

        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string LeaveSharePathData => ResourceService.VisualResources.GetString("VR_LeaveSharePathData");
        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");
        public string ViewDetailsPathData => ResourceService.VisualResources.GetString("VR_ViewDetailsPathData");

        #endregion
    }
}
