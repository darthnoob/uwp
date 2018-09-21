using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;
using mega;
using MegaApp.ViewModels.Settings;

namespace MegaApp.Services
{
    static class SettingsService
    {
        public const string ImageDateSetting = "ImageLastUploadDate";

        private static readonly Mutex FileSettingMutex = new Mutex(false, "FileSettingMutex");
        private static readonly Mutex SettingsMutex = new Mutex(false, "SettingsMutex");

        private static RecoveryKeySettingViewModel _recoveryKeySetting;
        public static RecoveryKeySettingViewModel RecoveryKeySetting
        {
            get
            {
                if (_recoveryKeySetting != null) return _recoveryKeySetting;
                _recoveryKeySetting = new RecoveryKeySettingViewModel();
                _recoveryKeySetting.Initialize();
                return _recoveryKeySetting;
            }
        }

        #region Events

        /// <summary>
        /// Event triggered when a settings reload is requested.
        /// </summary>
        public static event EventHandler ReloadSettingsRequested;

        /// <summary>
        /// Event invocator method called when a settings reload is requested.
        /// </summary>
        public static void ReloadSettings() => ReloadSettingsRequested?.Invoke(null, EventArgs.Empty);

        #endregion

        #region Methods

        /// <summary>
        /// Save a value to the app local settings container
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="key">Key name of the value container</param>
        /// <param name="value">Value to save</param>
        /// <returns>True if save was succesful, else it will return False</returns>
        public static bool Save<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var settings = ApplicationData.Current.LocalSettings;
            if (settings == null) return false;

