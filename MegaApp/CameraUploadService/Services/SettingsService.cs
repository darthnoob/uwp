using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace CameraUploadService.Services
{
    internal static class SettingsService
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
                
            }
            finally
            {
                SettingsMutex.ReleaseMutex();
            }
        }
        

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
               
            }
            finally
            {
                SettingsMutex.ReleaseMutex();                
            }

            return returnValue;
        }

      

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
                if (settings.Values.ContainsKey(key) && settings.Values[key] != null)
                    settings.Values[key] = null;
            }
            catch (Exception e)
            {
                   
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
                        //if (FileService.FileExists(Path.Combine(settings.Path, key)))
                        //{
                        //    var file = await settings.GetFileAsync(key);
                        //    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        //}
                    }
                    catch (FileNotFoundException) { /* Do nothing */ }
                }));
            }
            catch (Exception e)
            {
                
            }
            finally
            {
                FileSettingMutex.ReleaseMutex();
            }
        }

        public static async Task SaveSettingToFileAsync<T>(string key, T value)
        {
            try
            {
                //FileSettingMutex.WaitOne();

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
                
            }
            finally
            {
                //FileSettingMutex.ReleaseMutex();
            }
        }

        public static async Task<T> LoadSettingFromFileAsync<T>(string key)
        {
            var returnValue = default(T);

            try
            {
                //FileSettingMutex.WaitOne();

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
               
            }
            finally
            {
                //FileSettingMutex.ReleaseMutex();
            }

            return returnValue;
        }

    }
}
