using System;
using Windows.UI.Xaml.Data;
using mega;
using MegaApp.Services;

namespace MegaApp.Converters
{
    /// <summary>
    /// Converts from a `TransferState` value to a readable text
    /// </summary>
    public class TransferStateToReadableTextConverter : IValueConverter
    {
        /// <summary>
        /// Converts from a `TransferState` value to a readable text
        /// </summary>
        /// <param name="values">Object array with the `TransferType` and `TransferState` values being passed to the target.</param>
        /// <param name="targetType">The type of the target property, as a type reference.</param>
        /// <param name="parameter">Optional parameter. Not used.</param>
        /// <param name="culture">The language of the conversion.</param>
        /// <returns>String whit the transfer state.</returns>
        public object Convert(object values, Type targetType, object parameter, string language)
        {
            if (values == null)
                return ResourceService.UiResources.GetString("UI_TransferStatusNotStarted");

            var typeAndState = values as object[];
            if (typeAndState[0] == null || typeAndState[1] == null)
                return ResourceService.UiResources.GetString("UI_TransferStatusNotStarted");

            var transferType = (MTransferType)typeAndState[0];
            var transferState = (MTransferState)typeAndState[1];

            switch (transferState)
            {
                case MTransferState.STATE_NONE:
                    switch (transferType)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            return ResourceService.UiResources.GetString("UI_TransferStatusNotStarted");
                        case MTransferType.TYPE_UPLOAD:
                            return ResourceService.UiResources.GetString("UI_TransferStatusPreparing");
                        default:
                            throw new ArgumentOutOfRangeException(nameof(transferType), transferType, null);
                    }

                case MTransferState.STATE_QUEUED:
                    return ResourceService.UiResources.GetString("UI_TransferStatusQueued");

                case MTransferState.STATE_ACTIVE:
                    switch(transferType)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            return ResourceService.UiResources.GetString("UI_TransferStatusDownloading");
                        case MTransferType.TYPE_UPLOAD:
                            return ResourceService.UiResources.GetString("UI_TransferStatusUploading");
                        default:
                            throw new ArgumentOutOfRangeException(nameof(transferType), transferType, null);
                    }

                case MTransferState.STATE_PAUSED:
                    return ResourceService.UiResources.GetString("UI_TransferStatusPaused");

                case MTransferState.STATE_RETRYING:
                    return ResourceService.UiResources.GetString("UI_TransferStatusRetrying");

                case MTransferState.STATE_COMPLETING:
                    return ResourceService.UiResources.GetString("UI_TransferStatusCompleting");

                case MTransferState.STATE_COMPLETED:
                    switch (transferType)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            return ResourceService.UiResources.GetString("UI_TransferStatusDownloaded");
                        case MTransferType.TYPE_UPLOAD:
                            return ResourceService.UiResources.GetString("UI_TransferStatusUploaded");
                        default:
                            throw new ArgumentOutOfRangeException(nameof(transferType), transferType, null);
                    }

                case MTransferState.STATE_CANCELLED:
                    return ResourceService.UiResources.GetString("UI_TransferStatusCanceled");

                case MTransferState.STATE_FAILED:
                    return ResourceService.UiResources.GetString("UI_TransferStatusError");

                default:
                    throw new ArgumentOutOfRangeException(nameof(transferState), transferState, null);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
