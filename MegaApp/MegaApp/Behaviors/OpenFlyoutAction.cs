using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using MegaApp.Services;
using Microsoft.Xaml.Interactivity;

namespace MegaApp.Behaviors
{
    /// <summary>
    /// Behavior to open a Flyout dialog
    /// </summary>
    public class OpenFlyoutAction : DependencyObject, IAction
    {
        public DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(OpenFlyoutAction),
            new PropertyMetadata(false));

        /// <summary>
        /// Is the flyout enabled to be shown or not
        /// </summary>
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }
        
        public object Execute(object sender, object parameter)
        {
            // UI interaction SDK action
            SdkService.MegaSdk.retryPendingConnections();

            if (!this.IsEnabled) return null;

            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return null;
            var menuFlyout = FlyoutBase.GetAttachedFlyout((FrameworkElement) sender) as MenuFlyout;
            if (menuFlyout == null)
            {
                FlyoutBase.ShowAttachedFlyout(frameworkElement);
            }
            else
            {
                var rightTap = parameter as RightTappedRoutedEventArgs;
                if (rightTap != null)
                {
                    menuFlyout.ShowAt(frameworkElement, rightTap.GetPosition(frameworkElement));
                }
                else
                {
                    var hold = parameter as HoldingRoutedEventArgs;
                    if(hold != null)
                        menuFlyout.ShowAt(frameworkElement, hold.GetPosition(frameworkElement));
                    else
                        menuFlyout.ShowAt(frameworkElement);
                }
            }

            return null;
        }
    }
}
