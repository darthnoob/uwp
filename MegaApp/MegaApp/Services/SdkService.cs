using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml;
using mega;
using MegaApp.MegaApi;

namespace MegaApp.Services
{
    public static class SdkService
    {
        #region Events

        /// <summary>
        /// Event triggered when the API URL is changed.
        /// </summary>
        public static event EventHandler ApiUrlChanged;

        /// <summary>
        /// Event invocator method called when the API URL is changed.
        /// </summary>
        private static void OnApiUrlChanged() => ApiUrlChanged?.Invoke(null, EventArgs.Empty);

        #endregion

        #region Properties

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
        /// MegaSDK instance for the folder links management
        /// </summary>
        private static MegaSDK _megaSdkFolderLinks;
        public static MegaSDK MegaSdkFolderLinks
        {
            get
            {
                if (_megaSdkFolderLinks != null) return _megaSdkFolderLinks;
                _megaSdkFolderLinks = CreateSdk();
                return _megaSdkFolderLinks;
            }
        }

        // Timer to count the actions needed to change the API URL.
        private static DispatcherTimer timerChangeApiUrl;

        #endregion

        #region Methods

        /// <summary>
        /// Initialize all the SDK parameters
        /// </summary>
        public static void InitializeSdkParams()
        {
            //The next line enables a custom logger, if this function is not used OutputDebugString() is called
            //in the native library and log messages are only readable with the native debugger attached.
            //The default behavior of MegaLogger() is to print logs using Debug.WriteLine() but it could
            //be used to sends log to a file, for example.
            LogService.AddLoggerObject(LogService.MegaLogger);

            //You can select the maximum output level for debug messages.
            //By default FATAL, ERROR, WARNING and INFO will be enabled
            //DEBUG and MAX can only be enabled in Debug builds, they are ignored in Release builds
            MegaSDK.setLogLevel(MLogLevel.LOG_LEVEL_DEBUG);

            //You can send messages to the logger using MEGASDK.log(), those messages will be received
            //in the active logger
            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Example log message");

            // Use custom DNS servers
            SetDnsServers();

            // Set the ID for statistics
            MegaSDK.setStatsID(DeviceService.GetDeviceId());

            // Set the language code used by the app
            var appLanguageCode = AppService.GetAppLanguageCode();
            if (!MegaSdk.setLanguage(appLanguageCode) || !MegaSdkFolderLinks.setLanguage(appLanguageCode))
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING,
                    string.Format("Invalid app language code '{0}'", appLanguageCode));
            }

