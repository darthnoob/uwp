using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using BackgroundTaskService.MegaApi;
using BackgroundTaskService.Services;
using mega;

namespace BackgroundTaskService
{
    public sealed class CameraUploadTask : IBackgroundTask
    {
        // If you run any asynchronous code in your background task, then your background task needs to use a deferral. 
        // If you don't use a deferral, then the background task process can terminate unexpectedly
        // if the Run method completes before your asynchronous method call has completed.
        // Note: defined at class scope so we can mark it complete inside the OnCancel() callback if we choose to support cancellation
        BackgroundTaskDeferral _deferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            // Enable a custom logger
            LogService.SetLoggerObject(new MegaLogger());

            SdkService.InitializeSdkParams();

            var loggedIn = await LoginAsync();

            if (loggedIn)
            {
                var fetched = await FetchNodesAsync();
                if (fetched)
                {
                    var megaGlobalListener = new MegaGlobalListener();
                    SdkService.MegaSdk.addGlobalListener(megaGlobalListener);
                    await megaGlobalListener.ExecuteAsync(() => SdkService.MegaSdk.enableTransferResumption());

                    var cameraUploadRootNode = await SdkService.GetCameraUploadRootNodeAsync();
                    if (cameraUploadRootNode == null) return;

                    var fileToUpload = await TaskService.GetAvailableUpload(KnownFolders.PicturesLibrary,
                        TaskService.ImageDateSetting);
                    foreach (var storageFile in fileToUpload)
                    {
                        // Skip the current file if it has failed more than the max error count
                        if(await ErrorHandlingService.SkipFile(
                            storageFile.Name,
                            ErrorHandlingService.ImageErrorFileSetting, 
                            ErrorHandlingService.ImageErrorCountSetting)) continue;

                        // Calculate time for fingerprint check and upload
                        ulong mtime = TaskService.CalculateMtime(storageFile.DateCreated.DateTime);
                        try
                        {
                            using (var fs = await storageFile.OpenStreamForReadAsync())
                            {
                                var isUploaded = SdkService.IsAlreadyUploaded(storageFile, fs, cameraUploadRootNode, mtime);
                                if (isUploaded)
                                {
                                    await TaskService.SaveLastUploadDateAsync(storageFile, TaskService.ImageDateSetting);
                                    continue;
                                }
                                await SdkService.UploadAsync(storageFile, fs, cameraUploadRootNode, mtime);
                                // No error, clear error storage
                                await ErrorHandlingService.ClearAsync();
                            }
                        }
                        catch (OutOfMemoryException e)
                        {
                            // Something went wrong (could be memory limit)
                            // Just finish this run and try again next time
                            LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Out of memory while uploading", e);
                            break;
                        }
                        catch (Exception e)
                        {
                            LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error uploading item", e);
                            await ErrorHandlingService.SetFileErrorAsync(storageFile.Name, 
                                ErrorHandlingService.ImageErrorFileSetting, ErrorHandlingService.ImageErrorCountSetting);
                        }
                    }
                }
            }
            
            _deferral.Complete();
        }

        /// <summary>
        /// Fetch the nodes from MEGA
        /// </summary>
        /// <returns>True if succeeded, else False</returns>
        private static async Task<bool> FetchNodesAsync()
        {
            var fetch = new MegaRequestListener<bool>();
            return await fetch.ExecuteAsync(() => SdkService.MegaSdk.fetchNodes(fetch));
        }

        /// <summary>
        /// Fastlogin to MEGA user account
        /// </summary>
        /// <returns>True if succeeded, else false</returns>
        private static async Task<bool> LoginAsync()
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
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error uploading item", e);
                return false;
            }
        }
    }
}
