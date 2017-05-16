using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using mega;

namespace MegaApp.Converters
{
    public class TransferTypeToSymbolConverter : IValueConverter
    {
        /// <summary>
        /// Convert from <see cref="MTransferType"/> to the corresponding symbol.
        /// </summary>
        /// <param name="value">Input <see cref="MTransferType"/> parameter.</param>
        /// <param name="targetType">The type of the target property, as a type reference.</param>
        /// <param name="parameter">Optional parameter. Not used.</param>
        /// <param name="language">Any specific culture information for the current thread.</param>
        /// <returns>Symbol corresponding to the transfer type.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is MTransferType))
                return new Symbol();

            var type = (MTransferType)value;
            switch (type)
            {
                case MTransferType.TYPE_DOWNLOAD:
                    return Symbol.Download;
                case MTransferType.TYPE_UPLOAD:
                    return Symbol.Upload;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // Not yet needed in this application
            // Throw exception to check in testing if anything uses this method
            throw new NotImplementedException();
        }
    }
}
