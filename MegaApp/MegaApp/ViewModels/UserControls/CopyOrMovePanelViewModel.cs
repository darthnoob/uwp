using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels.UserControls
{
    public class CopyOrMovePanelViewModel : BaseViewModel
    {
        public CopyOrMovePanelViewModel()
        {
            this.AddFolderCommand = new RelayCommand(AddFolder);
            this.CancelCommand = new RelayCommand(CancelCopyOrMove);
            this.CopyCommand = new RelayCommand<bool>(CopyOrMove);
            this.MoveCommand = new RelayCommand<bool>(CopyOrMove);

            this.CloudDrive = new FolderViewModel(ContainerType.CloudDrive, true);
            this.CloudDrive.FolderRootNode =
                NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation,
                SdkService.MegaSdk.getRootNode(), this.CloudDrive);

            this.IncomingShares = new IncomingSharesViewModel(true);
            this.IncomingShares.Initialize();

            this.ActiveFolderView = this.CloudDrive;
        }

        #region Commands

        public ICommand AddFolderCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }
        public ICommand MoveCommand { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the copy/move process is finished
        /// </summary>
        public event EventHandler CopyOrMoveFinished;

        /// <summary>
        /// Event invocator method called when the copy/move process is finished
        /// </summary>
        protected void OnCopyOrMoveFinished()
        {
            this.CopyOrMoveFinished?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the copy/move process is cancelled
        /// </summary>
        public event EventHandler CopyOrMoveCanceled;

        /// <summary>
        /// Event invocator method called when the copy/move process is cancelled
        /// </summary>
        protected void OnCopyOrMoveCanceled()
        {
            this.CopyOrMoveCanceled?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        private void AddFolder()
        {
            if (this.ActiveFolderView.AddFolderCommand.CanExecute(null))
                this.ActiveFolderView.AddFolderCommand.Execute(null);
        }

        private void CancelCopyOrMove()
        {
            this.OnCopyOrMoveCanceled();
        }

        private async void CopyOrMove(bool move)
        {
            if (this.SelectedNodes == null || !this.SelectedNodes.Any()) return;

            bool finalResult = true;
            try
            {
                var newParentNode = this.ActiveFolderView.FolderRootNode;
                foreach (var node in this.SelectedNodes)
                {
                    if (node == null) continue;
                    node.DisplayMode = NodeDisplayMode.Normal;
                    var result = move ? await node.MoveAsync(newParentNode) : await node.CopyAsync(newParentNode);
                    finalResult = finalResult & (result == NodeActionResult.Succeeded);
                }
            }
            catch (Exception) { finalResult = false; }
            finally
            {
                this.OnCopyOrMoveFinished();

                if (!finalResult)
                {
                    string title = move ? ResourceService.AppMessages.GetString("AM_MoveFailed_Title") :
                        ResourceService.AppMessages.GetString("AM_CopyFailed_Title");
                    string message = move ? ResourceService.AppMessages.GetString("AM_MoveFailed") :
                        ResourceService.AppMessages.GetString("AM_CopyFailed");

                    await DialogService.ShowAlertAsync(title, message);
                }
            }
        }

        private void OnActiveFolderPropertyChanged(object sender, PropertyChangedEventArgs e) => 
            OnPropertyChanged(nameof(this.ActiveFolderView));

        #endregion

        #region Properties

        private List<IMegaNode> _selectedNodes;
        public List<IMegaNode> SelectedNodes
        {
            get { return _selectedNodes; }
            set
            {
                SetField(ref _selectedNodes, value);
                OnPropertyChanged(nameof(this.CopyOrMoveItemsToText));
            }
        }

        private FolderViewModel _cloudDrive;
        public FolderViewModel CloudDrive
        {
            get { return _cloudDrive; }
            private set { SetField(ref _cloudDrive, value); }
        }

        private IncomingSharesViewModel _incomingShares;
        public IncomingSharesViewModel IncomingShares
        {
            get { return _incomingShares; }
            private set { SetField(ref _incomingShares, value); }
        }

        private FolderViewModel _activeFolderView;
        public FolderViewModel ActiveFolderView
        {
            get { return _activeFolderView; }
            set
            {
                if (_activeFolderView != null)
                    _activeFolderView.PropertyChanged -= OnActiveFolderPropertyChanged;

                SetField(ref _activeFolderView, value);

                if (_activeFolderView != null)
                    _activeFolderView.PropertyChanged += OnActiveFolderPropertyChanged;
            }
        }

        #endregion

        #region UiResources

        public string CloudDriveNameText => ResourceService.UiResources.GetString("UI_CloudDriveName");
        public string IncomingSharesText => ResourceService.UiResources.GetString("UI_IncomingShares");

        public string CopyOrMoveItemsToText
        {
            get
            {
                if (SelectedNodes == null) return string.Empty;
                return SelectedNodes.Count == 1 ?
                    ResourceService.UiResources.GetString("UI_CopyOrMoveItemTo") :
                    string.Format(ResourceService.UiResources.GetString("UI_CopyOrMoveMultipleItemsTo"), SelectedNodes.Count);
            }
        }

        public string AddFolderText => ResourceService.UiResources.GetString("UI_NewFolder");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string CopyText => ResourceService.UiResources.GetString("UI_Copy");
        public string MoveText => ResourceService.UiResources.GetString("UI_Move");

        #endregion

        #region VisualResources

        public string AddFolderPathData => ResourceService.VisualResources.GetString("VR_CreateFolderPathData");
        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string CopyPathData => ResourceService.VisualResources.GetString("VR_CopyPathData");

        #endregion
    }
}
