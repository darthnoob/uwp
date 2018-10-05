using System;
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
            if (rootNode != null && rootNode.isInShare())
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
        /// <returns>TRUE if all went weel or FALSE in other case.</returns>
        public static bool RemoveFolderFromOfflineDB(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) return false;

            bool result = true;
            if (FolderService.FolderExists(folderPath))
            {
                try
                {
                    IEnumerable<string> childFolders = Directory.GetDirectories(folderPath);
                    if (childFolders != null)
                    {
                        foreach (var folder in childFolders)
                        {
                            if (folder != null)
                            {
                                result &= RemoveFolderFromOfflineDB(folder);
                                result &= SavedForOfflineDB.DeleteNodeByLocalPath(folder);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, 
                        string.Format("Error removing from the offline DB the subfolders of '{0}'", folderPath), e);
                    result = false;
                }

                try
                {
                    IEnumerable<string> childFiles = Directory.GetFiles(folderPath);
                    if (childFiles != null)
                    {
                        foreach (var file in childFiles)
                        {
                            if (file != null)
                                result &= SavedForOfflineDB.DeleteNodeByLocalPath(file);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR,
                        string.Format("Error removing from the offline DB the files of '{0}'", folderPath), e);
                    result = false;
                }
            }

            result &= SavedForOfflineDB.DeleteNodeByLocalPath(folderPath);

            return result;
        }

        /// <summary>
        /// Check and add to the DB if necessary the previous folders of the path
        /// </summary>
        /// <param name="node">Node to check the path</param>
        public static void CheckOfflineNodePath(MNode node)
        {
            if (node == null) return;

            var offlineParentNodePath = GetOfflineParentNodePath(node);
            var parentNode = SdkService.MegaSdk.getParentNode(node);
            if (parentNode == null) return;

            while (string.CompareOrdinal(offlineParentNodePath, AppService.GetOfflineDirectoryPath()) != 0)
            {
                var folderPathToAdd = offlineParentNodePath;

                if (!SavedForOfflineDB.ExistsNodeByLocalPath(folderPathToAdd))
                    SavedForOfflineDB.InsertNode(parentNode);

                offlineParentNodePath = GetOfflineParentNodePath(parentNode);
                parentNode = SdkService.MegaSdk.getParentNode(parentNode);
                if (parentNode == null) return;
            }
        }

        /// <summary>
        /// Checks if the previous folders of an offline folder node path are empty 
        /// and removes them from the offline folder and the DB on this case.
        /// </summary>
        /// <param name="folderNodePath">Path of the folder node</param>
        /// <returns>TRUE if the process finished successfully or FALSE in other case.</returns>
        public static bool CleanOfflineFolderNodePath(string folderNodePath)
        {
            if (string.IsNullOrWhiteSpace(folderNodePath)) return false;

            var result = true;
            while (string.CompareOrdinal(folderNodePath, AppService.GetOfflineDirectoryPath()) != 0)
            {
                try
                {
                    if (!FolderService.FolderExists(folderNodePath)) return false;

                    var folderPathToRemove = folderNodePath;
                    if (!FolderService.IsEmptyFolder(folderPathToRemove)) return true;

                    var directoryInfo = new DirectoryInfo(folderNodePath).Parent;
                    if (directoryInfo == null) return true;
                    folderNodePath = directoryInfo.FullName;

                    result &= FolderService.DeleteFolder(folderPathToRemove);
                    result &= SavedForOfflineDB.DeleteNodeByLocalPath(folderPathToRemove);
                }
                catch (Exception e)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR,
                        string.Format("Error cleaning offline node path '{0}'", folderNodePath), e);
                    return false;
                }
            }

            return result;
        }
    }
}
