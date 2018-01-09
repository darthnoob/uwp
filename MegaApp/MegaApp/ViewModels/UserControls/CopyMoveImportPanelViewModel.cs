using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;
using MegaApp.Views;

namespace MegaApp.ViewModels.UserControls
{
    public class CopyMoveImportPanelViewModel : BaseUiViewModel
    {
        public CopyMoveImportPanelViewModel()
        {
            SelectedNodesService.SelectedNodesChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(this.ActiveFolderView));
                OnPropertyChanged(nameof(this.CopyOrMoveItemsToText));
            };

            this.AddFolderCommand = new RelayCommand(AddFolder);
            this.CancelCommand = new RelayCommand(CancelCopyOrMove);
            this.CopyCommand = new RelayCommand<bool>(CopyOrMove);
            this.ImportCommand = new RelayCommand(Import);
            this.MoveCommand = new RelayCommand<bool>(CopyOrMove);

            this.ActiveFolderView = this.CloudDrive;
        }

        #region Commands

        public ICommand AddFolderCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand MoveCommand { get; }

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
            if (SelectedNodesService.SelectedNodes == null || !SelectedNodesService.SelectedNodes.Any()) return;

            bool finalResult = true;
            try
            {
                // Use a temporal list of nodes to copy/move to allow close the panel and
                // deselect the nodes in the main view meanwhile the nodes are copied/moved
                var selectedNodes = SelectedNodesService.SelectedNodes.ToList();

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

        private async void Import()
        {
            if (SelectedNodesService.SelectedNodes == null || !SelectedNodesService.SelectedNodes.Any()) return;

            bool finalResult = true;
            try
            {
                // Use a temporal list of nodes to copy/move to allow close the panel and
                // deselect the nodes in the main view meanwhile the nodes are imported
                var selectedNodes = SelectedNodesService.SelectedNodes.ToList();

                this.OnCopyOrMoveFinished();

                var newParentNode = this.ActiveFolderView.FolderRootNode;
                foreach (var node in selectedNodes)
                {
                    if (node == null) continue;
                    node.DisplayMode = NodeDisplayMode.Normal;
                    var result = await node.ImportAsync(newParentNode);
                    finalResult = finalResult & (result == NodeActionResult.Succeeded);
                }
            }
            catch (Exception) { finalResult = false; }
            finally
            {
                if (!finalResult)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Import failed.");
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_ImportFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_ImportFailed"));
                }

                this.OnCopyOrMoveFinished();
            }
        }

        private void OnActiveFolderPropertyChanged(object sender, PropertyChangedEventArgs e) => 
            OnPropertyChanged(nameof(this.ActiveFolderView));

        #endregion

        #region Properties

        public FolderViewModel CloudDrive => SelectedNodesService.CloudDrive;

        public IncomingSharesViewModel IncomingShares => SelectedNodesService.IncomingShares;

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
                if (SelectedNodesService.SelectedNodes == null) return string.Empty;

                var singleItemString = SelectedNodesService.IsSourceFolderLink ?
                    ResourceService.UiResources.GetString("UI_ImportItemTo") :
                    ResourceService.UiResources.GetString("UI_CopyOrMoveItemTo");

                var multipleItemString = SelectedNodesService.IsSourceFolderLink ?
                    ResourceService.UiResources.GetString("UI_ImportMultipleItemsTo") :
                    ResourceService.UiResources.GetString("UI_CopyOrMoveMultipleItemsTo");

                return SelectedNodesService.SelectedNodes.Count == 1 ? singleItemString : 
                    string.Format(multipleItemString, SelectedNodesService.SelectedNodes.Count);
            }
        }

        public string AddFolderText => ResourceService.UiResources.GetString("UI_NewFolder");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string CopyText => ResourceService.UiResources.GetString("UI_Copy");
        public string ImportText => ResourceService.UiResources.GetString("UI_Import");
        public string MoveText => ResourceService.UiResources.GetString("UI_Move");

        #endregion

        #region VisualResources

        public string AddFolderPathData => ResourceService.VisualResources.GetString("VR_CreateFolderPathData");
        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string ConfirmPathData => ResourceService.VisualResources.GetString("VR_ConfirmPathData");
        public string CopyPathData => ResourceService.VisualResources.GetString("VR_CopyPathData");

        #endregion
    }
}
