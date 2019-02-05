using System;
using System.Threading.Tasks;
using Windows.Storage;
using mega;

namespace MegaApp.Services
{
    static class UploadService
    {
        public enum FileToUploadAction
        {
            UPLOAD,
            COPY,
            COPY_AND_RENAME,
            SAME_FILE_IN_FOLDER
        }

        public static async Task<FileToUploadAction> CheckFileToUpload(StorageFile file, MNode uploadFolder)
        {
            // Check by fingerprint if the node is already uploaded to MEGA
            var fingerprint = SdkService.MegaSdk.getFileFingerprint(file.Path);
            var nodes = SdkService.MegaSdk.getNodesByFingerprint(fingerprint);

            // If the node doesn't exists by fingerprint, check if exists in the selected upload folder
            if (nodes == null || nodes.size() == 0)
            {
                var nodeExistsInFolder = SdkService.MegaSdk.getNodeByPath(file.Name, uploadFolder);
                if (nodeExistsInFolder == null)
                    return FileToUploadAction.UPLOAD;

                if ((await file.GetBasicPropertiesAsync()).Size == nodeExistsInFolder.getSize())
                    return FileToUploadAction.SAME_FILE_IN_FOLDER;

                return FileToUploadAction.UPLOAD;
            }

            // Check if the node exists preferable in the same folder and with the same name
            var node = nodes.get(0); // Default node is the first node found
            for (int i = 0; i < nodes.size(); i++)
            {
                var remotePath = SdkService.MegaSdk.getNodePath(SdkService.MegaSdk.getParentNode(nodes.get(i)));
                var uploadPath = SdkService.MegaSdk.getNodePath(uploadFolder);

                // If node exists in the same folder
                if (remotePath == uploadPath)
                {
                    node = nodes.get(i);

                    // If also has the same name is the final selected node
                    if (nodes.get(i).getName() == file.Name)
                        break;
                }
            }

            // Extra check to avoid null reference exceptions in case of node doesn't exists
            if (node == null)
                return FileToUploadAction.UPLOAD;

            // If node exists but with a different name, copy and rename the file
            if (file.Name != node.getName())
            {
                SdkService.MegaSdk.copyAndRenameNode(node, uploadFolder, file.Name);
                return FileToUploadAction.COPY_AND_RENAME;
            }                
            // If node exists with the same name but in a different folder, copy the file
            else if (SdkService.MegaSdk.getNodePath(uploadFolder) != SdkService.MegaSdk.getNodePath(SdkService.MegaSdk.getParentNode(node)))
            {
                SdkService.MegaSdk.copyNode(node, uploadFolder);
                return FileToUploadAction.COPY;
            }
            
            // If node exists with the same name and in the same folder, do nothing
            return FileToUploadAction.SAME_FILE_IN_FOLDER;
        }
    }
}
