using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class CloudDriveViewModel: BaseSdkViewModel
    {
        public CloudDriveViewModel() : base(SdkService.MegaSdk)
        {
            InitializeModel();

            this.CleanRubbishBinCommand = new RelayCommand(CleanRubbishBin);
        }

        /// <summary>
        /// Initialize the view model
        /// </summary>
        private void InitializeModel()
        {
            this.CloudDrive = new FolderViewModel(this.MegaSdk, ContainerType.CloudDrive);
            this.RubbishBin = new FolderViewModel(this.MegaSdk, ContainerType.RubbishBin);
            this.CameraUploads = new CameraUploadsViewModel();

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
            globalListener.OutSharedFolderAdded += CloudDrive.OnNodeAdded;
            globalListener.OutSharedFolderRemoved += CloudDrive.OnNodeAdded;

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
            globalListener.OutSharedFolderAdded -= CloudDrive.OnNodeAdded;
            globalListener.OutSharedFolderRemoved -= CloudDrive.OnNodeAdded;

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
                    NodeService.CreateNew(this.MegaSdk, App.AppInformation,
                    this.MegaSdk.getRootNode(), this.CloudDrive);
            }

            if(this.ActiveFolderView.Equals(this.CloudDrive))
                this.CloudDrive.LoadChildNodes();

            if (this.RubbishBin?.FolderRootNode == null)
            {
                this.RubbishBin.FolderRootNode = 
                    NodeService.CreateNew(this.MegaSdk, App.AppInformation,
                    this.MegaSdk.getRubbishNode(), this.RubbishBin);
            }

            if (this.ActiveFolderView.Equals(this.RubbishBin))
                this.RubbishBin.LoadChildNodes();

            if (this.CameraUploads?.FolderRootNode == null)
            {
                var cameraUploadsNode = await SdkService.GetCameraUploadRootNodeAsync();
                this.CameraUploads.FolderRootNode =
                    NodeService.CreateNew(this.MegaSdk, App.AppInformation,
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
