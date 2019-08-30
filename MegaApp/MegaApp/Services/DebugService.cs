using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.System;
using Windows.UI.Xaml;
using MegaApp.Extensions;
using MegaApp.ViewModels;

namespace MegaApp.Services
{
    class DebugService
    {
        private static DebugSettingsViewModel _debugSettings;
        public static DebugSettingsViewModel DebugSettings
        {
            get
            {
                if (_debugSettings != null) return _debugSettings;
                _debugSettings = new DebugSettingsViewModel();
                return _debugSettings;
            }
        }

        // Timer to count the actions needed to enable/disable the debug mode.
        private static DispatcherTimer _timerDebugMode;

        // Counter for the actions needed to enable/disable the debug mode.
        private static uint _changeStatusActionCounter = 0;

        /// <summary>
        /// Method that should be called when an action required for 
        /// enable/disable the debug mode is done.
        /// </summary>
        public static void ChangeStatusAction()
        {
            if (_changeStatusActionCounter == 0)
            {
                UiService.OnUiThread(() =>
                {
                    if (_timerDebugMode == null)
                    {
                        _timerDebugMode = new DispatcherTimer();
                        _timerDebugMode.Interval = new TimeSpan(0, 0, 5);
                        _timerDebugMode.Tick += (obj, args) => StopDebugModeTimer();
                    }
                    _timerDebugMode.Start();
                });
            }

            _changeStatusActionCounter++;

            if (_changeStatusActionCounter >= 5)
            {
                StopDebugModeTimer();
                DebugSettings.IsDebugMode = !DebugSettings.IsDebugMode;

                // To avoid change API URL accidentally
                SdkService.ChangeApiUrlActionFinished(true);
            }
        }

        /// <summary>
        /// Stops the timer to detect a status change for the debug mode.
        /// </summary>
        private static void StopDebugModeTimer()
        {
            if (_timerDebugMode != null)
                UiService.OnUiThread(() => _timerDebugMode.Stop());
            _changeStatusActionCounter = 0;
        }

        /// <summary>
        /// Compose an error report email about an exception
        /// </summary>
        /// <param name="e">Exception</param>
        public static async Task ComposeErrorReportEmailAsync(Exception e)
        {
            var emailMessage = new EmailMessage();

            emailMessage.To.Add(new EmailRecipient(ResourceService.AppResources.GetString("AR_DiagnosticsEmailAddress")));
            emailMessage.Subject = string.Format("Error report: [UWP - MegaApp] [{0}]. Hashcode: [{1}]", 
                AppService.GetAppVersion(), e.GetHashCode());

            var body = new StringBuilder();
            body.AppendLine(string.Format("[Type]:[{0}]", e.GetType().Name));
            body.AppendLine(string.Format("[ExceptionMessage]:[{0}]", e.Message));
            body.AppendLine(string.Format("[StackTrace]:[{0}{1}]", Environment.NewLine, e.StackTrace));
            body.AppendLine(string.Format("[InnerException]:[{0}]", e.InnerException != null ? e.InnerException.GetType().Name : "none"));
            body.AppendLine(string.Format("[OcurrenceDate]:[{0}]", DateTime.Now.ToString("R")));
            body.AppendLine(string.Format("[AppInstallDate]:[{0}]", Package.Current.InstalledDate.ToString("R")));
            body.AppendLine(string.Format("[AppVersion]:[{0}]", AppService.GetAppVersion()));
            body.AppendLine(string.Format("[Culture]:[{0}]", DeviceService.GetCulture()));
            body.AppendLine(string.Format("[DeviceManufacturer]:[{0}]", DeviceService.GetDeviceManufacturer()));
            body.AppendLine(string.Format("[DeviceModel]:[{0}]", DeviceService.GetDeviceModel()));
            body.AppendLine(string.Format("[DeviceHardwareVersion]:[{0}]", DeviceService.GetDeviceHardwareVersion()));
            body.AppendLine(string.Format("[DeviceFirmwareVersion]:[{0}]", DeviceService.GetDeviceFirmwareVersion()));
            body.AppendLine(string.Format("[OSVersion]:[{0}]", DeviceService.GetOperatingSystemVersion()));
            body.AppendLine(string.Format("[DeviceType]:[{0}]", DeviceService.GetDeviceFamily()));
            body.AppendLine(string.Format("[NetworkType]:[{0}]", NetworkService.GetNetworkType()));
            body.AppendLine(string.Format("[AppCurrentMemoryUsage]:[{0}]", MemoryManager.AppMemoryUsage.ToStringAndSuffix(2)));
            body.AppendLine(string.Format("[IsoStorageAvailableSpace]:[{0}]", (await DeviceService.GetFreeSpace()).ToStringAndSuffix(2)));

            emailMessage.Body = body.ToString();

            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }
    }
}
