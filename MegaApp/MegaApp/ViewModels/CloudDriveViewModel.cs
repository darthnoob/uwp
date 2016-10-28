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

        #region Public Methods

        public void LoadFolders()
        {
            if (this.CloudDrive.FolderRootNode == null)
                this.CloudDrive.FolderRootNode = NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, SdkService.MegaSdk.getRootNode(), ContainerType.CloudDrive);

            this.CloudDrive.LoadChildNodes();

            if (this.RubbishBin.FolderRootNode == null)
                this.RubbishBin.FolderRootNode = NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, SdkService.MegaSdk.getRubbishNode(), ContainerType.RubbishBin);

            this.RubbishBin.LoadChildNodes();
        }

        public void FetchNodes()
        {
            if (this.CloudDrive != null)
            {
                //OnUiThread(() => this.CloudDrive.SetEmptyContentTemplate(true));
                this.CloudDrive.CancelLoad();
            }

            if (this.RubbishBin != null)
            {
                //OnUiThread(() => this.RubbishBin.SetEmptyContentTemplate(true));
                this.RubbishBin.CancelLoad();
            }

            var fetchNodesRequestListener = new FetchNodesRequestListener(this);
            this.MegaSdk.fetchNodes(fetchNodesRequestListener);
        }

        private void InitializeModel()
        {
            this.CloudDrive = new FolderViewModel(ContainerType.CloudDrive);
            this.RubbishBin = new FolderViewModel(ContainerType.RubbishBin);

            // The Cloud Drive is always the first active folder on initalization
            this.ActiveFolderView = this.CloudDrive;
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

        #endregion

        #region UiResources

        public string CloudDriveNameText { get { return ResourceService.UiResources.GetString("UI_CloudDriveName"); } }
        public string RubbishBinNameText { get { return ResourceService.UiResources.GetString("UI_RubbishBinName"); } }

        #endregion

        #region VisualResources

        public string BreadcrumbHomeMegaIcon { get { return ResourceService.VisualResources.GetString("VR_BreadcrumbHomeMegaIcon"); } }
        public string BreadcrumbHomeRubbishBinIcon { get { return ResourceService.VisualResources.GetString("VR_BreadcrumbHomeRubbishBinIcon"); } }
        public string EmptyCloudDrivePathData { get { return ResourceService.VisualResources.GetString("VR_EmptyCloudDrivePathData"); } }
        public string EmptyFolderPathData { get { return ResourceService.VisualResources.GetString("VR_EmptyFolderPathData"); } }
        public string FolderLoadingPathData { get { return ResourceService.VisualResources.GetString("VR_FolderLoadingPathData"); } }

        #endregion
    }
}
