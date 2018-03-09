using System.Collections.Generic;
using System.IO;
using mega;
using MegaApp.Database;

namespace MegaApp.Services
{
    public static class OfflineService
    {
        /// <summary>
        /// Gets the offline path of a <see cref="MNode"/>
        /// </summary>
        /// <param name="node">Node to get the path</param>
        /// <returns>The offline path of the node</returns>
        public static string GetOfflineNodePath(MNode node)
        {
            if (node == null) return null;

            var rootNode = SdkService.MegaSdk.getRootNode(node);
            if (rootNode.isInShare())
            {
                return Path.Combine(AppService.GetOfflineDirectoryPath(),
                    SdkService.MegaSdk.getUserFromInShare(rootNode).getHandle().ToString(),
                    SdkService.MegaSdk.getNodePath(node).Split(':')[1].Replace("/", "\\"));
            }

            var nodePath = SdkService.MegaSdk.getNodePath(node);
            if (string.IsNullOrWhiteSpace(nodePath))
                return AppService.GetOfflineDirectoryPath();

            return Path.Combine(AppService.GetOfflineDirectoryPath(),
                nodePath.Remove(0, 1).Replace("/", "\\"));
        }

        /// <summary>
        /// Gets the offline path of the parent of a <see cref="MNode"/>
        /// </summary>
        /// <param name="node">Node to get the parent node path</param>
        /// <returns>The offline path of the parent node</returns>
        public static string GetOfflineParentNodePath(MNode node)
        {
            if (node == null) return null;

            var parentNode = SdkService.MegaSdk.getParentNode(node);
            if (parentNode == null)
            {
                if (node.isInShare())
                {
                    return Path.Combine(AppService.GetOfflineDirectoryPath(),
                        SdkService.MegaSdk.getUserFromInShare(node).getHandle().ToString());
                }

                return AppService.GetOfflineDirectoryPath();
            }

            var parentNodePath = SdkService.MegaSdk.getNodePath(parentNode);
            if (string.IsNullOrWhiteSpace(parentNodePath))
                return AppService.GetOfflineDirectoryPath();

            var rootNode = SdkService.MegaSdk.getRootNode(node);
            if (rootNode.isInShare())
            {
                return Path.Combine(AppService.GetOfflineDirectoryPath(),
                    SdkService.MegaSdk.getUserFromInShare(rootNode).getHandle().ToString(),
                    parentNodePath.Split(':')[1]).Replace("/", "\\");
            }

            return Path.Combine(AppService.GetOfflineDirectoryPath(),
                parentNodePath.Remove(0, 1).Replace("/", "\\"));
        }

        /// <summary>
        /// Removes a folder recursively from the offline DB
        /// </summary>
        /// <param name="folderPath">Path of the folder</param>
        public static void RemoveFolderFromOfflineDB(string folderPath)
        {
            if (FolderService.FolderExists(folderPath))
            {
                IEnumerable<string> childFolders = Directory.GetDirectories(folderPath);
                if (childFolders != null)
                {
                    foreach (var folder in childFolders)
                    {
                        if (folder != null)
                        {
                            var subFolderPath = Path.Combine(folderPath, folder);
                            RemoveFolderFromOfflineDB(subFolderPath);
                            SavedForOfflineDB.DeleteNodeByLocalPath(subFolderPath);
                        }
                    }
                }

                IEnumerable<string> childFiles = Directory.GetFiles(folderPath);
                if (childFiles != null)
                {
                    foreach (var file in childFiles)
                    {
                        if (file != null)
                            SavedForOfflineDB.DeleteNodeByLocalPath(Path.Combine(folderPath, file));
                    }
                }
            }

            SavedForOfflineDB.DeleteNodeByLocalPath(folderPath);
        }

        /// <summary>
        /// Checks if the previous folders of an offline folder node path are empty 
        /// and removes them from the offline folder and the DB on this case.
        /// </summary>
        /// <param name="folderNodePath">Path of the folder node</param>
        public static void CleanOfflineFolderNodePath(string folderNodePath)
        {
            while (string.Compare(folderNodePath, AppService.GetOfflineDirectoryPath()) != 0)
            {
                var folderPathToRemove = folderNodePath;
                if (!FolderService.IsEmptyFolder(folderPathToRemove)) return;

                folderNodePath = ((new DirectoryInfo(folderNodePath)).Parent).FullName;

                if (FolderService.DeleteFolder(folderPathToRemove))
                    SavedForOfflineDB.DeleteNodeByLocalPath(folderPathToRemove);
            }
        }
    }
}
