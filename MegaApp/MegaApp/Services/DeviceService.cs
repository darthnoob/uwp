using System;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Microsoft.Toolkit.Uwp.Helpers;
using MegaApp.Enums;

namespace MegaApp.Services
{
    class DeviceService
    {
        private static EasClientDeviceInformation _deviceInfo;
        /// <summary>
        /// Property that allows the capacity to recover some info of the device
        /// </summary>
        public static EasClientDeviceInformation DeviceInfo
        {
            get
            {
                if (_deviceInfo != null) return _deviceInfo;
                _deviceInfo = new EasClientDeviceInformation();
                return _deviceInfo;
            }
        }

        /// <summary>
        /// Gets the unique device Id
        /// </summary>
        /// <returns>Device Id</returns>
        public static string GetDeviceId()
        {
            HardwareToken token = HardwareIdentification.GetPackageSpecificToken(null);
            IBuffer hardwareId = token.Id;

            HashAlgorithmProvider hasher = HashAlgorithmProvider.OpenAlgorithm("MD5");
            IBuffer hashed = hasher.HashData(hardwareId);

            string hashedString = CryptographicBuffer.EncodeToHexString(hashed);
            return hashedString;
        }

        /// <summary>
        /// Gets the device type
        /// </summary>
        /// <returns>Device type</returns>
        public static DeviceFormFactorType GetDeviceType()
        {
            switch (AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Mobile":
                    return DeviceFormFactorType.Phone;
                case "Windows.Desktop":
                    return UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse
                        ? DeviceFormFactorType.Desktop
                        : DeviceFormFactorType.Tablet;
                case "Windows.Universal":
                    return DeviceFormFactorType.IoT;
                case "Windows.Team":
                    return DeviceFormFactorType.SurfaceHub;
                default:
                    return DeviceFormFactorType.Other;
            }
        }

        /// <summary>
        /// Gets the device family as string
        /// </summary>
        /// <returns>Device family</returns>
        public static string GetDeviceFamily() => SystemInformation.DeviceFamily;

        /// <summary>
        /// Gets the device culture
        /// </summary>
        /// <returns>Device culture</returns>
        public static string GetCulture() => SystemInformation.Culture.Name;

        /// <summary>
        /// Gets the device manufacturer
        /// </summary>
        /// <returns>Device manufacturer</returns>
        public static string GetDeviceManufacturer() => SystemInformation.DeviceManufacturer;

        /// <summary>
        /// Gets the device model
        /// </summary>
        /// <returns>Device model</returns>
        public static string GetDeviceModel() => SystemInformation.DeviceModel;

        /// <summary>
        /// Gets the device hardware version
        /// </summary>
        /// <returns>Device hardware version</returns>
        public static string GetDeviceHardwareVersion() => DeviceInfo.SystemHardwareVersion;

        /// <summary>
        /// Gets the device firmware version
        /// </summary>
        /// <returns>Device firmware version</returns>
        public static string GetDeviceFirmwareVersion() => DeviceInfo.SystemFirmwareVersion;

        /// <summary>
        /// Gets the Operating System version as string
        /// </summary>
        /// <returns>Operating System version</returns>
        public static string GetOperatingSystemVersion()
        {
            var version = SystemInformation.OperatingSystemVersion;
            return string.Format("{0} {1}.{2}.{3}.{4}", SystemInformation.OperatingSystem, 
                version.Major, version.Minor, version.Build, version.Revision);
        }

        /// <summary>
        /// Gets the device free storage space
        /// </summary>
        /// <returns>Device free storage space</returns>
        public static async Task<ulong> GetFreeSpace()
        {
            StorageFolder local = ApplicationData.Current.LocalFolder;
            var retrivedProperties = await local.Properties.RetrievePropertiesAsync(new string[] { "System.FreeSpace" });
            return (ulong)retrivedProperties["System.FreeSpace"];
        }

        /// <summary>
        /// Gets if the current device is a Desktop or Laptop computer.
        /// </summary>
        /// <returns>Result of the query.</returns>
        public static bool IsDesktopDevice() => GetDeviceType() == DeviceFormFactorType.Desktop;

        /// <summary>
        /// Gets if the current device is a Phone.
        /// </summary>
        /// <returns>Result of the query.</returns>
        public static bool IsPhoneDevice() => GetDeviceType() == DeviceFormFactorType.Phone;

        /// <summary>
        /// Gets if the current device is a Surface Hub.
        /// </summary>
        /// <returns>Result of the query.</returns>
        public static bool IsSurfaceHubDevice() => GetDeviceType() == DeviceFormFactorType.SurfaceHub;

        /// <summary>
        /// Gets if the current device is a Tablet.
        /// </summary>
        /// <returns>Result of the query.</returns>
        public static bool IsTabletDevice() => GetDeviceType() == DeviceFormFactorType.Tablet;
    }
}
