using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.Storage.Search;
using CameraUploadService.MegaApi;
using CameraUploadService.Services;
using mega;

namespace CameraUploadService
{
    public sealed class CameraUploadTask: IBackgroundTask
    {
        // If you run any asynchronous code in your background task, then your background task needs to use a deferral. 
        // If you don't use a deferral, then the background task process can terminate unexpectedly
        // if the Run method completes before your asynchronous method call has completed.
        // Note: defined at class scope so we can mark it complete inside the OnCancel() callback if we choose to support cancellation
        BackgroundTaskDeferral _deferral; 

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            SdkService.InitializeSdkParams();

            var loggedIn = await Login();

            if (loggedIn)
            {
                var fetched = await FetchNodes();
                if (fetched)
                {
                    var megaGlobalListener = new MegaGlobalListener();
                    SdkService.MegaSdk.addGlobalListener(megaGlobalListener);
                    await megaGlobalListener.ExecuteAsync(() => SdkService.MegaSdk.enableTransferResumption());
                    var fileToUpload = await TaskService.GetAvailableUpload(KnownFolders.PicturesLibrary);
                    while(fileToUpload != null)
                    {
                        await UploadAsync(fileToUpload);
                        fileToUpload = await TaskService.GetAvailableUpload(KnownFolders.PicturesLibrary);
                    }
                }
            }
            
            _deferral.Complete();
        }

        private async Task UploadAsync(StorageFile fileToUpload)
        {
            SdkService.MegaSdk.retryPendingConnections();

            if (fileToUpload == null) return;

            var cameraUploadRootNode = await GetCameraUploadRootNodeAsync();
            if (cameraUploadRootNode == null) return;

            using (var stream = await fileToUpload.OpenStreamForReadAsync())
            {
                // Make sure the stream pointer is at the start of the stream
                stream.Position = 0;
              
                //var props = await fileToUpload.GetBasicPropertiesAsync();
                // Calculate time for fingerprint check
                ulong mtime = TaskService.CalculateMtime(fileToUpload.DateCreated.DateTime);
                // Get the unique fingerprint of the file
                string fingerprint = SdkService.MegaSdk.getFileFingerprint(new MegaInputStream(stream), mtime);
                // Check if the fingerprint is already in the subfolders of the Camera Uploads
                var mNode = SdkService.MegaSdk.getNodeByFingerprint(fingerprint, cameraUploadRootNode);

                // If node already exists then save the node date and proceed with the next node
                if (mNode != null)
                {
                    await SettingsService.SaveSettingToFileAsync("LastUploadDate",
                        fileToUpload.DateCreated.DateTime);
                    return; // skip to next file
                }

                // Create a temporary local path to save the picture for upload
                string tempFilePath = Path.Combine(TaskService.GetTemporaryUploadFolder(), fileToUpload.Name);

                // Reset stream back to start because fingerprint action has moved the position
                stream.Position = 0;

                // Copy file to local storage to be able to upload
                using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    // Set buffersize to avoid copy failure of large files
                    await stream.CopyToAsync(fs, 8192);
                    await fs.FlushAsync();
                }

                // Init the upload
                var transfer = new MegaTransferListener();
                await transfer.ExecuteAsync(
                    () => SdkService.MegaSdk.startUploadWithMtimeTempSource(tempFilePath, cameraUploadRootNode, mtime, true, transfer));
            }
        }

        /// <summary>
        /// Locate the Camera Uploads folder node to use as parent for the uploads
        /// </summary>
        /// <returns>Camera Uploads root folder node</returns>
        private async Task<MNode> GetCameraUploadRootNodeAsync()
        {
            // First try to retrieve the Cloud Drive root node
            var rootNode = SdkService.MegaSdk.getRootNode();
            if (rootNode == null) return null;

            // Locate the camera upload node
            var cameraUploadNode = FindCameraUploadNode(rootNode);

            // If node found, return the node
            if (cameraUploadNode != null) return cameraUploadNode;

            // If node not found, create a new Camera Uploads node
            var folder = new MegaRequestListener<bool>();
            var result = await folder.ExecuteAsync(() => SdkService.MegaSdk.createFolder("Camera Uploads", rootNode, folder));
            return result ? FindCameraUploadNode(rootNode) : null;
        }


        /// <summary>
        /// Locate the Camera Uploads folder node in the specified root
        /// </summary>
        /// <param name="rootNode">Current root node</param>
        /// <returns>Camera Uploads folder node in</returns>
        private static MNode FindCameraUploadNode(MNode rootNode)
        {
            var childs = SdkService.MegaSdk.getChildren(rootNode);

            for (var x = 0; x < childs.size(); x++)
            {
                var node = childs.get(x);
                // Camera Uploads is a folder
                if (node.getType() != MNodeType.TYPE_FOLDER) continue;
                // Check the folder name
                if (!node.getName().ToLower().Equals("camera uploads")) continue;
                return node;
            }

            return null;
        }

        private async Task<bool> FetchNodes()
        {
            var fetch = new MegaRequestListener<bool>();
            return await fetch.ExecuteAsync(() => SdkService.MegaSdk.fetchNodes(fetch));
        }

        private async Task<bool> Login()
        {
            try
            {
                // Try to load shared session token file
                var sessionToken = await SettingsService.LoadSettingFromFileAsync<string>(
                    ResourceService.SettingsResources.GetString("SR_UserMegaSession"));

                if (string.IsNullOrEmpty(sessionToken) || string.IsNullOrWhiteSpace(sessionToken))
                    throw new Exception("Session token is empty.");

                var login = new MegaRequestListener<bool>();
                return await login.ExecuteAsync(() => SdkService.MegaSdk.fastLogin(sessionToken, login));
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
