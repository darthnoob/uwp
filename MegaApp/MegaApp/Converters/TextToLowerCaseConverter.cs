using System;
using Windows.UI.Xaml.Data;

namespace MegaApp.Converters
{
    public class TextToLowerCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is string)) return string.Empty;

            return ((string)value).ToLower();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
