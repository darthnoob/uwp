using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MegaApp.Classes;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace MegaApp.Services
{
    static class FolderService
    {
        public static bool FolderExists(string path)
        {  
            return Directory.Exists(path);
        }

        public static int GetNumChildFolders(string path)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(path)) return 0;
                return Directory.GetDirectories(path).Length;
            }
            catch (Exception) { return 0; }            
        }

        public static int GetNumChildFiles(string path, bool isOfflineFolder = false)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(path)) return 0;

                string[] childFiles = Directory.GetFiles(path);
                if (childFiles == null) return 0;

                int num = 0;
                if (!isOfflineFolder)
                {
                    num = childFiles.Length;
                }
                else
                {
                    foreach (var filePath in childFiles)
                        if (!FileService.IsPendingTransferFile(Path.GetFileName(filePath))) num++;
                }

                return num;
            }
            catch (Exception) { return 0; }
        }

        public static bool IsEmptyFolder(string path)
        {
            return (Directory.GetDirectories(path).Count() == 0 && Directory.GetFiles(path).Count() == 0) ? true : false;
        }

        public static void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);            
        }
        
        public static void DeleteFolder(string path, bool recursive = false)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                    Directory.Delete(path, recursive);
            }
            catch (Exception e)
            {
                new CustomMessageDialog(
                    ResourceService.AppMessages.GetString("AM_DeleteNodeFailed_Title"),
                    String.Format(ResourceService.AppMessages.GetString("AM_DeleteNodeFailed"), e.Message),
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            }
        }

        public static bool HasIllegalChars(string path)
        {
            var invalidChars = Path.GetInvalidPathChars();
            foreach (var c in invalidChars)
            {
                if (path.Contains(c.ToString())) return true;
            }
            return false;
        }        

        public static void Clear(string path)
        {
            try
            {
                IEnumerable<string> foldersToDelete = Directory.GetDirectories(path);
                if (foldersToDelete != null)
                {
                    foreach (var folder in foldersToDelete)
                    {
                        if (folder != null)
                            Directory.Delete(folder, true);
                    }
                }

                FileService.ClearFiles(Directory.GetFiles(path));
            }
            catch (IOException e)
            {
                new CustomMessageDialog(
                    ResourceService.AppMessages.GetString("AM_DeleteNodeFailed_Title"),
                    String.Format(ResourceService.AppMessages.GetString("AM_DeleteNodeFailed"), e.Message),
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            }
        }
    }    
}
