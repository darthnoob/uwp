using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using MegaApp.Enums;

namespace MegaApp.Converters
{
    class InverseFolderContentViewStateToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert from <see cref="FolderContentViewState"/> to a Visibility state.
        /// </summary>
        /// <param name="value">Input <see cref="FolderContentViewState"/> parameter.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">String with the view state name to compare.</param>
        /// <param name="language">Any specific culture information for the current thread.</param>
        /// <returns>Visibility display state.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // If parameters are not valid, visibility will be "collapsed"
            if (value == null || !(value is FolderContentViewState))
                return Visibility.Collapsed;
            if (parameter == null || string.IsNullOrWhiteSpace(parameter as string))
                return Visibility.Collapsed;

            // Convert "parameter" string to a FolderContentViewState
            var paramConvert = (FolderContentViewState)Enum.Parse(typeof(FolderContentViewState), parameter as string);

            return ((FolderContentViewState)value != paramConvert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // Not yet needed in this application
            // Throw exception to check in testing if anything uses this method
            throw new NotImplementedException();
        }
    }
}
