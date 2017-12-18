using System;
using System.Collections.Generic;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.ViewModels;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.Services
{
    public static class CopyOrMoveService
    {
        #region Events

        /// <summary>
        /// Event triggered when the selected nodes have changed.
        /// </summary>
        public static event EventHandler SelectedNodesChanged;

        #endregion

        private static List<IMegaNode> _selectedNodes;
        public static List<IMegaNode> SelectedNodes
        {
            get
            {
                if (_selectedNodes != null) return _selectedNodes;
                _selectedNodes = new List<IMegaNode>();
                return _selectedNodes;
            }

            set
            {
                _selectedNodes = value;
                SelectedNodesChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private static FolderViewModel _cloudDrive;
        public static FolderViewModel CloudDrive
        {
            get
            {
                if (_cloudDrive != null) return _cloudDrive;

                _cloudDrive = new FolderViewModel(ContainerType.CloudDrive, true);
                _cloudDrive.FolderRootNode =
                    NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation,
                    SdkService.MegaSdk.getRootNode(), _cloudDrive);
                _cloudDrive.LoadChildNodes();

                return _cloudDrive;
            }
        }

        private static IncomingSharesViewModel _incomingShares;
        public static IncomingSharesViewModel IncomingShares
        {
            get
            {
                if (_incomingShares != null) return _incomingShares;

                _incomingShares = new IncomingSharesViewModel(true);
                _incomingShares.Initialize();

                return _incomingShares;
            }
        }

        public static void ClearSelectedNodes()
        {
            foreach (var node in SelectedNodes)
                if (node != null) node.DisplayMode = NodeDisplayMode.Normal;

            SelectedNodes.Clear();
        }

        /// <summary>
        /// Check if a node is in the selected nodes group for move, copy or any other action.
        /// </summary>        
        /// <param name="node">Node to check if is in the selected node list</param>        
        /// <returns>True if is a selected node or false in other case</returns>
        public static bool IsCopyOrMoveSelectedNode(IMegaNode node, bool setDisplayMode = false)
        {
            if (SelectedNodes?.Count > 0)
            {
                var count = SelectedNodes.Count;
                for (int index = 0; index < count; index++)
                {
                    var selectedNode = SelectedNodes[index];
                    if (node.OriginalMNode.getBase64Handle() == selectedNode?.OriginalMNode.getBase64Handle())
                    {
                        if(setDisplayMode)
                        {
                            //Update the selected nodes list values
                            node.DisplayMode = NodeDisplayMode.SelectedForCopyOrMove;
                            SelectedNodes[index] = node;
                        }

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
