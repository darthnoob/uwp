using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using MegaApp.Enums;
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
                case ContainerType.InShares:
                case ContainerType.ContactInShares:
                    switch (paramString)
                    {
                        case "newfolder":
                        case "upload":
                            return !(bool)folder.ItemCollection?.HasSelectedItems && (bool)folder.FolderRootNode?.HasReadWritePermissions ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "download":
                            return (bool)folder.ItemCollection?.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "open":
                        case "information":
                            return (bool)folder.ItemCollection?.OnlyOneSelectedItem ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "remove":
                            if ((bool)folder.ItemCollection?.HasSelectedItems)
                            {
                                if (folder is SharedFoldersListViewModel)
                                {
                                    return (bool)folder.ItemCollection?.FocusedItem?.HasFullAccessPermissions ?
                                        Visibility.Visible : Visibility.Collapsed;
                                }

                                return (bool)folder.FolderRootNode?.HasFullAccessPermissions ?
                                    Visibility.Visible : Visibility.Collapsed;
                            }

                            return Visibility.Collapsed;

                        case "rename":
                            if ((bool)folder.ItemCollection?.OnlyOneSelectedItem)
                            {
                                if (folder is SharedFoldersListViewModel)
                                {
                                    return (bool)folder.ItemCollection?.FocusedItem?.HasFullAccessPermissions ?
                                        Visibility.Visible : Visibility.Collapsed;
                                }

                                return (bool)folder.FolderRootNode?.HasFullAccessPermissions ?
                                    Visibility.Visible : Visibility.Collapsed;
                            }

                            return Visibility.Collapsed;

                        case "leaveshare":
                            return folder is SharedFoldersListViewModel && (bool)folder.ItemCollection?.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                                default:
                            return Visibility.Collapsed;
                    }

                case ContainerType.OutShares:
                    switch (paramString)
                    {
                        case "newfolder":
                        case "upload":
                            return !(bool)folder.ItemCollection?.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "download":
                        case "remove":
                            return (bool)folder.ItemCollection?.HasSelectedItems ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "open":
                        case "information":
                        case "getlink":
                        case "rename":
                            return (bool)folder.ItemCollection?.OnlyOneSelectedItem ?
                                Visibility.Visible : Visibility.Collapsed;

                        case "removeshare":
                            return folder is SharedFoldersListViewModel && (bool)folder.ItemCollection?.HasSelectedItems ?
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
