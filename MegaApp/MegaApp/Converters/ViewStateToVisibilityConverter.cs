using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using MegaApp.Enums;
using MegaApp.ViewModels;

namespace MegaApp.Converters
{
    /// <summary>
    /// Class to convert from a viewstate value to a Visibility state (Visible/Collapsed)
    /// </summary>
    public class ViewStateToVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var node = value as NodeViewModel;
            var parentFolder = node?.Parent;
            if (parentFolder == null) return Visibility.Collapsed;

            var viewState = parentFolder.CurrentViewState;
            var command = parameter as string;
            switch (viewState)
            {
                case FolderContentViewState.CloudDrive:
                case FolderContentViewState.CameraUploads:
                    switch (command)
                    {
                        case "preview":
                            return node.IsImage ? Visibility.Visible : Visibility.Collapsed;
                        case "viewdetails":
                        case "download":
                        case "copyormove":
                        case "remove":
                            return Visibility.Visible;
                        case "getlink":
                        case "rename":
                            return parentFolder.ItemCollection.MoreThanOneSelected ? 
                                Visibility.Collapsed : Visibility.Visible;
                        default:
                            return Visibility.Collapsed;
                    }
                case FolderContentViewState.CopyOrMove:
                    break;
                case FolderContentViewState.Import:
                    break;
                case FolderContentViewState.MultiSelect:
                    switch (command)
                    {
                        case "download":
                        case "copyormove":
                        case "remove":
                            return Visibility.Visible;
                        default:
                            return Visibility.Collapsed;

                    };
                case FolderContentViewState.RubbishBin:
                    switch (command)
                    {
                        case "preview":
                            return node.IsImage ? Visibility.Visible : Visibility.Collapsed;
                        case "viewdetails":
                        case "download":
                        case "copyormove":
                        case "remove":
                            return Visibility.Visible;
                        case "rename":
                            return parentFolder.ItemCollection.MoreThanOneSelected ?
                                Visibility.Collapsed : Visibility.Visible;
                        default:
                            return Visibility.Collapsed;
                    }; 
                case FolderContentViewState.InShares:
                case FolderContentViewState.ContactInShares:
                    switch (command)
                    {
                        case "download":
                            return Visibility.Visible;
                        case "remove":
                            return parentFolder.FolderRootNode.HasFullAccessPermissions ?
                                Visibility.Visible : Visibility.Collapsed;
                        case "rename":
                            return parentFolder.ItemCollection.MoreThanOneSelected || !parentFolder.FolderRootNode.HasFullAccessPermissions ?
                                Visibility.Collapsed: Visibility.Visible;
                        default:
                            return Visibility.Collapsed;
                    }
                case FolderContentViewState.OutShares:
                    switch (command)
                    {
                        case "download":
                        case "remove":
                            return Visibility.Visible;
                        case "getlink":
                        case "rename":
                            return parentFolder.ItemCollection.OnlyOneSelectedItem ?
                                Visibility.Visible : Visibility.Collapsed;
                        default:
                            return Visibility.Collapsed;
                    }
                case FolderContentViewState.SavedForOffline:
                    break;
                case FolderContentViewState.FolderLink:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
