using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Windows.Networking.Connectivity;
using MegaApp.Classes;
using MegaApp.Enums;

namespace MegaApp.Services
{
    static class NetworkService
    {
        /// <summary>
        /// Returns if there is an available network connection.
        /// </summary>        
        /// <param name="showMessageDialog">Boolean parameter to indicate if show a message if no Intenert connection</param>
        /// <returns>True if there is an available network connection., False in other case.</returns>
        public static bool IsNetworkAvailable(bool showMessageDialog = false)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                if (connectionProfile?.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
                    return true;
            }

            if (showMessageDialog)
            {
                new CustomMessageDialog(
                    ResourceService.UiResources.GetString("UI_NoInternetConnection"),
                    ResourceService.AppMessages.GetString("AM_NoInternetConnectionMessage"),
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            }

            return false;
        }

        /// <summary>
        /// Gets the network type as string
        /// </summary>
        /// <returns>Network type</returns>
        public static string GetNetworkType()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                var connectionProfile = NetworkInformation.GetInternetConnectionProfile();

                if (connectionProfile?.IsWlanConnectionProfile != null && connectionProfile.IsWlanConnectionProfile)
                    return "WiFi";

                if (connectionProfile?.IsWwanConnectionProfile != null && connectionProfile.IsWwanConnectionProfile)
                {
                    switch(connectionProfile.WwanConnectionProfileDetails?.GetCurrentDataClass())
                    {
                        // Not connected
                        case WwanDataClass.None:
                            return "None";

                        // 2G-equivalent
                        case WwanDataClass.Edge:
                        case WwanDataClass.Gprs:
                            return "Mobile 2G";

                        // 3G-equivalent
                        case WwanDataClass.Cdma1xEvdo:
                        case WwanDataClass.Cdma1xEvdoRevA:
                        case WwanDataClass.Cdma1xEvdoRevB:
                        case WwanDataClass.Cdma1xEvdv:
                        case WwanDataClass.Cdma1xRtt:
                        case WwanDataClass.Cdma3xRtt:
                        case WwanDataClass.CdmaUmb:
                        case WwanDataClass.Umts:
                        case WwanDataClass.Hsdpa:
                        case WwanDataClass.Hsupa:
                            return "Mobile 3G";

                        // 4G-equivalent
                        case WwanDataClass.LteAdvanced:
                            return "Mobile 4G";

                        // Unknown
                        case WwanDataClass.Custom:
                        default:
                            return "Other";
                    }
                }

                return "Ethernet";
            }

            return "None";
        }

        // Code to detect if the IP has changed and refresh all open connections on this case
        public static void CheckChangesIp()
        {
            List<string> ipAddresses;

            // Find the IP of all network devices
            try
            {
                ipAddresses = new List<string>();
                var hostnames = NetworkInformation.GetHostNames();
                foreach (var hn in hostnames)
                {
                    if (hn.IPInformation != null)// && hn.Type == Windows.Networking.HostNameType.Ipv4)
                    {
                        string ipAddress = hn.DisplayName;
                        ipAddresses.Add(ipAddress);
                    }
                }
            }
            catch (ArgumentException) { return; }

            // If no network device is connected, do nothing
            if ((ipAddresses.Count < 1))
            {
                App.IpAddress = null;
                return;
            }

            // If the primary IP has changed
            if (ipAddresses[0] != App.IpAddress)
            {
                SdkService.MegaSdk.reconnect(); // Refresh all open connections
                App.IpAddress = ipAddresses[0]; // Storage the new primary IP address
            }
        }
    }
}
