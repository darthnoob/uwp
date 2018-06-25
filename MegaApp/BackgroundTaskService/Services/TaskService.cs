using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using mega;

namespace BackgroundTaskService.Services
{
    /// <summary>
    /// Class that handles Task logic
    /// </summary>
    internal static class TaskService
    {
        public const string ImageDateSetting = "ImageLastUploadDate";

        /// <summary>
        /// Get available files for upload depending on the Last Upload Date setting
        /// </summary>
        /// <param name="folders">StorageFolders to check for available files</param>
        /// <param name="dateSetting">Name of the date setting to load from settings</param>
        /// <returns>Available files for upload</returns>
        public static async Task<IList<StorageFile>> GetAvailableUploadAsync(string dateSetting, params StorageFolder[] folders)
        {
            try
            {
                var lastUploadDate = await SettingsService.LoadSettingFromFileAsync<DateTime>(dateSetting);

                var upload = new List<StorageFile>();
                foreach (var folder in folders)
                {
                    var files = (await folder.GetFilesAsync(CommonFileQuery.OrderByDate)).ToList();
                    
                    // Reorder because order by date query uses different ordering values and descending
                    files = files.OrderBy(file => file.DateCreated).ToList();
                    
                    // >= to get all files that have the same creation date
                    upload.AddRange(files.Where(file => file.DateCreated.DateTime >= lastUploadDate));
                }

                return upload;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error getting the available files for upload", e);
                return new List<StorageFile>();
            }
        }

        /// <summary>
        /// Save the creation date of the current file to the Last Upload Date settings
        /// </summary>
        /// <param name="fileToUpload">File to get creation date</param>
        /// <param name="dateSetting">Name of the date setting to save to settings</param>
        public static async Task SaveLastUploadDateAsync(StorageFile fileToUpload, string dateSetting)
        {
            await SettingsService.SaveSettingToFileAsync(dateSetting, fileToUpload.DateCreated.DateTime);
        }

        /// <summary>
        /// Get the temporary upload folder path
        /// </summary>
        /// <returns>Temporary upload folder path</returns>
        public static string GetTemporaryUploadFolder()
        {
            var uploadDir = Path.Combine(ApplicationData.Current.LocalFolder.Path, @"uploads\");

            // Check if the temporary upload folder exists or create it if not exists
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            return uploadDir;
        }

        /// <summary>
        /// Calculate mtime
        /// </summary>
        /// <param name="inputDateTime">Datetime to calculate</param>
        /// <returns>Mtime as ulong</returns>
        public static ulong CalculateMtime(DateTime inputDateTime)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = inputDateTime.ToUniversalTime() - origin;
            return (ulong) Math.Floor(diff.TotalSeconds);
        }
    }
}
