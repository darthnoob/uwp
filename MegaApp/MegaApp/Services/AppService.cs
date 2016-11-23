using System;
using System.IO;
using Windows.Storage;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Views;
using System.Threading;
using System.Threading.Tasks;

namespace MegaApp.Services
{
    class AppService
    {
        /// <summary>
        /// Check if the user has an active and online session
        /// </summary>
        /// <param name="navigationMode">Type of navigation that is taking place </param>
        /// <returns>True if the user has an active and online session or false in other case</returns>
        public static async Task<bool> CheckActiveAndOnlineSession(NavigationMode navigationMode = NavigationMode.New)
        {
            if (!Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn()) && !SettingsService.HasValidSession())
            {
                if(!await CheckSpecialNavigation(false))
                {
                    UiService.OnUiThread(() =>
                    {
                        NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true);
                    });
                }
                
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if need to navigate to a page depending on the current state or the active link.
        /// </summary>
        /// <param name="hasActiveAndOnlineSession">
        /// Bool value that indicates if the user has an active and online session.
        /// </param>
        /// <returns>True if navigates or false in other case.</returns>
        public static async Task<bool> CheckSpecialNavigation(bool hasActiveAndOnlineSession = true)
        {
            if (App.LinkInformation?.ActiveLink != null)
            {
                if ((App.LinkInformation.ActiveLink.Contains("#newsignup")) || 
                    App.LinkInformation.ActiveLink.Contains("#confirm"))
                {
                    if(hasActiveAndOnlineSession)
                    {
                        var customMessageDialog = new CustomMessageDialog(
                            ResourceService.AppMessages.GetString("AM_AlreadyLoggedInAlert_Title"),
                            ResourceService.AppMessages.GetString("AM_AlreadyLoggedInAlert"),
                            App.AppInformation,
                            MessageDialogButtons.YesNo);

                        var dialogResult = await customMessageDialog.ShowDialogAsync();
                        if(dialogResult == MessageDialogResult.OkYes)
                        {
                            // First need to log out of the current account
                            var waitHandleLogout = new AutoResetEvent(false);
                            SdkService.MegaSdk.logout(new LogOutRequestListener(false, waitHandleLogout));
                            waitHandleLogout.WaitOne();

                            return SpecialNavigation();
                        }
                    }
                    else
                    {
                        return SpecialNavigation();
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Navigates to the corresponding page depending on the current state or the active link.
        /// </summary>
        /// <returns>TRUE if navigates or FALSE in other case.</returns>
        private static bool SpecialNavigation()
        {
            if (App.LinkInformation.ActiveLink.Contains("#newsignup"))
            {
                UiService.OnUiThread(() =>
                    NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true));
                return true;
            }
            else if (App.LinkInformation.ActiveLink.Contains("#confirm"))
            {
                UiService.OnUiThread(() =>
                    NavigateService.Instance.Navigate(typeof(ConfirmAccountPage), true));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the app version number
        /// </summary>
        /// <returns>App version number</returns>
        public static string GetAppVersion()
        {
            return string.Format("{0}.{1}.{2}.{3}",
                Package.Current.Id.Version.Major,
                Package.Current.Id.Version.Minor,
                Package.Current.Id.Version.Build,
                Package.Current.Id.Version.Revision);
        }

        /// <summary>
        /// Get the MegaSDK version 
        /// </summary>
        /// <returns>MegaSDK version</returns>
        public static string GetMegaSDK_Version()
        {
            return string.Format("970e65b");
        }

        /// <summary>
        /// Get the app user agent
        /// </summary>
        /// <returns>App user agent</returns>
        public static string GetAppUserAgent()
        {
            return string.Format("MEGA_UWP/{0}", GetAppVersion());
        }

        /// <summary>
        /// Create working directories for the app to use if they do not exist yet
        /// </summary>
        public static void InitializeAppFolders()
        {
            try
            {
                string thumbnailDir = GetThumbnailDirectoryPath();
                if (!Directory.Exists(thumbnailDir)) Directory.CreateDirectory(thumbnailDir);

                string previewDir = GetPreviewDirectoryPath();
                if (!Directory.Exists(previewDir)) Directory.CreateDirectory(previewDir);

                string downloadDir = GetDownloadDirectoryPath();
                if (!Directory.Exists(downloadDir)) Directory.CreateDirectory(downloadDir);

                string uploadDir = GetUploadDirectoryPath();
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
            }
            catch (IOException) { }
        }

        /// <summary>
        /// Get the path of the temporary folder for the upload
        /// </summary>
        /// <param name="checkIfExists">Check if the folder exists</param>
        /// <returns>The folder path</returns>
        public static string GetUploadDirectoryPath(bool checkIfExists = false)
        {
            var uploadDir = Path.Combine(ApplicationData.Current.LocalFolder.Path, 
                ResourceService.AppResources.GetString("AR_UploadsDirectory"));

            if (checkIfExists)
            {
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);
            }

            return uploadDir;
        }

        /// <summary>
        /// Get the path of the temporary folder for the downloads
        /// </summary>
        /// <returns>The folder path</returns>
        public static string GetDownloadDirectoryPath()
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                ResourceService.AppResources.GetString("AR_DownloadsDirectory"));
        }

        /// <summary>
        /// Get the path of the folder to store the previews
        /// </summary>
        /// <returns>The folder path</returns>
        public static string GetPreviewDirectoryPath()
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                ResourceService.AppResources.GetString("AR_PreviewsDirectory"));
        }

