using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using mega;
using MegaApp.MegaApi;

namespace MegaApp.Services
{
    public static class SdkService
    {
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

        private static MegaSDK _megaSdkFolderLinks;
        private static MegaSDK MegaSdkFolderLinks
        {
            get
            {
                if (_megaSdkFolderLinks != null) return _megaSdkFolderLinks;
                _megaSdkFolderLinks = CreateSdk();
                return _megaSdkFolderLinks;
            }
        }

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
            MegaSDK.setStatsID(AppService.GetDeviceID());
        }

        private static MegaSDK CreateSdk()
        {
            // Get an instance of the object that allow recover the local device information.
            var deviceInfo = new EasClientDeviceInformation();

            // Initialize a MegaSDK instance
            return new MegaSDK(
                "Z5dGhQhL",
                string.Format("{0}/{1}/{2}",
                    AppService.GetAppUserAgent(),
                    deviceInfo.SystemManufacturer,
                    deviceInfo.SystemProductName),
                ApplicationData.Current.LocalFolder.Path,
                new MegaRandomNumberProvider());
        }

    }
}
