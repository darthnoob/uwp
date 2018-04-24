using System;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace MegaApp.Services
{
    internal static class ToastService
    {
        private const string LogoUri = "ms-appx:///Assets/ToastLogo.png";
        private const string WarningUri = "ms-appx:///Assets/ToastWarning.png";

        /// <summary>
        /// Show toast text notification and app logo.
        /// </summary>
        /// <param name="text">Text to show in the notification.</param>
        /// <param name="duration">
        /// How long (in seconds) will the notification be visible in the
        /// action center. Default is 5 seconds.
        /// To keep the system default value set zero or a negative value.
        /// </param>
        public static void ShowTextNotification(string text, int duration = 5) =>
            ShowTextNotification(null, text, duration);

        /// <summary>
        /// Show toast text notification and app logo.
        /// </summary>
        /// <param name="title">Title to show in the notification.</param>
        /// <param name="text">Text to show in the notification.</param>
        /// <param name="duration">
        /// How long (in seconds) will the notification be visible in the
        /// action center. Default is 5 seconds.
        /// To keep the system default value set zero or a negative value.
        /// </param>
        public static void ShowTextNotification(string title, string text, int duration = 5)
        {
            var visual = new ToastVisual
            {
                BindingGeneric = new ToastBindingGeneric
                {
                    Children =
                    {
                        new AdaptiveText { Text = title },
                        new AdaptiveText { Text = text }
                    },

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

            var toast = new ToastNotification(toastContent.GetXml());
            if (duration > 0)
                toast.ExpirationTime = new DateTimeOffset(DateTime.Now.AddSeconds(duration));

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        /// <summary>
        /// Show toast alert notification with a warning icon.
        /// </summary>
        /// <param name="text">Text to show in the notification.</param>
        public static void ShowAlertNotification(string text) => 
            ShowAlertNotification(null, text);

        /// <summary>
        /// Show toast alert notification with a warning icon.
        /// </summary>
        /// <param name="title">Title to show in the notification.</param>
        /// <param name="text">Text to show in the notification.</param>
        public static void ShowAlertNotification(string title, string text)
        {
            var visual = new ToastVisual
            {
                BindingGeneric = new ToastBindingGeneric
                {
                    Children =
                    {
                        new AdaptiveText { Text = title },
                        new AdaptiveText { Text = text }
                    },

                    AppLogoOverride = new ToastGenericAppLogo
                    {
                        Source = WarningUri,
                        HintCrop = ToastGenericAppLogoCrop.Circle
                    }
                }
            };

            var toastContent = new ToastContent
            {
                Visual = visual,
                Audio = new ToastAudio() { Silent = false },
            };

            var toast = new ToastNotification(toastContent.GetXml());

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
