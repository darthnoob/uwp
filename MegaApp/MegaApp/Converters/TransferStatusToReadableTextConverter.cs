using System;
using Windows.UI.Xaml.Data;
using MegaApp.Enums;
using MegaApp.Services;

namespace MegaApp.Converters
{
    /// <summary>
    /// Converts from a `TransferStatus` value to a readable text
    /// </summary>
    public class TransferStatusToReadableTextConverter : IValueConverter
    {
        /// <summary>
        /// Converts from a `TransferStatus` value to a readable text
        /// </summary>
        /// <param name="value">`TransferStatus` value being passed to the target.</param>
        /// <param name="targetType">The type of the target property, as a type reference.</param>
        /// <param name="parameter">Optional parameter. Not used.</param>
        /// <param name="culture">The language of the conversion.</param>
        /// <returns>String whit the transfer status.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return ResourceService.UiResources.GetString("UI_TransferStatusNotStarted");

            switch ((TransferStatus)value)
            {
                case TransferStatus.NotStarted:
                    return ResourceService.UiResources.GetString("UI_TransferStatusNotStarted");
                case TransferStatus.Preparing:
                    return ResourceService.UiResources.GetString("UI_TransferStatusPreparing");
                case TransferStatus.Queued:
                    return ResourceService.UiResources.GetString("UI_TransferStatusQueued");
                case TransferStatus.Downloading:
                    return ResourceService.UiResources.GetString("UI_TransferStatusDownloading");
                case TransferStatus.Downloaded:
                    return ResourceService.UiResources.GetString("UI_TransferStatusDownloaded");
                case TransferStatus.Uploading:
                    return ResourceService.UiResources.GetString("UI_TransferStatusUploading");
                case TransferStatus.Uploaded:
                    return ResourceService.UiResources.GetString("UI_TransferStatusUploaded");
                case TransferStatus.Pausing:
                    return ResourceService.UiResources.GetString("UI_TransferStatusPausing");
                case TransferStatus.Paused:
                    return ResourceService.UiResources.GetString("UI_TransferStatusPaused");
                case TransferStatus.Canceling:
                    return ResourceService.UiResources.GetString("UI_TransferStatusCanceling");
                case TransferStatus.Canceled:
                    return ResourceService.UiResources.GetString("UI_TransferStatusCanceled");
                case TransferStatus.Error:
                    return ResourceService.UiResources.GetString("UI_TransferStatusError");
                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
