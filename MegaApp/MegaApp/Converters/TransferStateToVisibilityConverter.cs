using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using mega;

namespace MegaApp.Converters
{
    /// <summary>
    /// Class to convert from a TransferState value to a Visibility state (Visible/Collapsed)
    /// </summary>
    public class TransferStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var transferState = (MTransferState)value;
            var command = parameter as string;
            switch (transferState)
            {
                case MTransferState.STATE_COMPLETED:
                    return Visibility.Collapsed;

                case MTransferState.STATE_ACTIVE:
                    switch (command)
                    {
                        case "pause":
                        case "cancel":
                            return Visibility.Visible;
                        case "resume":
                        case "retry":
                        default:
                            return Visibility.Collapsed;
                    }

                case MTransferState.STATE_CANCELLED:
                case MTransferState.STATE_FAILED:
                    switch (command)
                    {
                        case "retry":
                            return Visibility.Visible;
                        case "pause":
                        case "resume":
                        case "cancel":
                        default:
                            return Visibility.Collapsed;
                    }

                case MTransferState.STATE_PAUSED:
                    switch (command)
                    {
                        case "resume":
                        case "cancel":
                            return Visibility.Visible;
                        case "pause":
                        case "retry":
                        default:
                            return Visibility.Collapsed;
                    }

                case MTransferState.STATE_NONE:
                case MTransferState.STATE_QUEUED:
                    switch (command)
                    {
                        case "cancel":
                            return Visibility.Visible;
                        case "pause":
                        case "resume":
                        case "retry":
                        default:
                            return Visibility.Collapsed;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
