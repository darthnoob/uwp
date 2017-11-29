using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.UI.Animations;

namespace MegaApp.Extensions
{
    public class IsVisibleAnimation: DependencyObject
    {
        // Attention! Set initial state if bound property does not change value

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.RegisterAttached(
                "IsVisible",
                typeof(bool),
                typeof(IsVisibleAnimation),
                new PropertyMetadata(false, PropertyChangedCallback)
            );

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs dpc)
        {
            var element = d as UIElement;
            if (element == null) return;
            if ((bool)dpc.NewValue)
            {
                element.Visibility = Visibility.Visible;
                if (element.Opacity >= 1.0) return;
                element.Fade(1.0f, 250.0).Start();
                return;
            }
            element.Fade(0.0f, 250.0).Start();
            element.Visibility = Visibility.Collapsed;
        }

        public static void SetIsVisible(UIElement element, bool value)
        {
            element.SetValue(IsVisibleProperty, value);
           
        }
        public static bool GetIsVisible(UIElement element)
        {
            return (bool)element.GetValue(IsVisibleProperty);
        }
    }
}
