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
            finally
            {
                //FileSettingMutex.ReleaseMutex();
            }
        }

        public static async Task<T> LoadSettingFromFileAsync<T>(string key)
        {
            try
            {
                //FileSettingMutex.WaitOne();

                var settings = ApplicationData.Current.LocalFolder;

                var file = await settings.GetFileAsync(key);

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var dataContractSerializer = new DataContractSerializer(typeof(T));
                    return (T) dataContractSerializer.ReadObject(stream);
                }
            }
            catch
            {
                return default(T);
            }
            finally
            {
                //FileSettingMutex.ReleaseMutex();
            }
        }

    }
}
