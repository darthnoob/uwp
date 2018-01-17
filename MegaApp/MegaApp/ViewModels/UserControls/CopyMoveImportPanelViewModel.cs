using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;
using MegaApp.ViewModels.UserControls.CopyMoveImportPanel;

namespace MegaApp.ViewModels.UserControls.CopyMoveImportPanel
{
    public enum ActionType
    {
        Copy = 0,
        Move = 1,
        Import = 2
    }
}

namespace MegaApp.ViewModels.UserControls
{
    public class CopyMoveImportPanelViewModel : BaseUiViewModel
    {
        public CopyMoveImportPanelViewModel()
        {
            SelectedNodesService.SelectedNodesChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(this.ActiveFolderView));
                OnPropertyChanged(nameof(this.CopyMoveImportItemsToText));
            };

            this.AddFolderCommand = new RelayCommand(AddFolder);
            this.CancelCommand = new RelayCommand(Cancel);
            this.CopyCommand = new RelayCommand<ActionType>(ExecuteAction);
            this.ImportCommand = new RelayCommand<ActionType>(ExecuteAction);
            this.MoveCommand = new RelayCommand<ActionType>(ExecuteAction);

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
        /// Event triggered when the action is finished
        /// </summary>
        public event EventHandler ActionFinished;

        /// <summary>
        /// Event invocator method called when the action is finished
        /// </summary>
        protected void OnActionFinished()
        {
            this.ActionFinished?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the action is cancelled
        /// </summary>
        public event EventHandler ActionCanceled;

        /// <summary>
        /// Event invocator method called when the action is cancelled
        /// </summary>
        protected void OnActionCanceled()
        {
            this.ActionCanceled?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        private void AddFolder()
        {
            if (this.ActiveFolderView.AddFolderCommand.CanExecute(null))
                this.ActiveFolderView.AddFolderCommand.Execute(null);
        }

        private void Cancel()
        {
            this.OnActionCanceled();
        }

        private async void ExecuteAction(ActionType actionType)
        {
            if (SelectedNodesService.SelectedNodes == null || !SelectedNodesService.SelectedNodes.Any()) return;

            bool finalResult = true;
            try
            {
                // Use a temporal list of nodes to copy/move/import to allow close the panel and
                // deselect the nodes in the main view meanwhile the nodes are copied/moved/imported
                var selectedNodes = SelectedNodesService.SelectedNodes.ToList();

                // Store the new parent node to allow close and reset the panel meanwhile the action is being done
                var newParentNode = this.ActiveFolderView.FolderRootNode;

                this.OnActionFinished();

                foreach (var node in selectedNodes)
                {
                    if (node == null) continue;
                    node.DisplayMode = NodeDisplayMode.Normal;
                    NodeActionResult result;
                    
                    switch(actionType)
                    {
                        case ActionType.Copy:
                            result = await node.CopyAsync(newParentNode);
                            break;

                        case ActionType.Move:
                            result = await node.MoveAsync(newParentNode);
                            break;

                        case ActionType.Import:
                            result = await node.ImportAsync(newParentNode);
                            break;

                        default:
                            result = NodeActionResult.Failed;
                            break;
                    }

                    finalResult = finalResult & (result == NodeActionResult.Succeeded);
                }
            }
            catch (Exception) { finalResult = false; }
            finally
            {
                this.OnActionFinished();

                if (!finalResult)
                {
                    string title = string.Empty, message = string.Empty;
                    switch (actionType)
                    {
                        case ActionType.Copy:
                            LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Copy failed.");
                            title = ResourceService.AppMessages.GetString("AM_CopyFailed_Title");
                            message = ResourceService.AppMessages.GetString("AM_CopyFailed");
                            break;

                        case ActionType.Move:
                            LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Move failed.");
                            title = ResourceService.AppMessages.GetString("AM_MoveFailed_Title");
                            message = ResourceService.AppMessages.GetString("AM_MoveFailed");
                            break;

                        case ActionType.Import:
                            LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Import failed.");
                            title = ResourceService.AppMessages.GetString("AM_ImportFailed_Title");
                            message = ResourceService.AppMessages.GetString("AM_ImportFailed");
                            break;

                        default:
                            LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Action failed.");
                            break;
                    }

                    if(!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(message))
                        await DialogService.ShowAlertAsync(title, message);
                }
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

        public string CopyMoveImportItemsToText
        {
            get
            {
                if (SelectedNodesService.SelectedNodes == null) return string.Empty;

                var singleItemString = SelectedNodesService.IsSourcePublicLink ?
                    ResourceService.UiResources.GetString("UI_ImportItemTo") :
                    ResourceService.UiResources.GetString("UI_CopyOrMoveItemTo");

                var multipleItemString = SelectedNodesService.IsSourcePublicLink ?
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
        public string CopyPathData => ResourceService.VisualResources.GetString("VR_CopyPathData");
        public string ImportPathData => ResourceService.VisualResources.GetString("VR_ImportPathData");

        #endregion
    }
}
