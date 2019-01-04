using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;
using mega;

namespace BackgroundTaskService.Services
{
    internal static class SettingsService
    {
        private static readonly Mutex FileSettingMutex = new Mutex(false, "FileSettingMutex");
        private static readonly Mutex SettingsMutex = new Mutex(false, "SettingsMutex");

        public static T LoadSetting<T>(string key, T defaultValue)
        {
            var returnValue = defaultValue;

            try
            {
                SettingsMutex.WaitOne();
                var settings = ApplicationData.Current.LocalSettings;
                if (settings.Values[key] != null)
                    returnValue = (T)settings.Values[key];
            }
            catch (Exception e)
            {
                // Do nothing. Write a log entry and return the default type value
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error loading setting", e);
            }
            finally
            {
                SettingsMutex.ReleaseMutex();
            }

            return returnValue;
        }

        public static async Task SaveSettingToFileAsync<T>(string key, T value)
        {
            try
            {
                FileSettingMutex.WaitOne();

                var settings = ApplicationData.Current.LocalFolder;

                var file = await settings.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    var dataContractSerializer = new DataContractSerializer(typeof(T));
                    dataContractSerializer.WriteObject(stream, value);
                }
            }
            catch (Exception e)
            {
                // Do nothing. Write a log entry and release the mutex
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error saving setting to file", e);
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
                    returnValue = (T) dataContractSerializer.ReadObject(stream);
                }
            }
            catch (Exception e)
            {
                // Do nothing. Write a log entry and release the mutex
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error loading setting from file", e);
            }
            finally
            {
                FileSettingMutex.ReleaseMutex();
            }

            return returnValue;
        }

        /// <summary>
        /// Load the user session ID from the Credential Locker
        /// </summary>
        /// <returns>User session ID</returns>
        public static async Task<string> LoadSessionFromLockerAsync()
        {
            var session = LoadCredentialFromLocker(ResourceService.SettingsResources.GetString("SR_UserMegaSession"));
            if (session != null) return session;

            // Backward compatibility
            return await LoadSettingFromFileAsync<string>(ResourceService.SettingsResources.GetString("SR_UserMegaSession"));
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
    }
}
