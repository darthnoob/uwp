using System;
using System.Threading;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GoedWare.Controls.Breadcrumb;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public abstract class BaseFolderViewModel : BaseSdkViewModel
    {
        public BaseFolderViewModel(MegaSDK megaSdk, ContainerType containerType, bool isForSelectFolder = false)
            : base(megaSdk)
        {
            this.Type = containerType;
            this.IsForSelectFolder = isForSelectFolder;

            this.FolderRootNode = null;
            this.IsLoaded = false;
            this.IsBusy = false;
            this.BreadCrumb = new BreadCrumbViewModel(megaSdk);
            this.ItemCollection = new CollectionViewModel<IBaseNode>(megaSdk);

            this.ClosePanelCommand = new RelayCommand(ClosePanels);
            this.HomeSelectedCommand = new RelayCommand(BrowseToHome);
            this.ItemSelectedCommand = new RelayCommand<BreadcrumbEventArgs>(ItemSelected);
            this.OpenInformationPanelCommand = new RelayCommand(OpenInformationPanel);
        }

        #region Commands

        public ICommand ChangeViewCommand { get; set; }
        public ICommand ClosePanelCommand { get; set; }
        public ICommand HomeSelectedCommand { get; set; }
        public ICommand ItemSelectedCommand { get; set; }
        public ICommand OpenInformationPanelCommand { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the user navigates to the folder
        /// </summary>
        public event EventHandler FolderNavigatedTo;

        /// <summary>
        /// Event invocator method called the user navigates to the folder
        /// </summary>
        protected virtual void OnFolderNavigatedTo() => this.FolderNavigatedTo?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Event triggered when the user changes the view mode of the folder
        /// </summary>
        public event EventHandler ChangeViewEvent;

        /// <summary>
        /// Event invocator method called when the user changes the view mode of the folder
        /// </summary>
        protected virtual void OnChangeViewEvent() => this.ChangeViewEvent?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Methods

        public virtual void ClosePanels() => this.VisiblePanel = PanelType.None;

        public abstract void LoadChildNodes();

        /// <summary>
        /// Sets the view mode for the folder content.
        /// </summary>
        /// <param name="viewMode">View mode to set.</param>
        public virtual void SetView(FolderContentViewMode viewMode)
        {
            switch (viewMode)
            {
                case FolderContentViewMode.GridView:
                    this.NodeTemplateSelector = new NodeTemplateSelector()
                    {
                        FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeGridViewFileItemContent"],
                        FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeGridViewFolderItemContent"]
                    };

                    this.ViewMode = FolderContentViewMode.GridView;
                    this.NextViewButtonPathData = ResourceService.VisualResources.GetString("VR_ListViewPathData");
                    this.NextViewButtonLabelText = ResourceService.UiResources.GetString("UI_ListView");
                    break;

                case FolderContentViewMode.ListView:
                    SetViewDefaults();
                    break;
            }
        }

        public abstract void OnChildNodeTapped(IBaseNode baseNode);

        protected void BrowseToFolder(IBaseNode node)
        {
            if (node == null) return;

            this.ClosePanels();

            // Show the back button in desktop and tablet applications
            // Back button in mobile applications is automatic in the nav bar on screen
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            this.FolderRootNode = node;
            OnFolderNavigatedTo();

            this.LoadChildNodes();
        }

        public abstract void BrowseToHome();

        /// <summary>
        /// Cancel any running load process of this folder
        /// </summary>
        protected void CancelLoad()
        {
            if (this.LoadingCancelTokenSource != null && this.LoadingCancelToken.CanBeCanceled)
                this.LoadingCancelTokenSource.Cancel();
        }

        protected void CreateLoadCancelOption()
        {
            if (this.LoadingCancelTokenSource != null)
            {
                this.LoadingCancelTokenSource.Dispose();
                this.LoadingCancelTokenSource = null;
            }
            this.LoadingCancelTokenSource = new CancellationTokenSource();
            this.LoadingCancelToken = this.LoadingCancelTokenSource.Token;
        }

        protected void OpenInformationPanel() => this.VisiblePanel = PanelType.Information;

        protected void SetProgressIndication(bool onOff) => OnUiThread(() => this.IsBusy = onOff);

        /// <summary>
        /// Sets the view mode for the folder on load content.
        /// </summary>
        protected void SetViewOnLoad()
        {
            if (this.FolderRootNode == null) return;

            if (this.IsForSelectFolder)
            {
                SetViewDefaults();
                return;
            }

            SetView(UiService.GetViewMode(this.FolderRootNode.Base64Handle, this.FolderRootNode.Name));
        }

        /// <summary>
        /// Sets the default view mode for the folder content.
        /// </summary>
        protected virtual void SetViewDefaults()
        {
            this.ViewMode = FolderContentViewMode.ListView;
            this.NextViewButtonPathData = ResourceService.VisualResources.GetString("VR_GridViewPathData");
            this.NextViewButtonLabelText = ResourceService.UiResources.GetString("UI_GridView");
        }

        private void ItemSelected(BreadcrumbEventArgs e)
        {
            BrowseToFolder((IBaseNode)e.Item);
        }

        #endregion

        #region Properties

        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetField(ref _isLoaded, value); }
        }

        private string _emptyStateHeaderText;
        public string EmptyStateHeaderText
        {
            get { return _emptyStateHeaderText; }
            set { SetField(ref _emptyStateHeaderText, value); }
        }

        private string _emptyStateSubHeaderText;
        public string EmptyStateSubHeaderText
        {
            get { return _emptyStateSubHeaderText; }
            set { SetField(ref _emptyStateSubHeaderText, value); }
        }

        private IBaseNode _focusedNode;
        public IBaseNode FocusedNode
        {
            get { return _focusedNode; }
            set { SetField(ref _focusedNode, value); }
        }

        public ContainerType Type { get; private set; }

        private IBaseNode _folderRootNode;
        public IBaseNode FolderRootNode
        {
            get { return _folderRootNode; }
            set { SetField(ref _folderRootNode, value); }
        }

        public BreadCrumbViewModel BreadCrumb { get; }

        public CollectionViewModel<IBaseNode> ItemCollection { get; }

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        protected CancellationToken LoadingCancelToken { get; set; }

        private FolderContentViewMode _viewMode;
        public FolderContentViewMode ViewMode
        {
            get { return _viewMode; }
            set
            {
                SetField(ref _viewMode, value);
                OnPropertyChanged(nameof(this.IsListViewMode),
                    nameof(this.IsGridViewMode));
            }
        }

        public bool IsListViewMode => this.ViewMode == FolderContentViewMode.ListView;
        public bool IsGridViewMode => this.ViewMode == FolderContentViewMode.GridView;

        private string _nextViewButtonPathData;
        public string NextViewButtonPathData
        {
            get { return _nextViewButtonPathData; }
            set { SetField(ref _nextViewButtonPathData, value); }
        }

        private string _nextViewButtonLabelText;
        public string NextViewButtonLabelText
        {
            get { return _nextViewButtonLabelText; }
            set { SetField(ref _nextViewButtonLabelText, value); }
        }

        private DataTemplateSelector _nodeTemplateSelector;
        public DataTemplateSelector NodeTemplateSelector
        {
            get { return _nodeTemplateSelector; }
            set { SetField(ref _nodeTemplateSelector, value); }
        }

        private string _emptyInformationText;
        public string EmptyInformationText
        {
            get { return _emptyInformationText; }
            set { SetField(ref _emptyInformationText, value); }
        }

        public bool IsForSelectFolder { get; private set; }

        public bool IsFlyoutActionAvailable => !this.IsPanelOpen;

        public bool IsPanelOpen => this.VisiblePanel != PanelType.None;

        private PanelType _visiblePanel;
        public PanelType VisiblePanel
        {
            get { return _visiblePanel; }
            set
            {
                SetField(ref _visiblePanel, value);
                OnPropertyChanged(nameof(this.IsPanelOpen),
                    nameof(this.IsFlyoutActionAvailable));

                this.ItemCollection.IsOnlyAllowSingleSelectActive = (_visiblePanel != PanelType.None);
            }
        }

        public virtual string OrderTypeAndNumberOfItems => string.Empty;
        public virtual string OrderTypeAndNumberOfSelectedItems => string.Empty;

        #endregion

        #region UiResources

        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string ClosePanelText => ResourceService.UiResources.GetString("UI_ClosePanel");
        public string GridViewText => ResourceService.UiResources.GetString("UI_GridView");
        public string ListViewText => ResourceService.UiResources.GetString("UI_ListView");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");

        #endregion

        #region VisualResources

        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");

        #endregion
    }
}
