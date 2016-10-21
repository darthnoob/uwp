using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MegaApp.Views;

namespace MegaApp.Services
{
    class AppService
    {
        public static async Task<bool> CheckActiveAndOnlineSession(NavigationMode navigationMode = NavigationMode.New)
        {
            if (!Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn()) && !await SettingsService.HasValidSession())
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>                    
                    (Window.Current.Content as Frame).Navigate(typeof(LoginAndCreateAccountPage)));

                return false;
            }

            return true;
        }

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
