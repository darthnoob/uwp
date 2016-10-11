using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using MegaApp.Classes;

namespace MegaApp.Services
{
    static class SettingsService
    {
        private static readonly Mutex FileSettingMutex = new Mutex(false, "FileSettingMutex");
        private static readonly Mutex SettingsMutex = new Mutex(false, "SettingsMutex");

        public static async void SaveSetting<T>(string key, T value)
        {
            try
            {
                SettingsMutex.WaitOne();
                var settings = ApplicationData.Current.LocalSettings;
                settings.Values[key] = value;
            }
            catch (Exception e)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_SaveSettingsFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_SaveSettingsFailed"), e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
            }
            finally
            {
                SettingsMutex.ReleaseMutex();
            }
        }

        //public static void SecureSaveSetting(string key, string value)
        //{
        //    try
        //    {
        //        SettingsMutex.WaitOne();

        //        var settings = IsolatedStorageSettings.ApplicationSettings;

        //        if (settings.Contains(key))
        //            settings[key] = CryptoService.EncryptData(value);
        //        else
        //            settings.Add(key, CryptoService.EncryptData(value));

        //        settings.Save();
        //    }
        //    catch (Exception e)
        //    {
        //        Deployment.Current.Dispatcher.BeginInvoke(() =>
        //        {
        //            new CustomMessageDialog(
        //                AppMessages.SaveSettingsFailed_Title,
        //                String.Format(AppMessages.SaveSettingsFailed, e.Message),
        //                App.AppInformation,
        //                MessageDialogButtons.Ok).ShowDialog();
        //        });
        //    }
        //    finally
        //    {
        //        SettingsMutex.ReleaseMutex();
        //    }
        //}

        public static async Task<T> LoadSetting<T>(string key, T defaultValue)
        {
            var returnValue = defaultValue;

            try
            {
                SettingsMutex.WaitOne();
                var settings = ApplicationData.Current.LocalSettings;                
                if (settings.Values[key] != null)
                    returnValue = (T)settings.Values[key];
            }
            catch(Exception e)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_LoadSettingsFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_LoadSettingsFailed"), e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
            }
            finally
            {
                SettingsMutex.ReleaseMutex();                
            }

            return returnValue;
        }

        //public static string SecureLoadSetting(string key)
        //{
        //    return SecureLoadSetting(key, null);
        //}

        //public static string SecureLoadSetting(string key, string defaultValue)
        //{
        //    var returnValue = defaultValue;

        //    try
        //    {
        //        SettingsMutex.WaitOne();

        //        var settings = IsolatedStorageSettings.ApplicationSettings;                

        //        if (settings.Contains(key))
        //            returnValue = CryptoService.DecryptData((string)settings[key]);
        //    }
        //    catch (Exception e)
        //    {
        //        Deployment.Current.Dispatcher.BeginInvoke(() =>
        //        {
        //            new CustomMessageDialog(
        //                AppMessages.AM_LoadSettingsFailed_Title,
        //                String.Format(AppMessages.AM_LoadSettingsFailed, e.Message),
        //                App.AppInformation,
        //                MessageDialogButtons.Ok).ShowDialog();
        //        });                
        //    }
        //    finally
        //    {
        //        SettingsMutex.ReleaseMutex();
        //    }

        //    return returnValue;
        //}

        public static Task<T> LoadSetting<T>(string key)
        {
            return LoadSetting(key, default(T));
        }

        public static async void DeleteSetting(string key)
        {
            try
            {
                SettingsMutex.WaitOne();

                var settings = ApplicationData.Current.LocalSettings;
                if (settings.Values[key] != null)
                    settings.Values[key] = null;
            }
            catch (Exception e)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_DeleteSettingsFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_DeleteSettingsFailed"), e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
            }
            finally
            {
                SettingsMutex.ReleaseMutex();
            }
        }

        public static async void DeleteFileSetting(string key)
        {
            try
            {
                FileSettingMutex.WaitOne();

                var settings = ApplicationData.Current.LocalFolder;

                Task.WaitAll(Task.Run(async () =>
                {
                    try
                    {
                        // Checking to try avoid "FileNotFoundException"
                        if (FileService.FileExists(Path.Combine(settings.Path, key)))
                        {
                            var file = await settings.GetFileAsync(key);
                            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        }
                    }
                    catch (FileNotFoundException) { /* Do nothing */ }
                }));
            }
            catch (Exception e)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_DeleteSettingsFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_DeleteSettingsFailed"), e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
            }
            finally
            {
                FileSettingMutex.ReleaseMutex();
            }
        }

        public static async void SaveSettingToFile<T>(string key, T value)
        {
            try
            {
                FileSettingMutex.WaitOne();

                var settings = ApplicationData.Current.LocalFolder;

                Task.WaitAll(Task.Run(async () =>
                {
                    var file = await settings.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);

                    using (var stream = await file.OpenStreamForWriteAsync())
                    {
                        var dataContractSerializer = new DataContractSerializer(typeof(T));
                        dataContractSerializer.WriteObject(stream, value);
                    }
                }));
            }
            catch (Exception e)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_SaveSettingsFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_SaveSettingsFailed"), e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
            }
            finally
            {
                FileSettingMutex.ReleaseMutex();
            }
        }

        public static async Task<T> LoadSettingFromFile<T>(string key)
        {
            var returnValue = default(T);

            try
            {
                FileSettingMutex.WaitOne();

                var settings = ApplicationData.Current.LocalFolder;

                var file = await settings.GetFileAsync(key);

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var dataContractSerializer = new DataContractSerializer(typeof(T));
                    returnValue = (T)dataContractSerializer.ReadObject(stream);
                }
            }
            catch (Exception e)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_LoadSettingsFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_LoadSettingsFailed"), e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
            }
            finally
            {
                FileSettingMutex.ReleaseMutex();
            }

            return returnValue;
        }

        public async static Task<bool> HasValidSession()
        {
            try
            {
                if (await LoadSetting<string>(ResourceService.SettingsResources.GetString("SR_UserMegaSession")) != null)
                    return true;
                else
                    return false;
            }
            catch (ArgumentNullException) { return false; }
        }

        /// <summary>
        /// Save all the login data settings
        /// </summary>
        /// <param name="email">User account email</param>
        /// <param name="session">User session ID</param>
        public static void SaveMegaLoginData(string email, string session)
        {
            SaveSetting(ResourceService.SettingsResources.GetString("SR_UserMegaEmailAddress"), email);
            SaveSetting(ResourceService.SettingsResources.GetString("SR_UserMegaSession"), session);

            // Save session for automatic camera upload agent
            SaveSettingToFile(ResourceService.SettingsResources.GetString("SR_UserMegaSession"), session);
        }

        /// <summary>
        /// Clear all the login data settings
        /// </summary>
        public static void ClearMegaLoginData()
        {
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_UserMegaEmailAddress"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_UserMegaSession"));

            DeleteFileSetting(ResourceService.SettingsResources.GetString("SR_UserMegaSession"));
        }

        /// <summary>
        /// Clear all the user settings.
        /// </summary>
        public static void ClearSettings()
        {
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_AskDownloadLocationIsEnabled"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_CameraUploadsConnectionType"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_CameraUploadsFileType"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_CameraUploadsFirstInit"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_CameraUploadsIsEnabled"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_DebugModeIsEnabled"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_DefaultDownloadLocation"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_ExportImagesToPhotoAlbum"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_QuestionAskedDownloadOption"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_UserPinLockIsEnabled"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_UserPinLock"));

            DeleteFileSetting(ResourceService.SettingsResources.GetString("SR_LastUploadDate"));
        }
    }
}
