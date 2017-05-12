using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using MegaApp.Enums;
using MegaApp.ViewModels;

namespace MegaApp.Converters
{
    /// <summary>
    /// Class to convert from a viewstate  value to a Visibility state (Visible/Collapsed)
    /// </summary>
    public class ViewStateToVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var node = value as NodeViewModel;
            if (node == null) return Visibility.Collapsed;

            var parentFolder = node.Parent;
            if (parentFolder == null) return Visibility.Collapsed;

            var viewState = parentFolder.CurrentViewState;
            var parentFolderType = parentFolder.Type;
            var command = parameter as string;
            switch (viewState)
            {
                case FolderContentViewState.CloudDrive:
                    switch (command)
                    {
                        case "preview":
                            return (node.IsImage) ? 
                                Visibility.Visible : Visibility.Collapsed;
                        case "download":
                        case "copyormove":
                        case "movetorubbish":
                            return Visibility.Visible;
                        case "getlink":
                        case "rename":
                            return (parentFolder.ItemCollection.MoreThanOneSelected) ? 
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
                            return Visibility.Visible;
                        case "remove":
                        {
                            switch (parentFolderType)
                            {
                                case ContainerType.CloudDrive:
                                    return Visibility.Collapsed;
                                case ContainerType.RubbishBin:
                                    return Visibility.Visible;
                            }
                            return Visibility.Collapsed;;
                        }
                        case "movetorubbish":
                        {
                            switch (parentFolderType)
                            {
                                case ContainerType.CloudDrive:
                                    return Visibility.Visible;
                                case ContainerType.RubbishBin:
                                    return Visibility.Collapsed;
                            }
                            return Visibility.Collapsed; ;
                        }
                        default:
                            return Visibility.Collapsed;

                    };
                case FolderContentViewState.RubbishBin:
                    switch (command)
                    {
                        case "preview":
                            return (node.IsImage) ?
                                Visibility.Visible : Visibility.Collapsed;
                        case "download":
                        case "copyormove":
                        case "remove":
                            return Visibility.Visible;
                        case "rename":
                            return (parentFolder.ItemCollection.MoreThanOneSelected) ?
                                Visibility.Collapsed : Visibility.Visible;
                        default:
                            return Visibility.Collapsed;
                    }; 
                case FolderContentViewState.InShares:
                    break;
                case FolderContentViewState.OutShares:
                    break;
                case FolderContentViewState.ContactInShares:
                    break;
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
            throw new NotImplementedException();
        }
    }
}
