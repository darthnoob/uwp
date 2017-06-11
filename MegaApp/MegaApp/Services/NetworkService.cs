using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Windows.Networking.Connectivity;
using MegaApp.Classes;

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
