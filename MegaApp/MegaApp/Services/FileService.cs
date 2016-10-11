using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using MegaApp.Classes;

namespace MegaApp.Services
{
    static class FileService
    {
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

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

        public async static void DeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_DeleteNodeFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_DeleteNodeFailed"), e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
            }
        }

        public static bool HasIllegalChars(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                if (name.Contains(c.ToString())) return true;
            }
            return false;
        }

        public static void ClearFiles(IEnumerable<string> filesToDelete)
        {
            if (filesToDelete == null) return;

            foreach (var file in filesToDelete)
            {
                DeleteFile(file);
            }            
        }

        public static async Task<bool> CopyFile(string sourcePath, string destinationFolderPath, string newFileName = null)
        {
            StorageFile copy = null;

            try 
            { 
                var file = await StorageFile.GetFileFromPathAsync(sourcePath);
                if (file == null) return false;
            
                var folder = await StorageFolder.GetFolderFromPathAsync(destinationFolderPath);
                if (folder == null) return false;

                newFileName = newFileName ?? file.Name;

                copy = await file.CopyAsync(folder, newFileName, NameCollisionOption.GenerateUniqueName); 
            }
            catch (UnauthorizedAccessException) 
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_CopyFileUnauthorizedAccessException_Title"),
                        ResourceService.AppMessages.GetString("AM_CopyFileUnauthorizedAccessException"),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
                return false;
            }
            catch (Exception e) 
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_CopyFileFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_CopyFileFailed"), e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
                return false;
            }

            return copy != null;
        }

        // Move a file. Copies the file and remove the source file if the copy was successful
        public static async Task<bool> MoveFile(string sourcePath, string destinationFolderPath, string newFileName = null)
        {
            if(!await CopyFile(sourcePath, destinationFolderPath, newFileName)) return false;

            DeleteFile(sourcePath);
            
            return true;
        }
       
        public static async Task<bool> OpenFile(string filePath)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(filePath);

                if (file != null)
                    return await Windows.System.Launcher.LaunchFileAsync(file);

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_FileNotFound_Title"),
                        ResourceService.AppMessages.GetString("AM_FileNotFound"),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });                

                return false;
            }
            catch (Exception)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_OpenFileFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_OpenFileFailed"),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
                
                return false;
            }            
        }

        public static async void SelectMultipleFiles()
        {
            try
            {
                var fileOpenPicker = new FileOpenPicker();

                fileOpenPicker.ContinuationData["Operation"] = "SelectedFiles";

                // Use wildcard filter to start FileOpenPicker in location selection screen instead of 
                // photo selection screen
                fileOpenPicker.FileTypeFilter.Add("*");
                fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

                fileOpenPicker.PickMultipleFilesAndContinue();
            }
            catch (Exception e)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_SelectFileFailed_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_SelectFileFailed"), e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
            }
        }

        public static string CreateRandomFilePath(string path)
        {
            return Path.Combine(path, Guid.NewGuid().ToString("N"));
        }
    }
}
