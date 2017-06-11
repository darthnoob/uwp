using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class CloudDriveViewModel: BaseSdkViewModel
    {
        public event EventHandler ClearSelectedItems;
        public event EventHandler DisableSelection;
        public event EventHandler EnableSelection;

        public CloudDriveViewModel()
        {
            InitializeModel();

            this.CleanRubbishBinCommand = new RelayCommand(CleanRubbishBin);
        }

        /// <summary>
        /// Initialize the view model
        /// </summary>
        private void InitializeModel()
        {
            this.CloudDrive = new FolderViewModel(ContainerType.CloudDrive);
            this.RubbishBin = new FolderViewModel(ContainerType.RubbishBin);
            this.CameraUploads = new CameraUploadsViewModel();

            this.CloudDrive.AcceptCopyEvent += OnAcceptCopy;
            this.RubbishBin.AcceptCopyEvent += OnAcceptCopy;

            this.CloudDrive.AcceptMoveEvent += OnAcceptMove;
            this.RubbishBin.AcceptMoveEvent += OnAcceptMove;

            this.CloudDrive.CancelCopyOrMoveEvent += OnCancelCopyOrMove;
            this.RubbishBin.CancelCopyOrMoveEvent += OnCancelCopyOrMove;

            this.CloudDrive.CopyOrMoveEvent += OnCopyOrMove;
            this.RubbishBin.CopyOrMoveEvent += OnCopyOrMove;
            this.CameraUploads.CopyOrMoveEvent += OnCopyOrMove;

            this.RubbishBin.ChildNodesCollectionChanged += OnRubbishBinChildNodesCollectionChanged;

            // The Cloud Drive is always the first active folder on initialization
            this.ActiveFolderView = this.CloudDrive;
        }

        #region Commands

        public ICommand CleanRubbishBinCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add folders to global listener to receive notifications
        /// </summary>
        /// <param name="globalListener">Global notifications listener</param>
        public void Initialize(GlobalListener globalListener)
        {
            if (globalListener == null) return;
            globalListener.NodeAdded += CloudDrive.OnNodeAdded;
            globalListener.NodeRemoved += CloudDrive.OnNodeRemoved;
            globalListener.NodeAdded += RubbishBin.OnNodeAdded;
            globalListener.NodeRemoved += RubbishBin.OnNodeRemoved;
            globalListener.NodeAdded += CameraUploads.OnNodeAdded;
            globalListener.NodeRemoved += CameraUploads.OnNodeRemoved;
        }

        /// <summary>
        /// Remove folders from global listener
        /// </summary>
        /// <param name="globalListener">Global notifications listener</param>
        public void Deinitialize(GlobalListener globalListener)
        {
            if (globalListener == null) return;
            globalListener.NodeAdded -= CloudDrive.OnNodeAdded;
            globalListener.NodeRemoved -= CloudDrive.OnNodeRemoved;
            globalListener.NodeAdded -= RubbishBin.OnNodeAdded;
            globalListener.NodeRemoved -= RubbishBin.OnNodeRemoved;
            globalListener.NodeAdded -= CameraUploads.OnNodeAdded;
            globalListener.NodeRemoved -= CameraUploads.OnNodeRemoved;
        }

        /// <summary>
        /// Load folders of the view model
        /// </summary>
        public async void LoadFolders()
        {
            if (this.CloudDrive?.FolderRootNode == null)
            {
                this.CloudDrive.FolderRootNode = 
                    NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, 
                    SdkService.MegaSdk.getRootNode(), this.CloudDrive);
            }

            if(this.ActiveFolderView.Equals(this.CloudDrive))
                this.CloudDrive.LoadChildNodes();

            if (this.RubbishBin?.FolderRootNode == null)
            {
                this.RubbishBin.FolderRootNode = 
                    NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, 
                    SdkService.MegaSdk.getRubbishNode(), this.RubbishBin);
            }

            if (this.ActiveFolderView.Equals(this.RubbishBin))
                this.RubbishBin.LoadChildNodes();

            if (this.CameraUploads?.FolderRootNode == null)
            {
                var cameraUploadsNode = await SdkService.GetCameraUploadRootNodeAsync();
                this.CameraUploads.FolderRootNode =
                    NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation,
                        cameraUploadsNode, this.CameraUploads);
            }

            if (this.ActiveFolderView.Equals(this.CameraUploads))
                this.CameraUploads.LoadChildNodes();
        }

        #endregion

        #region Private Methods

        private async void CleanRubbishBin()
        {
            if (this.ActiveFolderView.Type != ContainerType.RubbishBin || this.IsRubbishBinEmpty) return;

            var dialogResult = await DialogService.ShowOkCancelAsync(
                ResourceService.AppMessages.GetString("AM_CleanRubbishBin_Title"),
                ResourceService.AppMessages.GetString("AM_CleanRubbishBinQuestion"));

            if (!dialogResult) return;

            if(this.ActiveFolderView.CanGoFolderUp())
                this.ActiveFolderView.BrowseToHome();

            var cleanRubbishBin = new CleanRubbishBinRequestListenerAsync();
            var result = await cleanRubbishBin.ExecuteAsync(() =>
            {
                this.MegaSdk.cleanRubbishBin(cleanRubbishBin);
            });

            if (!result)
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_CleanRubbishBin_Title"),
                    ResourceService.AppMessages.GetString("AM_CleanRubbishBinFailed"));
                return;
            }

            OnUiThread(() => OnPropertyChanged("IsRubbishBinEmpty"));
        }

        private void OnRubbishBinChildNodesCollectionChanged(object sender, EventArgs e)
        {
            OnUiThread(() => OnPropertyChanged("IsRubbishBinEmpty"));
        }

        private void ResetViewStates()
        {
            CloudDrive.IsMultiSelectActive = false;
            CloudDrive.CurrentViewState = FolderContentViewState.CloudDrive;
            CloudDrive.PreviousViewState = FolderContentViewState.CloudDrive;

            RubbishBin.IsMultiSelectActive = false;
            RubbishBin.CurrentViewState = FolderContentViewState.RubbishBin;
            RubbishBin.PreviousViewState = FolderContentViewState.RubbishBin;

            CameraUploads.IsMultiSelectActive = false;
            CameraUploads.CurrentViewState = FolderContentViewState.CameraUploads;
            CameraUploads.PreviousViewState = FolderContentViewState.CameraUploads;
        }

        private void OnCopyOrMove(object sender, EventArgs e)
        {
            if (this.ActiveFolderView.ItemCollection.SelectedItems == null || 
                !this.ActiveFolderView.ItemCollection.HasSelectedItems) return;

            this.ActiveFolderView.CloseNodeDetails();

            foreach (var node in this.ActiveFolderView.ItemCollection.SelectedItems)
                if (node != null) node.DisplayMode = NodeDisplayMode.SelectedForCopyOrMove;

            this.ActiveFolderView.CopyOrMoveSelectedNodes = this.ActiveFolderView.ItemCollection.SelectedItems.ToList();            
            this.ActiveFolderView.IsMultiSelectActive = false;

            ResetViewStates();

            this.CloudDrive.PreviousViewState = this.CloudDrive.CurrentViewState;
            this.CloudDrive.CurrentViewState = FolderContentViewState.CopyOrMove;
            this.RubbishBin.PreviousViewState = this.RubbishBin.CurrentViewState;
            this.RubbishBin.CurrentViewState = FolderContentViewState.CopyOrMove;
            this.CameraUploads.PreviousViewState = this.CameraUploads.CurrentViewState;
            this.CameraUploads.CurrentViewState = FolderContentViewState.CopyOrMove;

            this.SourceFolderView = this.ActiveFolderView;

            this.DisableSelection?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Reset the variables used in the copy or move actions
        /// </summary>
        private void ResetCopyOrMove()
        {
            SourceFolderView.ItemCollection.SelectedItems.Clear();
            SourceFolderView.CopyOrMoveSelectedNodes.Clear();
            SourceFolderView = null;
            ResetViewStates();
            ClearSelectedItems?.Invoke(this, EventArgs.Empty);
            EnableSelection?.Invoke(this, EventArgs.Empty);
        }

        private void OnCancelCopyOrMove(object sender, EventArgs e)
        {
            if (SourceFolderView?.CopyOrMoveSelectedNodes != null)
            {
                foreach (var node in SourceFolderView.CopyOrMoveSelectedNodes)
                    if (node != null) node.DisplayMode = NodeDisplayMode.Normal;
            }

            ResetCopyOrMove();
        }

        private void OnAcceptCopy(object sender, EventArgs e)
        {
            // Use a temp variable to avoid InvalidOperationException
            AcceptCopyAction(SourceFolderView.CopyOrMoveSelectedNodes.ToList());
            ResetCopyOrMove();
        }

        private async void AcceptCopyAction(IList<IMegaNode> nodes)
        {
            if (nodes == null || !nodes.Any()) return;

            bool result = true;
            try
            {
                // Fix the new parent node to allow navigation while the nodes are being copied
                var newParentNode = ActiveFolderView.FolderRootNode;
                foreach (var node in nodes)
                {
                    if (node != null)
                    {
                        result = result & (await node.CopyAsync(newParentNode) == NodeActionResult.Succeeded);
                        node.DisplayMode = NodeDisplayMode.Normal;
                    }
                }
            }
            catch (Exception) { result = false; }
            finally
            {
                if (!result)
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_CopyFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_CopyFailed"));
                }
            }
        }

        private void OnAcceptMove(object sender, EventArgs e)
        {
            // Use a temp variable to avoid InvalidOperationException
            AcceptMoveAction(SourceFolderView.CopyOrMoveSelectedNodes.ToList());
            ResetCopyOrMove();
        }

        private async void AcceptMoveAction(IList<IMegaNode> nodes)
        {
            if (nodes == null || !nodes.Any()) return;

            bool result = true;
            try
            {
                // Fix the new parent node to allow navigation while the nodes are being moved
                var newParentNode = ActiveFolderView.FolderRootNode;
                foreach (var node in nodes)
                {
                    if (node != null)
                    {
                        result = result & (await node.MoveAsync(newParentNode) == NodeActionResult.Succeeded);
                        node.DisplayMode = NodeDisplayMode.Normal;
                    }
                }
            }
            catch (Exception) { result = false; }
            finally
            {
                if (!result)
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_MoveFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_MoveFailed"));
                }
            }
        }

        #endregion

        #region Properties

        public bool IsRubbishBinEmpty => (this.MegaSdk.getNumChildren(this.MegaSdk.getRubbishNode()) == 0);

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

        private CameraUploadsViewModel _cameraUploads;
        public CameraUploadsViewModel CameraUploads
        {
            get { return _cameraUploads; }
            private set { SetField(ref _cameraUploads, value); }
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

        public string CameraUploadsNameText => ResourceService.UiResources.GetString("UI_CameraUploads");
        public string CloudDriveNameText => ResourceService.UiResources.GetString("UI_CloudDriveName");
        public string EmptyRubbishBinText => ResourceService.UiResources.GetString("UI_EmptyRubbishBin");
        public string RubbishBinNameText => ResourceService.UiResources.GetString("UI_RubbishBinName");

        #endregion

        #region VisualResources

        public string EmptyCloudDrivePathData => ResourceService.VisualResources.GetString("VR_EmptyCloudDrivePathData");
        public string EmptyFolderPathData => ResourceService.VisualResources.GetString("VR_EmptyFolderPathData");
        public string EmptyRubbishBinPathData => ResourceService.VisualResources.GetString("VR_RubbishBinPathData");
        public string FolderLoadingPathData => ResourceService.VisualResources.GetString("VR_FolderLoadingPathData");

        #endregion
    }
}
