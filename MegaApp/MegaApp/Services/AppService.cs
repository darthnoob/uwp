using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.UI.Core;
using mega;
using MegaApp.Classes;
using MegaApp.Database;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Views;
using MegaApp.Views.CreateAccount;
using MegaApp.Views.Login;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Services
{
    class AppService
    {
        /// <summary>
        /// Check if the user has an active and online session
        /// </summary>
        /// <param name="byMainPage">The caller of method</param>
        /// <returns>True if the user has an active and online session or false in other case</returns>
        public static async Task<bool> CheckActiveAndOnlineSessionAsync(bool byMainPage = false)
        {
            var hasActiveAndOnlineSession = 
                Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn()) ||
                await SettingsService.HasValidSessionAsync();

            if (byMainPage)
            {

                if (!hasActiveAndOnlineSession)
                {
                    UiService.OnUiThread(() =>
                    {
                        NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true);
                    });
                }

                return hasActiveAndOnlineSession;
            }
            
            if (!await CheckSpecialNavigation(hasActiveAndOnlineSession))
            {
                UiService.OnUiThread(() =>
                {
                    NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true);
                });
            }

            return hasActiveAndOnlineSession;
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
            if (LinkInformationService.ActiveLink == null) return false;

            if (LinkInformationService.ActiveLink.Contains("#newsignup") ||
                LinkInformationService.ActiveLink.Contains("#confirm") ||
                LinkInformationService.ActiveLink.Contains("#recover"))
            {
                if (!hasActiveAndOnlineSession) return await SpecialNavigation();

                var result = await DialogService.ShowOkCancelAsync(
                    ResourceService.AppMessages.GetString("AM_AlreadyLoggedInAlert_Title"),
                    ResourceService.AppMessages.GetString("AM_AlreadyLoggedInAlert"),
                    OkCancelDialogButtons.YesNo);

                if (!result) return false;

                // First need to log out of the current account
                var waitHandleLogout = new AutoResetEvent(false);
                SdkService.MegaSdk.logout(new LogOutRequestListener(false, waitHandleLogout));
                waitHandleLogout.WaitOne();

                return await SpecialNavigation();
            }

            if (LinkInformationService.ActiveLink.Contains("#verify"))
            {
                if (hasActiveAndOnlineSession)
                    return await SpecialNavigation();

                await DialogService.ShowAlertAsync(
                    ResourceService.UiResources.GetString("UI_ChangeEmail"),
                    ResourceService.AppMessages.GetString("AM_UserNotOnline"));
            }
            else if (LinkInformationService.ActiveLink.Contains("#F!"))
            {
                if (hasActiveAndOnlineSession)
                    return await SpecialNavigation();

                await DialogService.ShowAlertAsync(
                    ResourceService.UiResources.GetString("UI_FolderLink"),
                    ResourceService.AppMessages.GetString("AM_UserNotOnline"));
            }
            else if (LinkInformationService.ActiveLink.Contains("#!"))
            {
                if (hasActiveAndOnlineSession)
                    return await SpecialNavigation();

                await DialogService.ShowAlertAsync(
                    ResourceService.UiResources.GetString("UI_FileLink"),
                    ResourceService.AppMessages.GetString("AM_UserNotOnline"));
            }

            return false;
        }

        /// <summary>
        /// Navigates to the corresponding page depending on the current state or the active link.
        /// </summary>
        /// <returns>TRUE if navigates or FALSE in other case.</returns>
        private static async Task<bool> SpecialNavigation()
        {
            if (LinkInformationService.ActiveLink.Contains("#newsignup"))
            {
                UiService.OnUiThread(() =>
                    NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true));
                return true;
            }

            if (LinkInformationService.ActiveLink.Contains("#confirm"))
            {
                var signUp = new QuerySignUpLinkRequestListenerAsync();
                var result = await signUp.ExecuteAsync(() =>
                {
                    SdkService.MegaSdk.querySignupLink(LinkInformationService.ActiveLink, signUp);
                });

                switch (result)
                {
                    case SignUpLinkType.Valid:
                        UiService.OnUiThread(() =>
                            NavigateService.Instance.Navigate(typeof(ConfirmAccountPage), true,
                                new NavigationObject
                                {
                                    Action = NavigationActionType.Default,
                                    Parameters = new Dictionary<NavigationParamType, object>
                                    {
                                        { NavigationParamType.Email, signUp.EmailAddress },
                                        { NavigationParamType.Data, LinkInformationService.ActiveLink },
                                    }
                                }));
                        return true;
                    case SignUpLinkType.AlreadyConfirmed:
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_AlreadyConfirmedAccount_Title"),
                            ResourceService.AppMessages.GetString("AM_AlreadyConfirmedAccount"));
                        break;
                    case SignUpLinkType.Expired:
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_SignUpLinkExpired_Title"),
                            ResourceService.AppMessages.GetString("AM_SignUpLinkExpired"));
                        break;
                    case SignUpLinkType.Unknown:
                    case SignUpLinkType.Invalid:
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_InvalidSignUpLink_Title"),
                            ResourceService.AppMessages.GetString("AM_InvalidSignUpLink"));
                        break;
                }

                return false;
            }

            if (LinkInformationService.ActiveLink.Contains("#verify"))
            {
                UiService.OnUiThread(() =>
                    NavigateService.Instance.Navigate(typeof(ConfirmChangeEmailPage), true));
                return true;
            }

            if (LinkInformationService.ActiveLink.Contains("#recover"))
            {
                // Check if it is recover or park account
                var query = new QueryPasswordLinkRequestListenerAsync();
                var result = await query.ExecuteAsync(() =>
                {
                    SdkService.MegaSdk.queryResetPasswordLink(LinkInformationService.ActiveLink, query);
                });

                switch (result)
                {
                    case RecoverLinkType.Recovery:
                        UiService.OnUiThread(() => NavigateService.Instance.Navigate(typeof(RecoverPage), true));
                        return true;
                    case RecoverLinkType.ParkAccount:
                        UiService.OnUiThread(() => NavigateService.Instance.Navigate(typeof(ConfirmParkAccountPage), true));
                        return true;
                    case RecoverLinkType.Expired:
                        await DialogService.ShowAlertAsync(
                                ResourceService.AppMessages.GetString("AM_RecoveryLinkExpired_Title"),
                                ResourceService.AppMessages.GetString("AM_RecoveryLinkExpired"));
                        break;
                    case RecoverLinkType.Unknown:
                       
                        await DialogService.ShowAlertAsync(
                            ResourceService.AppMessages.GetString("AM_InvalidRecoveryLink_Title"),
                            ResourceService.AppMessages.GetString("AM_InvalidRecoveryLink"));
                        break;

                }
            }

            if (LinkInformationService.ActiveLink.Contains("#F!"))
            {
                LinkInformationService.UriLink = UriLinkType.Folder;

                // Navigate to the folder link page
                UiService.OnUiThread(() =>
                    NavigateService.Instance.Navigate(typeof(FolderLinkPage)));
                return true;
            }

            if (LinkInformationService.ActiveLink.Contains("#!"))
            {
                LinkInformationService.UriLink = UriLinkType.Folder;

                // Navigate to the file link page
                UiService.OnUiThread(() =>
                    NavigateService.Instance.Navigate(typeof(FileLinkPage)));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the app name
        /// </summary>
        /// <returns>App name</returns>
        public static string GetAppName()
        {
            return ResourceService.AppResources.GetString("AR_ApplicationName");
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
            return ResourceService.AppResources.GetString("AR_SdkVersion");
        }

        /// <summary>
        /// Get the MegaSDK link 
        /// </summary>
        /// <returns>MegaSDK link</returns>
        public static string GetMegaSDK_Link()
        {
            return ResourceService.AppResources.GetString("AR_SdkLink");
        }

        /// <summary>
        /// Get the app user agent
        /// </summary>
        /// <returns>App user agent</returns>
        public static string GetAppUserAgent()
        {
            // Get an instance of the object that allow recover the local device information.
            var deviceInfo = new EasClientDeviceInformation();

            return string.Format(
                "MEGA_UWP/{0}/{1}/{2}/{3}",
                GetAppVersion(),
                deviceInfo.SystemManufacturer,
                deviceInfo.SystemProductName,
                deviceInfo.OperatingSystem);
        }

        /// <summary>
        /// Get the code of the language used by the app
        /// </summary>
        /// <returns>Code of the language used by the app or an empty string if fails</returns>
        public static string GetAppLanguageCode()
        {
            try
            {
                CultureInfo ci = CultureInfo.CurrentUICulture;
                var languageCode = ci.TwoLetterISOLanguageName;

                switch (languageCode)
                {
                    case null:
                        LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error getting the app language code");
                        return string.Empty;
                    case "pt":
                        return (ci.Name.Equals("pt-BR")) ? ci.Name : languageCode;
                    case "zh":
                        return (ci.Name.Equals("zh-HANS") || ci.Name.Equals("zh-HANT")) ? ci.Name : languageCode;
                    default:
                        return languageCode;
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error getting the app language code", e);
                return string.Empty;
            }
        }

        /// <summary>
        /// Initialize the DB (create tables if no exist).
        /// </summary>
        public static void InitializeDatabase()
        {
            SavedForOfflineDB.CreateTable();
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

                string offlineDir = GetOfflineDirectoryPath();
                if (!Directory.Exists(offlineDir)) Directory.CreateDirectory(offlineDir);
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
        /// Get the path of the offline folder
        /// </summary>
        /// <returns>The folder path</returns>
        public static string GetOfflineDirectoryPath()
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                ResourceService.AppResources.GetString("AR_OfflineDirectory"));
        }

        /// <summary>
        /// Gets the log file path created in DEBUG mode.
        /// </summary>
        /// <returns>Log file path.</returns>
        public static string GetFileLogPath()
        {
            return Path.Combine(GetOfflineDirectoryPath(),
                ResourceService.AppResources.GetString("AR_LogFileName"));
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

            ClearAppDatabase();
        }

        /// <summary>
        /// Clear the database cache
        /// </summary>
        public static void ClearAppDatabase()
        {
            SavedForOfflineDB.DeleteAllNodes();
        }

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
            // Disable the "Camera Uploads" service if is enabled
            if (TaskService.IsBackGroundTaskActive(TaskService.CameraUploadTaskEntryPoint, TaskService.CameraUploadTaskName))
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Disabling CAMERA UPLOADS service (LOGOUT)");
                TaskService.UnregisterBackgroundTask(TaskService.CameraUploadTaskEntryPoint, TaskService.CameraUploadTaskName);
            }

            // Clear settings, cache, previews, thumbnails, etc.
            SettingsService.ClearSettings();
            SettingsService.ClearMegaLoginData();
            ClearAppCache(false);

            // Clear all the account and user data info
            AccountService.ClearAccountDetails();
            AccountService.ClearUserData();

            // Clear all the contacts info
            ContactsService.Clear();
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

        public static async Task ContactSupport()
        {
            await EmailManager.ShowComposeNewEmailAsync(new EmailMessage
            {
                To = { new EmailRecipient(ResourceService.AppResources.GetString("AR_SupportEmailAddress")) }
            });
        }
    }
}
