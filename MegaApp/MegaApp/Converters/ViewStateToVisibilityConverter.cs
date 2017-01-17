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
            var node = value as FolderViewModel;
            if (node == null) return Visibility.Collapsed;
            var viewState = node.CurrentViewState;
            var type = node.Type;
            var command = parameter as string;
            switch (viewState)
            {
                case FolderContentViewState.CloudDrive:
                    switch (command)
                    {
                        case "download":
                        case "rename":
                        case "movetorubbish":
                            return Visibility.Visible;
                        default:
                            return Visibility.Collapsed;

                    }
                case FolderContentViewState.CopyOrMoveItem:
                    break;
                case FolderContentViewState.ImportItem:
                    break;
                case FolderContentViewState.MultiSelect:
                    switch (command)
                    {
                        case "download":
                            return Visibility.Visible;
                        case "remove":
                        {
                            switch (type)
                            {
                                case ContainerType.CloudDrive:
                                    return Visibility.Collapsed; ;
                                case ContainerType.RubbishBin:
                                    return Visibility.Visible; ;
                            }
                            return Visibility.Collapsed;;
                        }
                        case "movetorubbish":
                        {
                            switch (type)
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
                        case "download":
                        case "rename":
                        case "remove":
                            return Visibility.Visible;
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