        /// <summary>
        /// Get the path of the folder to store the thumbnails
        /// </summary>
        /// <returns>The folder path</returns>
        public static string GetThumbnailDirectoryPath()
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                ResourceService.AppResources.GetString("AR_ThumbnailsDirectory"));
        }

        public static void ClearAppCache(bool includeLocalFolder)
        {
            if (includeLocalFolder)
                ClearLocalCache();
            ClearThumbnailCache();
            ClearPreviewCache();
            ClearDownloadCache();
            ClearUploadCache();

            //ClearAppDatabase();
        }

        //public static void ClearAppDatabase()
        //{
        //    SavedForOffline.DeleteAllNodes();
        //}

        /// <summary>
        /// Clear the thumbnails cache
        /// </summary>
        public static void ClearThumbnailCache()
        {
            string thumbnailDir = GetThumbnailDirectoryPath();
            if (!String.IsNullOrWhiteSpace(thumbnailDir) && !FolderService.HasIllegalChars(thumbnailDir) &&
                Directory.Exists(thumbnailDir))
            {
                FileService.ClearFiles(Directory.GetFiles(thumbnailDir));
            }
        }

        /// <summary>
        /// Clear the previews cache
        /// </summary>
        public static void ClearPreviewCache()
        {
            string previewDir = GetPreviewDirectoryPath();
            if (!String.IsNullOrWhiteSpace(previewDir) && !FolderService.HasIllegalChars(previewDir) &&
                Directory.Exists(previewDir))
            {
                FileService.ClearFiles(Directory.GetFiles(previewDir));
            }
        }

        /// <summary>
        /// Clear the downloads cache
        /// </summary>
        public static void ClearDownloadCache()
        {
            string downloadDir = GetDownloadDirectoryPath();
            if (!String.IsNullOrWhiteSpace(downloadDir) && !FolderService.HasIllegalChars(downloadDir) &&
                Directory.Exists(downloadDir))
            {
                FolderService.Clear(downloadDir);
            }
        }

        /// <summary>
        /// Clear the uploads cache
        /// </summary>
        public static void ClearUploadCache()
        {
            string uploadDir = GetUploadDirectoryPath();
            if (!String.IsNullOrWhiteSpace(uploadDir) && !FolderService.HasIllegalChars(uploadDir) &&
                Directory.Exists(uploadDir))
            {
                FileService.ClearFiles(Directory.GetFiles(uploadDir));
            }
        }

        /// <summary>
        /// Clear the app local cache
        /// </summary>
        public static void ClearLocalCache()
        {
            string localCacheDir = ApplicationData.Current.LocalFolder.Path;
            if (!String.IsNullOrWhiteSpace(localCacheDir) && !FolderService.HasIllegalChars(localCacheDir) &&
                Directory.Exists(localCacheDir))
            {
                FileService.ClearFiles(Directory.GetFiles(localCacheDir));
            }
        }

        /// <summary>
        /// Method that executes the actions needed for a logout
        /// </summary>
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
            AppService.ClearAppCache(false);

            // Delete the User Data
            //App.UserData = null;
        }

        /// <summary>
        /// Set the software back button visibility for the app view
        /// (Only for desktop or devices without hardware button)
        /// </summary>
        /// <param name="isVisible">TRUE for visible or FALSE for hidden</param>
        public static void SetAppViewBackButtonVisibility(bool isVisible)
        {
            var computedVisible = isVisible || NavigateService.MainFrame.CanGoBack;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
               computedVisible ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }
    }
}
