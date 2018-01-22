using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.SharedFolders
{
    public class SharedFoldersListViewModel : FolderViewModel
    {
        public SharedFoldersListViewModel(ContainerType containerType, bool isForSelectFolder = false) : 
            base(SdkService.MegaSdk, containerType, isForSelectFolder)
        {
            this.LeaveShareCommand = new RelayCommand(LeaveShare);
            this.RemoveSharedAccessCommand = new RelayCommand(RemoveSharedAccess);

            this.ClosePanelCommand = new RelayCommand(ClosePanels);
            this.OpenContentPanelCommand = new RelayCommand(OpenContentPanel);
            this.OpenInformationPanelCommand = new RelayCommand(OpenInformationPanel);

            this.ItemCollection.ItemCollectionChanged += OnFolderListViewStateChanged;
            this.ItemCollection.SelectedItemsCollectionChanged += OnFolderListViewStateChanged;
        }

        #region Commands

        public ICommand LeaveShareCommand { get; }
        public ICommand RemoveSharedAccessCommand { get; }

        public ICommand OpenContentPanelCommand { get; }

        #endregion

        #region Methods

        private void OnFolderListViewStateChanged(object sender, EventArgs args)
        {
            OnUiThread(() => OnPropertyChanged(nameof(this.SharedFolders)));
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

            // If node isn't found in current view, don't process the remove action
            if (node == null) return;

            try
            {
                OnUiThread(() =>
                {
                    this.ItemCollection.Items.Remove(node);

                    if (this.ItemCollection.FocusedItem?.Equals(node) == true)
                        this.ClosePanels();
                });
            }
            catch (Exception) { /* Dummy catch, supress possible exception */ }
        }

        private async void LeaveShare()
        {
            if (!this.ItemCollection.HasSelectedItems) return;

            if (this.ItemCollection.OnlyOneSelectedItem)
            {
                var node = this.ItemCollection.SelectedItems.First();

                var dialogResult = await DialogService.ShowOkCancelAsync(
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

                var dialogResult = await DialogService.ShowOkCancelAsync(
                    ResourceService.AppMessages.GetString("AM_LeaveMultipleSharedFolders_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_LeaveMultipleSharedFoldersQuestion"), count),
                    ResourceService.AppMessages.GetString("AM_LeaveSharedFolderWarning"),
                    this.LeaveText, this.CancelText);

                if (!dialogResult) return;

                // Use a temp variable to avoid InvalidOperationException
                LeaveMultipleSharedFolders(this.ItemCollection.SelectedItems.ToList());
            }
        }

        private async void LeaveMultipleSharedFolders(ICollection<IMegaNode> sharedFolders)
        {
            if (sharedFolders?.Count < 1) return;

            bool result = true;
            if (sharedFolders != null)
            {
                foreach (var node in sharedFolders)
                    result = result & await node.RemoveAsync(true);
            }

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

                var dialogResult = await DialogService.ShowOkCancelAsync(
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

                var dialogResult = await DialogService.ShowOkCancelAsync(
                    ResourceService.AppMessages.GetString("AM_RemoveAccessMultipleSharedFolders_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_RemoveAccessMultipleSharedFoldersQuestion"), count),
                    ResourceService.AppMessages.GetString("AM_RemoveAccessMultipleSharedFoldersWarning"),
                    this.RemoveText, this.CancelText);

                if (!dialogResult) return;

                // Use a temp variable to avoid InvalidOperationException
                RemoveAccessMultipleSharedFolders(this.ItemCollection.SelectedItems.ToList());
            }
        }

        private async void RemoveAccessMultipleSharedFolders(ICollection<IMegaNode> sharedFolders)
        {
            if (sharedFolders?.Count < 1) return;

            bool result = true;
            if (sharedFolders != null)
            {
                foreach (var node in sharedFolders)
                    result = result & await (node as IMegaOutgoingSharedFolderNode).RemoveSharedAccessAsync();
            }

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

        private void OpenContentPanel()
        {
            this.VisiblePanel = PanelType.Content;
        }

        #endregion

        #region Properties

        public FolderViewModel SharedFolders => this;

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

        #endregion

        #region UiResources

        public string GetLinkText => ResourceService.UiResources.GetString("UI_GetLink");
        public string InformationText => ResourceService.UiResources.GetString("UI_Information");
        public string LeaveShareText => ResourceService.UiResources.GetString("UI_LeaveShare");
        public string OpenText => ResourceService.UiResources.GetString("UI_Open");
        public string RemoveSharedAccessText => ResourceService.UiResources.GetString("UI_RemoveSharedAccess");
        public string SharedFoldersText => ResourceService.UiResources.GetString("UI_SharedFolders");
        public string SelectOrDeselectAllText => ResourceService.UiResources.GetString("UI_SelectOrDeselectAll");
        public string ManageCollaboratorsText => ResourceService.UiResources.GetString("UI_ManageCollaborators");

        private string LeaveText => ResourceService.UiResources.GetString("UI_Leave");

        #endregion

        #region VisualResources

        public string LeaveSharePathData => ResourceService.VisualResources.GetString("VR_LeaveSharePathData");
        public string LinkPathData => ResourceService.VisualResources.GetString("VR_LinkPathData");
        public string InformationPathData => ResourceService.VisualResources.GetString("VR_InformationPathData");
        public string ManageSharePathData => ResourceService.VisualResources.GetString("VR_ManageSharePathData");

        #endregion
    }
}
