using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using MegaApp.Enums;

namespace MegaApp.Converters
{
    public class ContactsContentTypeToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert from <see cref="ContactsContentType"/> to a Visibility state.
        /// </summary>
        /// <param name="value">Input <see cref="ContactsContentType"/> parameter.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">String with the view state name to compare.</param>
        /// <param name="language">Any specific culture information for the current thread.</param>
        /// <returns>Visibility display state.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var paramString = parameter as string;

            // If parameters are not valid, visibility will be "collapsed"
            if (!(value is ContactsContentType))
                return Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(paramString))
                return Visibility.Collapsed;

            // Convert "parameter" string to a ContactsViewState
            var paramConvert = (ContactsContentType)Enum.Parse(typeof(ContactsContentType), parameter as string);

            return ((ContactsContentType)value == paramConvert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
