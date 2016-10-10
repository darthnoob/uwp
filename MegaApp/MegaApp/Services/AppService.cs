using Windows.ApplicationModel;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.System.Profile;

namespace MegaApp.Services
{
    class AppService
    {
        public static string GetAppVersion()
        {
            return string.Format("{0}.{1}.{2}.{3}",
                Package.Current.Id.Version.Major,
                Package.Current.Id.Version.Minor,
                Package.Current.Id.Version.Build,
                Package.Current.Id.Version.Revision);
        }

        public static string GetMegaSDK_Version()
        {
            return string.Format("970e65b");
        }

        public static string GetAppUserAgent()
        {
            return string.Format("MEGA_UWP/{0}", GetAppVersion());
        }

        public static string GetDeviceID()
        {
            HardwareToken token = HardwareIdentification.GetPackageSpecificToken(null);
            IBuffer hardwareId = token.Id;

            HashAlgorithmProvider hasher = HashAlgorithmProvider.OpenAlgorithm("MD5");
            IBuffer hashed = hasher.HashData(hardwareId);

            string hashedString = CryptographicBuffer.EncodeToHexString(hashed);
            return hashedString;
        }

        public static void LogoutActions()
        {
            //// Disable the "camera upload" service if is enabled
            //if (MediaService.GetAutoCameraUploadStatus())
            //{
            //    MegaSDK.log(MLogLevel.LOG_LEVEL_INFO, "Disabling CAMERA UPLOADS service (LOGOUT)");
            //    MediaService.SetAutoCameraUpload(false);
            //}

            // Clear settings, cache, previews, thumbnails, etc.
            SettingsService.ClearSettings();
            SettingsService.ClearMegaLoginData();
            //Deployment.Current.Dispatcher.BeginInvoke(() =>
            //{
            //    // Added extra checks preventing null reference exceptions
            //    if (App.MainPageViewModel == null) return;

            //    if (App.MainPageViewModel.CloudDrive != null)
            //        App.MainPageViewModel.CloudDrive.ChildNodes.Clear();

            //    if (App.MainPageViewModel.RubbishBin != null)
            //        App.MainPageViewModel.RubbishBin.ChildNodes.Clear();
            //});
            //AppService.ClearAppCache(false);

            // Delete the User Data
            //App.UserData = null;
        }
    }
}
