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

            this.CloudDrive.AcceptCopyEvent += OnAcceptCopy;
            this.RubbishBin.AcceptCopyEvent += OnAcceptCopy;

            this.CloudDrive.AcceptMoveEvent += OnAcceptMove;
            this.RubbishBin.AcceptMoveEvent += OnAcceptMove;

            this.CloudDrive.CancelCopyOrMoveEvent += OnCancelCopyOrMove;
            this.RubbishBin.CancelCopyOrMoveEvent += OnCancelCopyOrMove;

            this.CloudDrive.CopyOrMoveEvent += OnCopyOrMove;
            this.RubbishBin.CopyOrMoveEvent += OnCopyOrMove;

            this.RubbishBin.ChildNodesCollectionChanged += OnRubbishBinChildNodesCollectionChanged;

            // The Cloud Drive is always the first active folder on initalization
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
                    SdkService.MegaSdk.getRootNode(), this.CloudDrive);
            }

            this.CloudDrive.LoadChildNodes();

            if (this.RubbishBin?.FolderRootNode == null)
            {
                this.RubbishBin.FolderRootNode = 
                    NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, 
                    SdkService.MegaSdk.getRubbishNode(), this.RubbishBin);
            }

            this.RubbishBin.LoadChildNodes();
        }

        /// <summary>
        /// Load all content trees: nodes, shares, contacts
        /// </summary>
        public async void FetchNodes()
        {
            OnUiThread(() => this.CloudDrive?.SetEmptyContent(true));
            this.CloudDrive?.CancelLoad();

            OnUiThread(() => this.RubbishBin?.SetEmptyContent(true));
            this.RubbishBin?.CancelLoad();

            var fetchNodes = new FetchNodesRequestListenerAsync();
            //fetchNodes.ServerBusy += OnServerBusy;
            if (!await fetchNodes.ExecuteAsync(() => this.MegaSdk.fetchNodes(fetchNodes)))
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_FetchNodesFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_FetchNodesFailed"));
                return;
            }

            var cloudDriveRootNode = this.CloudDrive.FolderRootNode ??
                NodeService.CreateNew(this.MegaSdk, App.AppInformation,
                this.MegaSdk.getRootNode(), this.CloudDrive);
            var rubbishBinRootNode = this.RubbishBin.FolderRootNode ??
                NodeService.CreateNew(this.MegaSdk, App.AppInformation, 
                this.MegaSdk.getRubbishNode(), this.RubbishBin);

            UiService.OnUiThread(() =>
            {
                this.CloudDrive.FolderRootNode = cloudDriveRootNode;
                this.RubbishBin.FolderRootNode = rubbishBinRootNode;

                LoadFolders();
            });
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

            OnPropertyChanged("IsRubbishBinEmpty");
        }

        private void OnRubbishBinChildNodesCollectionChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("IsRubbishBinEmpty");
        }

        private void ResetViewStates()
        {
            CloudDrive.IsMultiSelectActive = false;
            CloudDrive.CurrentViewState = FolderContentViewState.CloudDrive;
            CloudDrive.PreviousViewState = FolderContentViewState.CloudDrive;

            RubbishBin.IsMultiSelectActive = false;
            RubbishBin.CurrentViewState = FolderContentViewState.RubbishBin;
            RubbishBin.PreviousViewState = FolderContentViewState.RubbishBin;
        }

        private void OnCopyOrMove(object sender, EventArgs e)
        {
            if (this.ActiveFolderView.SelectedNodes == null || !this.ActiveFolderView.SelectedNodes.Any()) return;

            this.ActiveFolderView.CloseNodeDetails();

            foreach (var node in this.ActiveFolderView.SelectedNodes)
                if (node != null) node.DisplayMode = NodeDisplayMode.SelectedForCopyOrMove;

            this.ActiveFolderView.CopyOrMoveSelectedNodes = this.ActiveFolderView.SelectedNodes.ToList();            
            this.ActiveFolderView.IsMultiSelectActive = false;

            ResetViewStates();

            this.CloudDrive.PreviousViewState = this.CloudDrive.CurrentViewState;
            this.CloudDrive.CurrentViewState = FolderContentViewState.CopyOrMove;
            this.RubbishBin.PreviousViewState = this.RubbishBin.CurrentViewState;
            this.RubbishBin.CurrentViewState = FolderContentViewState.CopyOrMove;

            this.SourceFolderView = this.ActiveFolderView;

            this.DisableSelection?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Reset the variables used in the copy or move actions
        /// </summary>
        private void ResetCopyOrMove()
        {
            SourceFolderView.SelectedNodes.Clear();
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