            // Change the API URL if required by settings
            if (SettingsService.Load(ResourceService.SettingsResources.GetString("SR_UseStagingServer"), false))
            {
                MegaSdk.changeApiUrl(ResourceService.AppResources.GetString("AR_StagingUrl"));
                MegaSdkFolderLinks.changeApiUrl(ResourceService.AppResources.GetString("AR_StagingUrl"));
            }
            else if (SettingsService.Load(ResourceService.SettingsResources.GetString("SR_UseStagingServerPort444"), false))
            {
                MegaSdk.changeApiUrl(ResourceService.AppResources.GetString("AR_StagingUrlPort444"), true);
                MegaSdkFolderLinks.changeApiUrl(ResourceService.AppResources.GetString("AR_StagingUrlPort444"), true);
            }
        }

        /// <summary>
        /// Create a MegaSDK instance
        /// </summary>
        /// <returns>The new MegaSDK instance</returns>
        private static MegaSDK CreateSdk()
        {
            // Initialize a MegaSDK instance
            var newMegaSDK = new MegaSDK(
                "Z5dGhQhL",
                AppService.GetAppUserAgent(),
                ApplicationData.Current.LocalFolder.Path,
                new MegaRandomNumberProvider());

            // Enable retrying when public key pinning fails
            newMegaSDK.retrySSLerrors(true);

            return newMegaSDK;
        }

        /// <summary>
        /// Use custom DNS servers in the selected SDK instance.
        /// </summary>
        /// <param name="megaSdk">SDK instance to set the custom DNS servers.</param>
        /// <param name="refresh">Indicates if should refresh the previously stored addresses.</param>
        private static async void SetDnsServers(MegaSDK megaSdk, bool refresh = true)
        {
            var dnsServers = NetworkService.GetSystemDnsServers(refresh);
            if (string.IsNullOrWhiteSpace(dnsServers))
                dnsServers = await NetworkService.GetMegaDnsServersAsync(refresh);
            if (!string.IsNullOrWhiteSpace(dnsServers))
                megaSdk.setDnsServers(dnsServers);
        }

        /// <summary>
        /// Use custom DNS servers in all the SDK instances.
        /// </summary>
        /// <param name="refresh">Indicates if should refresh the previously stored addresses.</param>
        public static async void SetDnsServers(bool refresh = true)
        {
            var dnsServers = NetworkService.GetSystemDnsServers(refresh);
            if (string.IsNullOrWhiteSpace(dnsServers))
                dnsServers = await NetworkService.GetMegaDnsServersAsync(refresh);

            if (!string.IsNullOrWhiteSpace(dnsServers))
            {
                MegaSdk.setDnsServers(dnsServers);
                MegaSdkFolderLinks.setDnsServers(dnsServers);
            }
        }

        /// <summary>
        /// Locate or create the Camera Uploads folder node to use as parent for the uploads
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

            // If node not found and the service is enabled, create a new Camera Uploads node
            if (TaskService.IsBackGroundTaskActive(CameraUploadService.TaskEntryPoint, CameraUploadService.TaskName))
            {
                var createFolder = new CreateFolderRequestListenerAsync();
                var result = await createFolder.ExecuteAsync(() =>
                {
                    MegaSdk.createFolder("Camera Uploads", rootNode, createFolder);
                });
                return result ? FindCameraUploadNode(rootNode) : null;
            }

            return null;
        }

        /// <summary>
        /// Checks if a node exists by its name.
        /// </summary>
        /// <param name="searchNode">The parent node of the tree to explore.</param>
        /// <param name="name">Name of the node to search.</param>
        /// <param name="isFolder">True if the node to search is a folder or false in other case.</param>
        /// <param name="recursive">True if you want to seach recursively in the node tree.</param>
        /// <returns>True if the node exists or false in other case.</returns>
        public static bool ExistsNodeByName(MNode searchNode, string name, bool isFolder, bool recursive = false)
        {
            var searchResults = MegaSdk.search(searchNode, name, recursive);
            for (var i = 0; i < searchResults.size(); i++)
            {
                var node = searchResults.get(i);
                if (node.isFolder() == isFolder && node.getName().ToLower().Equals(name.ToLower()))
                    return true;
            }

            return false;
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

        /// <summary>
        /// Method that should be called when an action required for 
        /// change the API URL is started.
        /// </summary>
        public static void ChangeApiUrlActionStarted()
        {
            UiService.OnUiThread(() =>
            {
                if (timerChangeApiUrl == null)
                {
                    timerChangeApiUrl = new DispatcherTimer();
                    timerChangeApiUrl.Interval = new TimeSpan(0, 0, 5);
                    timerChangeApiUrl.Tick += (obj, args) => ChangeApiUrl();
                }
                timerChangeApiUrl.Start();
            });
        }

        /// <summary>
        /// Method that should be called when an action required for 
        /// change the API URL is finished.
        /// </summary>
        public static void ChangeApiUrlActionFinished() => StopChangeApiUrlTimer();

        /// <summary>
        /// Change the API URL.
        /// </summary>
        private static async void ChangeApiUrl()
        {
            StopChangeApiUrlTimer();

            var useStagingServer = SettingsService.Load(ResourceService.SettingsResources.GetString("SR_UseStagingServer"), false) ||
                SettingsService.Load(ResourceService.SettingsResources.GetString("SR_UseStagingServerPort444"), false);

            if (!useStagingServer)
            {
                var result = await DialogService.ShowChangeToStagingServerDialog();
                if (!result) return;
            }
            else
            {
                SettingsService.Save(ResourceService.SettingsResources.GetString("SR_UseStagingServer"), false);
                SettingsService.Save(ResourceService.SettingsResources.GetString("SR_UseStagingServerPort444"), false);
                MegaSdk.changeApiUrl(ResourceService.AppResources.GetString("AR_ApiUrl"));
                MegaSdkFolderLinks.changeApiUrl(ResourceService.AppResources.GetString("AR_ApiUrl"));
            }

            // Reset the "Camera Uploads" service if is enabled
            if (TaskService.IsBackGroundTaskActive(CameraUploadService.TaskEntryPoint, CameraUploadService.TaskName))
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Resetting CAMERA UPLOADS service (API URL changed)");
                await TaskService.RegisterBackgroundTaskAsync(
                    CameraUploadService.TaskEntryPoint, CameraUploadService.TaskName,
                    new TimeTrigger(CameraUploadService.TaskTimeTrigger, false));
            }

            OnApiUrlChanged();
        }

        /// <summary>
        /// Stops the timer to detect an API URL change.
        /// </summary>
        private static void StopChangeApiUrlTimer() =>
            UiService.OnUiThread(() => timerChangeApiUrl?.Stop());

        #endregion
    }
}
