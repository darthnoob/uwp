using System;
using System.Globalization;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using MegaApp.Enums;

namespace MegaApp.Converters
{
    public class NodeDisplayModeToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return new SolidColorBrush(Colors.Transparent);

            switch ((NodeDisplayMode)value)
            {
                case NodeDisplayMode.Normal:
                    return new SolidColorBrush(Colors.Transparent);
                case NodeDisplayMode.SelectedNode:
                {
                    var solidColor = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"])
                    {
                        // Specify culture independent number information
                        // Parameter is always with , (resource)
                        // Else the number will be converted by the current culture on the phone
                        Opacity = System.Convert.ToDouble(parameter, new NumberFormatInfo()
                        {
                            NumberDecimalSeparator = ","
                           
                        })
                    };
                    return solidColor;
                }
                
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
