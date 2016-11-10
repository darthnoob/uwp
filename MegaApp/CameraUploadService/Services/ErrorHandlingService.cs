using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraUploadService.Services
{
    internal static class ErrorHandlingService
    {
        public const string ImageErrorSetting = "ImageErrorFileName";
        public const string ImageErrorCountSetting = "ImageErrorCount";
        public const int MaxFileUploadErrors = 10;
        

        /// <summary>
        /// Save the filename and error count of that specific file
        /// </summary>
        /// <param name="fileName">The file that failed</param>
        /// <returns>Number of current errors</returns>
        public static async Task<int> SetFileErrorAsync(string fileName)
        {
            try
            {
                // Load filename last error
                var lastErrorFileName = await SettingsService.LoadSettingFromFileAsync<string>("ErrorFileName");

                // Check if it is the same file that has an error again
                if (!string.IsNullOrEmpty(lastErrorFileName) &&
                    lastErrorFileName.Equals(fileName))
                {
                    // If the same file, add to the error count, save and return
                    var count = await SettingsService.LoadSettingFromFileAsync<int>("FileErrorCount");
                    count++;
                    await SettingsService.SaveSettingToFileAsync("FileErrorCount", count);
                    return count;
                }

                // New file error. Save the name and set the error count to one.
                await SettingsService.SaveSettingToFileAsync("ErrorFileName", fileName);
                await SettingsService.SaveSettingToFileAsync("FileErrorCount", 1);
                return 1;
            }
            catch (Exception)
            {
                // Do not let the error process cause the main service to generate a fault
                return 0;
            }
        }

        public static async Task<bool> SkipFile(string fileName)
        {
            var lastErrorFileName = await SettingsService.LoadSettingFromFileAsync<string>("ErrorFileName");
            if (string.IsNullOrEmpty(lastErrorFileName) || !lastErrorFileName.Equals(fileName)) return false;

            var count = await SettingsService.LoadSettingFromFileAsync<int>("FileErrorCount");
            return count >= (MaxFileUploadErrors - 1);
        }
       
        /// <summary>
        /// Clear filename and error count for error processing
        /// </summary>
        public static async Task ClearAsync()
        {
            await SettingsService.SaveSettingToFileAsync("ErrorFileName", string.Empty);
            await SettingsService.SaveSettingToFileAsync("FileErrorCount", 0);
        }
    }
}
