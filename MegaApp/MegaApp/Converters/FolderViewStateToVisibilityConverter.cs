using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.ViewModels;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.Converters
{
    /// <summary>
    /// Class to convert from a folder viewstate value to a Visibility state (Visible/Collapsed)
    /// </summary>
    public class FolderViewStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var folder = value as FolderViewModel;
            if (folder == null) return Visibility.Collapsed;

            var paramString = parameter as string;
            if (string.IsNullOrWhiteSpace(paramString))
                return Visibility.Collapsed;

            var containerType = folder.Type;
            switch (containerType)
            {
                case ContainerType.CloudDrive:
                    switch (paramString)
                    {
                        case "newfolder":
                        case "upload":
                            return folder.ItemCollection != null && !folder.ItemCollection.HasSelectedItems ? 
                                Visibility.Visible : Visibility.Collapsed;

                        case "download":
                        case "copyormove":
                        case "remove":
                            return folder.ItemCollection != null && folder.ItemCollection.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "copy":
                            return folder.IsCopyOrMoveViewModel ? Visibility.Visible : Visibility.Collapsed;

                        case "move":
                            return folder.IsCopyOrMoveViewModel && !CopyOrMoveService.IsMoveAllowed ? 
                                Visibility.Visible : Visibility.Collapsed;

                        default:
                            return Visibility.Collapsed;
                    }

                case ContainerType.CameraUploads:
                    switch (paramString)
                    {
                        case "download":
                        case "copyormove":
                        case "remove":
                            return folder.ItemCollection != null && folder.ItemCollection.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                        default:
                            return Visibility.Collapsed;
                    }

                case ContainerType.RubbishBin:
                    switch (paramString)
                    {
                        case "clean":
                            return folder.ItemCollection != null && !folder.ItemCollection.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "download":
                        case "copyormove":
                        case "remove":
                            return folder.ItemCollection != null && folder.ItemCollection.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                        default:
                            return Visibility.Collapsed;
                    }

                case ContainerType.InShares:
                case ContainerType.ContactInShares:
                    switch (paramString)
                    {
                        case "newfolder":
                        case "upload":
                            return folder.ItemCollection != null && folder.FolderRootNode != null && 
                                !folder.ItemCollection.HasSelectedItems && folder.FolderRootNode.HasReadWritePermissions ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "download":
                        case "copyormove":
                            return folder.ItemCollection != null && folder.ItemCollection.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "open":
                        case "information":
                            return folder.ItemCollection != null && folder.ItemCollection.OnlyOneSelectedItem ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "remove":
                            if (folder.ItemCollection != null && folder.ItemCollection.HasSelectedItems)
                            {
                                if (folder is SharedFoldersListViewModel)
                                {
                                    return folder.ItemCollection.FocusedItem != null && folder.ItemCollection.FocusedItem.HasFullAccessPermissions ?
                                        Visibility.Visible : Visibility.Collapsed;
                                }

                                return folder.FolderRootNode != null && folder.FolderRootNode.HasFullAccessPermissions ?
                                    Visibility.Visible : Visibility.Collapsed;
                            }

                            return Visibility.Collapsed;

                        case "rename":
                            if (folder.ItemCollection != null && folder.ItemCollection.OnlyOneSelectedItem)
                            {
                                if (folder is SharedFoldersListViewModel)
                                {
                                    return folder.ItemCollection.FocusedItem != null && folder.ItemCollection.FocusedItem.HasFullAccessPermissions ?
                                        Visibility.Visible : Visibility.Collapsed;
                                }

                                return folder.FolderRootNode != null && folder.FolderRootNode.HasFullAccessPermissions ?
                                    Visibility.Visible : Visibility.Collapsed;
                            }

                            return Visibility.Collapsed;

                        case "leaveshare":
                            return folder is SharedFoldersListViewModel && 
                                folder.ItemCollection != null && folder.ItemCollection.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "copy":
                            return folder.IsCopyOrMoveViewModel && folder.FolderRootNode != null && folder.FolderRootNode.HasReadWritePermissions ?
                                    Visibility.Visible : Visibility.Collapsed;

                        case "move":
                            return folder.IsCopyOrMoveViewModel && !CopyOrMoveService.IsMoveAllowed &&
                                folder.FolderRootNode != null && folder.FolderRootNode.HasReadWritePermissions ?
                                Visibility.Visible : Visibility.Collapsed;

                        default:
                            return Visibility.Collapsed;
                    }

                case ContainerType.OutShares:
                    switch (paramString)
                    {
                        case "newfolder":
                        case "upload":
                            return folder.ItemCollection != null && folder.ItemCollection.HasSelectedItems ?
                                Visibility.Collapsed : Visibility.Visible;

                        case "download":
                        case "copyormove":
                        case "remove":
                            return folder.ItemCollection != null && folder.ItemCollection.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "open":
                        case "information":
                        case "getlink":
                        case "rename":
                            return folder.ItemCollection != null && folder.ItemCollection.OnlyOneSelectedItem ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "removeshare":
                            return folder is SharedFoldersListViewModel &&
                                folder.ItemCollection != null && folder.ItemCollection.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                        default:
                            return Visibility.Collapsed;
                    }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
