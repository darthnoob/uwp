using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels.UserControls
{
    public class CopyOrMovePanelViewModel : BaseViewModel
    {
        public CopyOrMovePanelViewModel()
        {
            CopyOrMoveService.SelectedNodesChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(this.ActiveFolderView));
                OnPropertyChanged(nameof(this.CopyOrMoveItemsToText));
            };

            this.AddFolderCommand = new RelayCommand(AddFolder);
            this.CancelCommand = new RelayCommand(CancelCopyOrMove);
            this.CopyCommand = new RelayCommand<bool>(CopyOrMove);
            this.MoveCommand = new RelayCommand<bool>(CopyOrMove);

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
            if (CopyOrMoveService.SelectedNodes == null || !CopyOrMoveService.SelectedNodes.Any()) return;

            bool finalResult = true;
            try
            {
                // Use a temporal list of nodes to copy/move to allow close the panel and
                // deselect the nodes in the main view meanwhile the nodes are copied/moved
                var selectedNodes = CopyOrMoveService.SelectedNodes.ToList();

                this.OnCopyOrMoveFinished();

                var newParentNode = this.ActiveFolderView.FolderRootNode;
                foreach (var node in selectedNodes)
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

        public FolderViewModel CloudDrive => CopyOrMoveService.CloudDrive;

        public IncomingSharesViewModel IncomingShares => CopyOrMoveService.IncomingShares;

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
                if (CopyOrMoveService.SelectedNodes == null) return string.Empty;
                return CopyOrMoveService.SelectedNodes.Count == 1 ?
                    ResourceService.UiResources.GetString("UI_CopyOrMoveItemTo") :
                    string.Format(ResourceService.UiResources.GetString("UI_CopyOrMoveMultipleItemsTo"),
                    CopyOrMoveService.SelectedNodes.Count);
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
