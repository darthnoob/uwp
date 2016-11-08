using System.IO;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using CameraUploadService.MegaApi;
using mega;

namespace CameraUploadService.Services
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
                string.Format("{0}/{1}/{2}",
                    string.Format("MEGA_UWP/UploadService/{0}", GetTaskVersion()),
                    deviceInfo.SystemManufacturer,
                    deviceInfo.SystemProductName),
                ApplicationData.Current.LocalFolder.Path,
                new MegaRandomNumberProvider());
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
