using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using BackgroundTaskService.MegaApi;
using mega;

namespace BackgroundTaskService.Services
{
    internal static class SdkService
    {
        /// <summary>
        /// Main MegaSDK instance of the app
        /// </summary>
        private static MegaSDK _megaSdk;
        public static MegaSDK MegaSdk
        {
            get
            {
                if (_megaSdk != null) return _megaSdk;
                _megaSdk = CreateSdk();
                return _megaSdk;
            }
        }
        
        /// <summary>
        /// Initialize all the SDK parameters
        /// </summary>
        public static void InitializeSdkParams()
        {
            //The next line enables a custom logger, if this function is not used OutputDebugString() is called
            //in the native library and log messages are only readable with the native debugger attached.
            //The default behavior of MegaLogger() is to print logs using Debug.WriteLine() but it could
            //be used to sends log to a file, for example.
            MegaSDK.setLoggerObject(new MegaLogger());

            //You can select the maximum output level for debug messages.
            //By default FATAL, ERROR, WARNING and INFO will be enabled
            //DEBUG and MAX can only be enabled in Debug builds, they are ignored in Release builds
            MegaSDK.setLogLevel(MLogLevel.LOG_LEVEL_DEBUG);

            //You can send messages to the logger using MEGASDK.log(), those messages will be received
            //in the active logger
            MegaSDK.log(MLogLevel.LOG_LEVEL_INFO, "Example log message");

            // Set the ID for statistics
            MegaSDK.setStatsID(GetDeviceId());
        }

        /// <summary>
        /// Create a MegaSDK instance
        /// </summary>
        /// <returns>The new MegaSDK instance</returns>
        private static MegaSDK CreateSdk()
        {
            // Create Camera Upload service directory if not already exists
            var folderCameraUploadService = Path.Combine(ApplicationData.Current.LocalFolder.Path, "CameraUploadService");
            if (!Directory.Exists(folderCameraUploadService)) Directory.CreateDirectory(folderCameraUploadService);

            // Get an instance of the object that allow recover the local device information.
            var deviceInfo = new EasClientDeviceInformation();

            // Initialize a MegaSDK instance
            return new MegaSDK(
                "Z5dGhQhL",
                String.Format("{0}/{1}/{2}",
                    String.Format("MEGA_UWP/UploadService/{0}", GetTaskVersion()),
                    deviceInfo.SystemManufacturer,
                    deviceInfo.SystemProductName),
                ApplicationData.Current.LocalFolder.Path,
                new MegaRandomNumberProvider());
        }

        public static bool IsAlreadyUploaded(StorageFile fileToUpload, Stream fileStream, MNode rootNode, ulong mTime)
        {
            // Make sure the stream pointer is at the start of the stream
            fileStream.Position = 0;
            // Get the unique fingerprint of the file
            string fingerprint = MegaSdk.getFileFingerprint(new MegaInputStream(fileStream), mTime);
            // Check if the fingerprint is already in the subfolders of the Camera Uploads
            var mNode = MegaSdk.getNodeByFingerprint(fingerprint, rootNode);

            return mNode != null;
        }

        /// <summary>
        /// Locate the Camera Uploads folder node to use as parent for the uploads
        /// </summary>
        /// <returns>Camera Uploads root folder node</returns>
        public static async Task<MNode> GetCameraUploadRootNodeAsync()
        {
            // First try to retrieve the Cloud Drive root node
            var rootNode = MegaSdk.getRootNode();
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

        public static async Task UploadAsync(StorageFile fileToUpload, Stream fileStream, MNode rootNode, ulong mTime)
        {
            MegaSdk.retryPendingConnections();

            // Make sure the stream pointer is at the start of the stream
            fileStream.Position = 0;

            // Create a temporary local path to save the picture for upload
            string tempFilePath = Path.Combine(TaskService.GetTemporaryUploadFolder(), fileToUpload.Name);

            // Copy file to local storage to be able to upload
            using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                // Set buffersize to avoid copy failure of large files
                await fileStream.CopyToAsync(fs, 8192);
                await fs.FlushAsync();
            }

            // Init the upload
            var transfer = new MegaTransferListener();
            var result = await transfer.ExecuteAsync(
                () => MegaSdk.startUploadWithMtimeTempSource(tempFilePath, rootNode, mTime, true, transfer),
                TaskService.ImageDateSetting);
            if(!string.IsNullOrEmpty(result)) throw new Exception(result);
        }

        /// <summary>
        /// Locate the Camera Uploads folder node in the specified root
        /// </summary>
        /// <param name="rootNode">Current root node</param>
        /// <returns>Camera Uploads folder node in</returns>
        private static MNode FindCameraUploadNode(MNode rootNode)
        {
            var childs = MegaSdk.getChildren(rootNode);

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

        private static string GetTaskVersion()
        {
            return "1.0.0";
        }

        private static string GetDeviceId()
        {
            HardwareToken token = HardwareIdentification.GetPackageSpecificToken(null);
            IBuffer hardwareId = token.Id;

            HashAlgorithmProvider hasher = HashAlgorithmProvider.OpenAlgorithm("MD5");
            IBuffer hashed = hasher.HashData(hardwareId);

            string hashedString = CryptographicBuffer.EncodeToHexString(hashed);
            return hashedString;
        }
    }
}
