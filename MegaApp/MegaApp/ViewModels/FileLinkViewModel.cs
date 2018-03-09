using System;
using System.ComponentModel;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    public class FileLinkViewModel : BaseSdkViewModel
    {
        public FileLinkViewModel() : base(SdkService.MegaSdk)
        {
            this.VisiblePanel = PanelType.None;

            this.ClosePanelCommand = new RelayCommand(ClosePanels);
            this.DownloadCommand = new RelayCommand(Download);
            this.ImportCommand = new RelayCommand(Import);
        }

        #region Commands

        public ICommand ClosePanelCommand { get; set; }
        public ICommand DownloadCommand { get; }
        public ICommand ImportCommand { get; }

        #endregion

        #region Methods

        public async void GetPublicNode(string link)
        {
            if (string.IsNullOrWhiteSpace(link)) return;

            PublicLinkService.Link = link;

            if (_getPublicNode == null)
                _getPublicNode = new GetPublicNodeRequestListenerAsync();

            this.ControlState = false;
            this.IsBusy = true;

            var result = await _getPublicNode.ExecuteAsync(() =>
                this.MegaSdk.getPublicNode(PublicLinkService.Link, _getPublicNode));

            bool navigateBack = true;
            switch (result)
            {
                case GetPublicNodeResult.Success:
                    LinkInformationService.PublicNode = _getPublicNode.PublicNode;
                    this.Node = NodeService.CreateNew(this.MegaSdk, App.AppInformation,
                        LinkInformationService.PublicNode, null);
                    if (this.Node is ImageNodeViewModel)
                    {
                        (this.Node as ImageNodeViewModel).SetThumbnailImage();
                        (this.Node as ImageNodeViewModel).SetPreviewImage();
                    }
                    navigateBack = false;
                    break;

                case GetPublicNodeResult.InvalidHandleOrDecryptionKey:
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Get public node failed. Invalid handle or decryption key.");
                    PublicLinkService.ShowLinkNoValidAlert();
                    break;

                case GetPublicNodeResult.InvalidDecryptionKey:
                    PublicLinkService.Link = await PublicLinkService.ShowDecryptionKeyNotValidAlertAsync();
                    if (PublicLinkService.Link != null)
                    {
                        this.GetPublicNode(PublicLinkService.Link);
                        return;
                    }
                    break;

                case GetPublicNodeResult.NoDecryptionKey:
                    PublicLinkService.Link = await PublicLinkService.ShowDecryptionAlertAsync();
                    this._getPublicNode.DecryptionAlert = true;
                    if (PublicLinkService.Link != null)
                    {
                        this.GetPublicNode(PublicLinkService.Link);
                        return;
                    }
                    break;

                case GetPublicNodeResult.UnavailableLink:
                    this.ShowUnavailableFileLinkAlert();
                    break;

                case GetPublicNodeResult.AssociatedUserAccountTerminated:
                    PublicLinkService.ShowAssociatedUserAccountTerminatedAlert();
                    break;

                case GetPublicNodeResult.Unknown:
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Get public node failed.");
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_GetPublicNodeFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_GetPublicNodeFailed"));
                    break;
            }

            this.ControlState = true;
            this.IsBusy = false;

            if (!navigateBack) return;

            OnUiThread(() =>
            {
                // Navigate to the Cloud Drive page
                NavigateService.Instance.Navigate(typeof(CloudDrivePage), false,
                    NavigationObject.Create(this.GetType()));
            });
        }

        private async void ShowUnavailableFileLinkAlert()
        {
            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_LinkUnavailableTitle"),
                ResourceService.AppMessages.GetString("AM_FileLinkUnavailable"));
        }

        private void ClosePanels()
        {
            SelectedNodesService.SelectedNodes.Clear();
            this.VisiblePanel = PanelType.None;
        }

        private void Download()
        {
            this.Node?.Download(TransferService.MegaTransfers);
        }

        private void Import()
        {
            SelectedNodesService.SelectedNodes.Clear();
            SelectedNodesService.SelectedNodes.Add(this.Node);
            this.VisiblePanel = PanelType.CopyMoveImport;
        }

        private void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(this.Node));

            if (e.PropertyName.Equals(nameof(this.Node.ThumbnailImageUri)))
                OnPropertyChanged(nameof(this.Node.ThumbnailImageUri));

            if (e.PropertyName.Equals(nameof(this.Node.PreviewImageUri)))
                OnPropertyChanged(nameof(this.Node.PreviewImageUri));
        }

        #endregion

        #region Properties

        private GetPublicNodeRequestListenerAsync _getPublicNode;

        private NodeViewModel _node;
        public NodeViewModel Node
        {
            get { return _node; }
            set
            {
                if (_node != null)
                    _node.PropertyChanged -= OnNodePropertyChanged;

                SetField(ref _node, value);

                if (_node != null)
                    _node.PropertyChanged += OnNodePropertyChanged;
            }
        }

        public bool IsPanelOpen => this.VisiblePanel != PanelType.None;

        private PanelType _visiblePanel;
        public PanelType VisiblePanel
        {
            get { return _visiblePanel; }
            set
            {
                SetField(ref _visiblePanel, value);
                OnPropertyChanged(nameof(this.IsPanelOpen));
            }
        }

        #endregion

        #region UiResources

        public string SectionNameText => ResourceService.UiResources.GetString("UI_FileLink");

        public string ClosePanelText => ResourceService.UiResources.GetString("UI_ClosePanel");
        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string ImportText => ResourceService.UiResources.GetString("UI_Import");

        public string DateCreatedLabelText => ResourceService.UiResources.GetString("UI_DateCreated");
        public string DateModifiedLabelText => ResourceService.UiResources.GetString("UI_DateModified");
        public string SizeLabelText => ResourceService.UiResources.GetString("UI_Size");
        public string TypeLabelText => ResourceService.UiResources.GetString("UI_Type");

        #endregion

        #region VisualResources

        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string ImportPathData => ResourceService.VisualResources.GetString("VR_ImportPathData");

        #endregion
    }
}