            try
            {
                if (settings.Values.ContainsKey(key))
                {
                    settings.Values[key] = value;
                }
                else
                {
                    settings.Values.Add(key, value);
                }

                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);
                return false;
            }
        }

        public static T Load<T>(string key, T defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var settings = ApplicationData.Current.LocalSettings;
            if (settings == null) return defaultValue;

            try
            {
                if (settings.Values.ContainsKey(key))
                {
                    return (T) settings.Values[key];
                }
                return defaultValue;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);
                return defaultValue;
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

        public static async Task<T> LoadSettingAsync<T>(string key, T defaultValue)
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
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_LoadSettingsFailed_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_LoadSettingsFailed"), e.Message));
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

        public static async Task<T> LoadSettingAsync<T>(string key)
        {
            return await LoadSettingAsync(key, default(T));
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
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_DeleteSettingsFailed_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_DeleteSettingsFailed"), e.Message));
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
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_DeleteSettingsFailed_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_DeleteSettingsFailed"), e.Message));
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
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_SaveSettingsFailed_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_SaveSettingsFailed"), e.Message));
            }
            finally
            {
                FileSettingMutex.ReleaseMutex();
            }
        }

        public static async Task<T> LoadSettingFromFileAsync<T>(string key)
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
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_LoadSettingsFailed_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_LoadSettingsFailed"), e.Message));
            }
            finally
            {
                FileSettingMutex.ReleaseMutex();
            }

            return returnValue;
        }

        public static async Task<bool> HasValidSessionAsync()
        {
            try
            {
                var hasValidSession = GetCredentialFromLocker(ResourceService.SettingsResources.GetString("SR_UserMegaSession"));
                if (hasValidSession != null) return true;

                // Backward compatibility
                var hasValidSessionFile = await LoadSettingAsync<string>(ResourceService.SettingsResources.GetString("SR_UserMegaSession"));
                return hasValidSessionFile != null ? true : false;
            }
            catch (ArgumentNullException) { return false; }
        }

        /// <summary>
        /// Save the user session ID in the Credential Locker
        /// </summary>
        /// <param name="email">User account email</param>
        /// <param name="session">User session ID</param>
        public static void SaveSessionToLocker(string email, string session)
        {
            SaveCredentialToLocker(
                ResourceService.SettingsResources.GetString("SR_UserMegaSession"),
                email, session);

            DeleteOldMegaLogingDataSettings();
        }

        /// <summary>
        /// Load the user session ID from the Credential Locker
        /// </summary>
        /// <returns>User session ID</returns>
        public static async Task<string> LoadSessionFromLockerAsync()
        {
            var session = LoadCredentialFromLocker(ResourceService.SettingsResources.GetString("SR_UserMegaSession"));
            if (session != null)
            {
                DeleteOldMegaLogingDataSettings();
                return session;
            }

            // Backward compatibility
            var email = await LoadSettingAsync<string>(ResourceService.SettingsResources.GetString("SR_UserMegaEmailAddress"));
            var sessionID = await LoadSettingAsync<string>(ResourceService.SettingsResources.GetString("SR_UserMegaSession"));

            if (email != null && sessionID != null)
                SaveSessionToLocker(email, sessionID);

            return sessionID;
        }

        /// <summary>
        /// Remove the user session ID from the Credential Locker
        /// </summary>
        public static void RemoveSessionFromLocker()
        {
            DeleteOldMegaLogingDataSettings();
            RemoveCredentialFromLocker(ResourceService.SettingsResources.GetString("SR_UserMegaSession"));
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
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_DefaultDownloadLocation"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_ExportImagesToPhotoAlbum"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_QuestionAskedDownloadOption"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_UserPinLockIsEnabled"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_UserPinLock"));

            DeleteFileSetting(ResourceService.SettingsResources.GetString("SR_LastUploadDate"));
        }

        /// <summary>
        /// Save a credential in the Credential Locker
        /// </summary>
        /// <param name="resourceName">Resource name of the credential</param>
        /// <param name="email">User account email</param>
        /// <param name="value">Credential value</param>
        /// <returns>TRUE if all went well or FALSE in other case</returns>
        private static bool SaveCredentialToLocker(string resourceName, string email, string session)
        {
            try
            {
                var vault = new PasswordVault();
                var credential = new PasswordCredential(resourceName, email, session);
                vault.Add(credential);
                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);
                return false;
            }
        }

        /// <summary>
        /// Get a credential from the Credential Locker
        /// </summary>
        /// <param name="resourceName">Resource name of the credential</param>
        /// <returns>The credential if exists. NULL if not exists or something fails</returns>
        private static PasswordCredential GetCredentialFromLocker(string resourceName)
        {
            try
            {
                var vault = new PasswordVault();
                return vault.RetrieveAll().Count > 0 ?
                    vault.FindAllByResource(resourceName).First() : null;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);
                return null;
            }
        }

        /// <summary>
        /// Get the value of a credential from the Credential Locker
        /// </summary>
        /// <param name="resourceName">Resource name of the credential</param>
        /// <returns>The credential value if exists. NULL if not exists or something fails</returns>
        private static string LoadCredentialFromLocker(string resourceName)
        {
            try
            {
                var credential = GetCredentialFromLocker(resourceName);
                if (credential == null) return null;
                credential.RetrievePassword();
                return credential.Password;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);
                return null;
            }
        }

        /// <summary>
        /// Remove a credential from the Credential Locker
        /// </summary>
        /// <param name="resourceName">Resource name of the credential to remove</param>
        /// <returns>TRUE if all went well or FALSE in other case</returns>
        private static bool RemoveCredentialFromLocker(string resourceName)
        {
            try
            {
                var vault = new PasswordVault();
                var credential = GetCredentialFromLocker(resourceName);
                if (credential != null)
                    vault.Remove(credential);
                
                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);
                return false;
            }
        }

        // Backward compatibility
        private static void DeleteOldMegaLogingDataSettings()
        {
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_UserMegaEmailAddress"));
            DeleteSetting(ResourceService.SettingsResources.GetString("SR_UserMegaSession"));

            DeleteFileSetting(ResourceService.SettingsResources.GetString("SR_UserMegaSession"));
        }

        #endregion
    }
}
