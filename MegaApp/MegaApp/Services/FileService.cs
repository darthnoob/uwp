using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using mega;
using MegaApp.Enums;

namespace MegaApp.Services
{
    static class FileService
    {
        /// <summary>
        /// Determines if exists the specified file
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns>TRUE if the file exists or FALSE in other case</returns>
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static FileType GetFileType(string filename)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(Path.GetExtension(filename)))
                    return FileType.TYPE_UNKNOWN;

                if (ImageService.IsImage(filename))
                    return FileType.TYPE_IMAGE;
                else if (AudioService.IsAudio(filename))
                    return FileType.TYPE_AUDIO;
                else if (VideoService.IsVideo(filename))
                    return FileType.TYPE_VIDEO;
                else
                    return FileType.TYPE_FILE;
            }
            catch (Exception)
            {
                return FileType.TYPE_UNKNOWN;
            }
        }

        /// <summary>
        /// Determines if a file is a temporary file of a pending transfer
        /// </summary>
        /// <param name="filename">Name of the file to check</param>
        /// <returns>TRUE if is a temporary file of a pending transfer or FALSE in other case</returns>
        public static bool IsPendingTransferFile(string filename)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filename) || HasIllegalChars(filename)) return false;

                string extension = Path.GetExtension(filename);

                if (string.IsNullOrEmpty(extension)) return false;

                switch (extension.ToLower())
                {
                    case ".mega":
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Delete the specified file
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns>TRUE if the deletion was well or FALSE in other case</returns>
        public static bool DeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    File.Delete(path);
                return true;
            }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// Determines if a filename contains illegal characters
        /// </summary>
        /// <param name="filename">Name of the file to check</param>
        /// <returns>TRUE if has illegal chars or FALSE in other case</returns>
        public static bool HasIllegalChars(string filename)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                if (filename.Contains(c.ToString())) return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes a list of files
        /// </summary>
        /// <param name="filesToDelete">List of files to delete</param>
        /// <returns>TRUE if the deletion was well or FALSE in other case</returns>
        public static bool ClearFiles(IEnumerable<string> filesToDelete)
        {
            if (filesToDelete == null) return false;

            bool result = true;
            foreach (var file in filesToDelete)
                result = result & DeleteFile(file);

            return result;
        }

        /// <summary>
        /// Copy a file to a specified folder and allow rename the destination file
        /// </summary>
        /// <param name="srcFilePath">Path of the source file</param>
        /// <param name="destFolderPath">Path of the destination folder</param>
        /// <param name="fileNewName">New name for the destination file</param>
        /// <returns>TRUE if the file was copied or FALSE if something failed</returns>
        public static async Task<bool> CopyFileAsync(string srcFilePath, string destFolderPath,
            string fileNewName = null, bool isForMove = false)
        {
            try 
            {
                var srcFile = await StorageFile.GetFileFromPathAsync(srcFilePath);
                if (srcFile == null)
                {
                    string errorMessage = "Source file does not exist or could not be found: " + srcFilePath;
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, errorMessage);
                    return false;
                }

                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destFolderPath))
                    Directory.CreateDirectory(destFolderPath);

                var destFolder = await StorageFolder.GetFolderFromPathAsync(destFolderPath);
                if (destFolder == null)
                {
                    string errorMessage = "Destination folder does not exist or could not be found: " + destFolderPath;
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, errorMessage);
                    return false;
                }

                fileNewName = fileNewName ?? srcFile.Name;
                
                using (var folderStream = await destFolder.OpenStreamForWriteAsync(fileNewName, CreationCollisionOption.GenerateUniqueName))
                {
                    // Set buffersize to avoid copy failure of large files
                    var fileStream = await srcFile.OpenStreamForReadAsync();
                    await fileStream.CopyToAsync(folderStream, 8192);
                    await folderStream.FlushAsync();
                }

                return true;
            }
            catch (Exception e) 
            {
                if (!isForMove)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error copying file:", e);
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Source: " + srcFilePath);
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Destination: " + destFolderPath);
                }

                return false;
            }
        }

        /// <summary>
        /// Move a file to a specified folder and allow rename the destination file.
        /// Copy the file and remove the source file if the copy was successful.
        /// </summary>
        /// <param name="srcFilePath">Path of the source file</param>
        /// <param name="destFolderPath">Path of the destination folder</param>
        /// <param name="fileNewName">New name for the destination file</param>
        /// <returns>TRUE if the file was moved or FALSE if something failed</returns>
        public static async Task<bool> MoveFileAsync(string srcFilePath, string destFolderPath, string fileNewName = null)
        {
            try
            {
                bool result = true;
                result &= await CopyFileAsync(srcFilePath, destFolderPath, fileNewName, true);
                result &= DeleteFile(srcFilePath);
                return result;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error moving file:", e);
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Source: " + srcFilePath);
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Destination: " + destFolderPath);
                return false;
            }
        }
       
        public static async Task<bool> OpenFile(string filePath)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(filePath);

                if (file != null)
                    return await Windows.System.Launcher.LaunchFileAsync(file);

                UiService.OnUiThread(async() =>
                {
                     await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_FileNotFound_Title"),
                        ResourceService.AppMessages.GetString("AM_FileNotFound"));
                });

                return false;
            }
            catch (Exception)
            {
                UiService.OnUiThread(async() =>
                {
                     await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_OpenFileFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_OpenFileFailed"));
                });

                return false;
            }            
        }

        /// <summary>
        /// Selects multiple files using the system file picker
        /// </summary>
        /// <returns>A list with the selected files</returns>
        public static async Task<IReadOnlyList<StorageFile>> SelectMultipleFiles()
        {
            try
            {
                var fileOpenPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.ComputerFolder,
                    CommitButtonText = ResourceService.UiResources.GetString("UI_Upload")
                };
                fileOpenPicker.FileTypeFilter.Add("*");

                return await fileOpenPicker.PickMultipleFilesAsync();                
            }
            catch (Exception e)
            {
                UiService.OnUiThread(async() =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_SelectFileFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_SelectFileFailed"), e.Message));
                });

                return new List<StorageFile>();
            }
        }

        /// <summary>
        /// Select single file using the system file picker
        /// </summary>
        /// <returns>The selected file</returns>
        public static async Task<StorageFile> SelectSingleFile(IEnumerable<string> fileTypeFilter = null)
        {
            try
            {
                var fileOpenPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.ComputerFolder
                };
                if (fileTypeFilter == null)
                {
                    fileOpenPicker.FileTypeFilter.Add("*");
                }
                else
                {
                    foreach (var filter in fileTypeFilter)
                    {
                        fileOpenPicker.FileTypeFilter.Add(filter);
                    }
                }

                return await fileOpenPicker.PickSingleFileAsync();
            }
            catch (Exception e)
            {
                UiService.OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_SelectFileFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_SelectFileFailed"), e.Message));
                });

                return null;
            }
        }

        public static async Task<StorageFile> SaveFile(KeyValuePair<string, IList<string>> fileTypes)
        {
            try
            {
                var fileSavePicker = new FileSavePicker()
                {
                    SuggestedStartLocation = PickerLocationId.ComputerFolder,
                };
                fileSavePicker.FileTypeChoices.Add(fileTypes);
                return await fileSavePicker.PickSaveFileAsync();
            }
            catch (Exception e)
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_SelectFileFailed_Title"),
                    string.Format(ResourceService.AppMessages.GetString("AM_SelectFileFailed"), e.Message));
                return null;
            }
        }

        public static string CreateRandomFilePath(string path)
        {
            return Path.Combine(path, Guid.NewGuid().ToString("N"));
        }
    }
}
