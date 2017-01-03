using System;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class CloudDriveViewModel: BaseSdkViewModel
    {
        public CloudDriveViewModel() : base()
        {
            InitializeModel();
        }

        /// <summary>
        /// Initialize the view model
        /// </summary>
        private void InitializeModel()
        {
            this.CloudDrive = new FolderViewModel(ContainerType.CloudDrive);
            this.RubbishBin = new FolderViewModel(ContainerType.RubbishBin);

            // The Cloud Drive is always the first active folder on initalization
            this.ActiveFolderView = this.CloudDrive;
        }

        #region Public Methods

        /// <summary>
        /// Add folders to global listener to receive notifications
        /// </summary>
        /// <param name="globalListener">Global notifications listener</param>
        public void Initialize(GlobalListener globalListener)
        {
            globalListener?.Folders?.Add(this.CloudDrive);
            globalListener?.Folders?.Add(this.RubbishBin);
        }

        /// <summary>
        /// Remove folders from global listener
        /// </summary>
        /// <param name="globalListener">Global notifications listener</param>
        public void Deinitialize(GlobalListener globalListener)
        {
            globalListener?.Folders?.Remove(this.CloudDrive);
            globalListener?.Folders?.Remove(this.RubbishBin);
        }

        /// <summary>
        /// Load folders of the view model
        /// </summary>
        public void LoadFolders()
        {
            if (this.CloudDrive?.FolderRootNode == null)
            {
                this.CloudDrive.FolderRootNode = 
                    NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, 
                    SdkService.MegaSdk.getRootNode(), ContainerType.CloudDrive);
            }

            this.CloudDrive.LoadChildNodes();

            if (this.RubbishBin?.FolderRootNode == null)
            {
                this.RubbishBin.FolderRootNode = 
                    NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, 
                    SdkService.MegaSdk.getRubbishNode(), ContainerType.RubbishBin);
            }

            this.RubbishBin.LoadChildNodes();
        }

        /// <summary>
        /// Load all content trees: nodes, shares, contacts
        /// </summary>
        public void FetchNodes()
        {
            OnUiThread(() => this.CloudDrive?.SetEmptyContentTemplate(true));
            this.CloudDrive?.CancelLoad();

            OnUiThread(() => this.RubbishBin?.SetEmptyContentTemplate(true));
            this.RubbishBin?.CancelLoad();

            var fetchNodesRequestListener = new FetchNodesRequestListener(this);
            this.MegaSdk.fetchNodes(fetchNodesRequestListener);
        }

        public async void AcceptCopyAction()
        {
            bool result = true;
            try
            {
                // Copy all the selected nodes and then clear and release the selected nodes list
                if (SourceFolderView?.SelectedNodes?.Count > 0)
                {
                    // Fix the new parent node to allow navigation while the nodes are being copied
                    var newParentNode = ActiveFolderView.FolderRootNode;
                    foreach (var node in SourceFolderView.SelectedNodes)
                    {
                        if (node != null)
                        {
                            result = result & (await node.CopyAsync(newParentNode) == NodeActionResult.Succeeded);
                            node.DisplayMode = NodeDisplayMode.Normal;
                        }
                    }
                    SourceFolderView.SelectedNodes.Clear();
                }

                // Release the focused node
                if (SourceFolderView?.FocusedNode != null)
                {
                    SourceFolderView.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
                    SourceFolderView.FocusedNode = null;
                }
            }
            catch (InvalidOperationException)
            {
                result = false;
            }
            finally
            {
                if(!result)
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_CopyFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_CopyFailed"));
                }

                SourceFolderView = null;
            }
        }

        public async void AcceptMoveAction()
        {
            bool result = true;
            try
            {
                // Copy all the selected nodes and then clear and release the selected nodes list
                if (SourceFolderView?.SelectedNodes?.Count > 0)
                {
                    // Fix the new parent node to allow navigation while the nodes are being moved
                    var newParentNode = ActiveFolderView.FolderRootNode;
                    foreach (var node in SourceFolderView.SelectedNodes)
                    {
                        if (node != null)
                        {
                            result = result & (await node.MoveAsync(newParentNode) == NodeActionResult.Succeeded);
                            node.DisplayMode = NodeDisplayMode.Normal;
                        }
                    }
                    SourceFolderView.SelectedNodes.Clear();
                }

                // Release the focused node
                if (SourceFolderView?.FocusedNode != null)
                {
                    SourceFolderView.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
                    SourceFolderView.FocusedNode = null;
                }
            }
            catch (InvalidOperationException)
            {
                result = false;
            }
            finally
            {
                if(!result)
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_MoveFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_MoveFailed"));
                }

                SourceFolderView = null;
            }
        }

        #endregion

        #region Properties

        private FolderViewModel _cloudDrive;
        public FolderViewModel CloudDrive
        {
            get { return _cloudDrive; }
            private set { SetField(ref _cloudDrive, value); }
        }

        private FolderViewModel _rubbishBin;
        public FolderViewModel RubbishBin
        {
            get { return _rubbishBin; }
            private set { SetField(ref _rubbishBin, value); }
        }

        private FolderViewModel _activeFolderView;
        public FolderViewModel ActiveFolderView
        {
            get { return _activeFolderView; }
            set { SetField(ref _activeFolderView, value); }
        }

        /// <summary>
        /// Property needed to store the source folder in a move/copy action 
        /// </summary>
        private FolderViewModel _sourceFolderView;
        public FolderViewModel SourceFolderView
        {
            get { return _sourceFolderView; }
            set { SetField(ref _sourceFolderView, value); }
        }

        #endregion

        #region UiResources

        public string AddFolderText => ResourceService.UiResources.GetString("UI_AddFolder");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string CloudDriveNameText => ResourceService.UiResources.GetString("UI_CloudDriveName");
        public string CopyOrMoveText => CopyText + "/" + MoveText.ToLower();
        public string CopyText => ResourceService.UiResources.GetString("UI_Copy");
        public string DeselectAllText => ResourceService.UiResources.GetString("UI_DeselectAll");
        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string EmptyRubbishBinText => ResourceService.UiResources.GetString("UI_EmptyRubbishBin");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string MoveText => ResourceService.UiResources.GetString("UI_Move");
        public string MoveToRubbishBinText => ResourceService.UiResources.GetString("UI_MoveToRubbishBin");
        public string RemoveText => ResourceService.UiResources.GetString("UI_Remove");
        public string RefreshText => ResourceService.UiResources.GetString("UI_Refresh");        
        public string RubbishBinNameText => ResourceService.UiResources.GetString("UI_RubbishBinName");
        public string SelectAllText => ResourceService.UiResources.GetString("UI_SelectAll");
        public string UploadText => ResourceService.UiResources.GetString("UI_Upload");

        #endregion

        #region VisualResources

        public string BreadcrumbHomeMegaIcon => ResourceService.VisualResources.GetString("VR_BreadcrumbHomeMegaIcon");
        public string BreadcrumbHomeRubbishBinIcon => ResourceService.VisualResources.GetString("VR_BreadcrumbHomeRubbishBinIcon");
        public string EmptyCloudDrivePathData => ResourceService.VisualResources.GetString("VR_EmptyCloudDrivePathData");
        public string EmptyFolderPathData => ResourceService.VisualResources.GetString("VR_EmptyFolderPathData");
        public string FolderLoadingPathData => ResourceService.VisualResources.GetString("VR_FolderLoadingPathData");

        #endregion
    }
}
