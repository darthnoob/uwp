using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace CameraUploadService.Services
{
    internal static class TaskService
    {
        public static async Task<StorageFile> GetAvailableUpload(StorageFolder folder)
        {
            var lastUploadDate = await SettingsService.LoadSettingFromFileAsync<DateTime>("LastUploadDate");
            var files = await folder.GetFilesAsync(CommonFileQuery.OrderByDate);
            // Reorder because order by date query uses different ordering values and descending
            files = files.OrderBy(file => file.DateCreated).ToList();
            // Return the first available file
            return files.FirstOrDefault(file => file.DateCreated.DateTime > lastUploadDate);
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
            return (ulong)Math.Floor(diff.TotalSeconds);
        }
    }
}
