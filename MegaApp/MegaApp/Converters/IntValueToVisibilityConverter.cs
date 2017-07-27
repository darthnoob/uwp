using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using mega;
using MegaApp.Services;

namespace MegaApp.Converters
{
    public class IntValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;

            try
            {
                return System.Convert.ToInt32(value) == System.Convert.ToInt32(parameter)
                    ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Exception produced at 'IntValueToVisibilityConverter'", e);
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}