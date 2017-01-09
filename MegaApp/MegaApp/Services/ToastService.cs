using System;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace MegaApp.Services
{
    internal static class ToastService
    {
        private const string LogoUri = "ms-appx:///Assets/StoreLogo.png";

        /// <summary>
        /// Show toast text notification and app logo.
        /// </summary>
        /// <param name="text">Text to show in the notification.</param>
        /// <param name="duration">How long will the notification be visible in the action center. 
        /// In seconds. Default is 5 seconds.</param>
        public static void ShowText(string text, int duration = 5)
        {
            var visual = new ToastVisual
            {
                BindingGeneric = new ToastBindingGeneric
                {
                    Children = { new AdaptiveText { Text = text } },
                    AppLogoOverride = new ToastGenericAppLogo
                    {
                        Source = LogoUri,
                        HintCrop = ToastGenericAppLogoCrop.Circle
                    }
                }
            };

            var toastContent = new ToastContent
            {
                Visual = visual,
                Audio = new ToastAudio() { Silent = true },
            };


            var toast = new ToastNotification(toastContent.GetXml())
            {
                ExpirationTime = new DateTimeOffset(DateTime.Now.AddSeconds(duration))
            };

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
