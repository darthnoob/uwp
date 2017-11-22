using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using MegaApp.Enums;

namespace MegaApp.Converters
{
    public class InputStateToColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var inputState = value as InputState? ?? InputState.Normal;
            switch (inputState)
            {
                case InputState.Normal:
                    return new SolidColorBrush(Colors.Transparent);
                case InputState.Warning:
                    return (SolidColorBrush) Application.Current.Resources["MegaRedColorBrush"]; ;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
