using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MegaApp.Converters
{
    public class IntValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;

            return System.Convert.ToInt32(value) == System.Convert.ToInt32(parameter)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}