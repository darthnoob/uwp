using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using DnsClient;
using mega;

namespace MegaApp.Services
{
    static class NetworkService
    {
        #region Properties

        /// <summary>
        /// State of the network connection
        /// </summary>
        public static bool IsNetworkAvailable;

        /// <summary>
        /// MEGA DNS servers
        /// </summary>
        private static string MegaDnsServers;

        /// <summary>
        /// System DNS servers
        /// </summary>
        private static string SystemDnsServers;

        #endregion

        #region Methods

        /// <summary>
        /// Returns if there is an available network connection.
        /// </summary>        
        /// <param name="showMessageDialog">Boolean parameter to indicate if show a message if no Intenert connection</param>
        /// <returns>True if there is an available network connection., False in other case.</returns>
        public static async Task<bool> IsNetworkAvailableAsync(bool showMessageDialog = false)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                if (connectionProfile?.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
                {
                    IsNetworkAvailable = true;
                    return IsNetworkAvailable;
                }
            }

            if (showMessageDialog)
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.UiResources.GetString("UI_NoInternetConnection"),
                    ResourceService.AppMessages.GetString("AM_NoInternetConnectionMessage"));
            }

            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "No network available.");
            IsNetworkAvailable = false;
            return IsNetworkAvailable;
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

        /// <summary>
        /// Gets the system DNS servers IP addresses.
        /// </summary>
        /// <param name="refresh">Indicates if should refresh the previously stored addresses.</param>
        /// <returns>String with the system DNS servers IP addresses separated by commas.</returns>
        public static async Task<string> GetSystemDnsServers(bool refresh = false)
        {
            try
            {
                if (!refresh && !string.IsNullOrWhiteSpace(SystemDnsServers))
                    return SystemDnsServers;

                if (!await IsNetworkAvailableAsync()) return null;

                string dnsServers = string.Empty;

                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Getting system DNS servers...");

                var lookup = new LookupClient();
                if (lookup != null)
                {
                    foreach (var server in lookup.NameServers)
                    {
                        if (server == null) continue;

                        if (dnsServers.Length > 0)
                            dnsServers = string.Concat(dnsServers, ",");

                        dnsServers = string.Concat(dnsServers, server.Endpoint.Address.ToString());
                    }
                }

                if (string.IsNullOrWhiteSpace(dnsServers))
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "No system DNS servers.");
                    return dnsServers;
                }

                LogService.Log(MLogLevel.LOG_LEVEL_INFO, $"System DNS servers: {dnsServers}");
                SystemDnsServers = dnsServers;
                return SystemDnsServers;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error getting System DNS servers.", e);
                return null;
            }
        }

        /// <summary>
        /// Gets the MEGA DNS servers IP addresses.
        /// </summary>
        /// <param name="refresh">Indicates if should refresh the previously stored addresses.</param>
        /// <returns>String with the MEGA DNS servers IP addresses separated by commas.</returns>
        public static async Task<string> GetMegaDnsServersAsync(bool refresh = false)
        {
            try
            {
                if (!refresh && !string.IsNullOrWhiteSpace(MegaDnsServers))
                    return MegaDnsServers;

                if (!await IsNetworkAvailableAsync()) return null;

                string dnsServers = string.Empty;

                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Getting MEGA DNS servers...");

                IPHostEntry host = await Dns.GetHostEntryAsync("ns.mega.co.nz");
                if (host != null)
                {
                    foreach (IPAddress address in host.AddressList)
                    {
                        if (dnsServers.Length > 0)
                            dnsServers = string.Concat(dnsServers, ",");

                        dnsServers = string.Concat(dnsServers, address.ToString());
                    }
                }

                if (string.IsNullOrWhiteSpace(dnsServers))
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "No MEGA DNS servers.");
                    return dnsServers;
                }

                LogService.Log(MLogLevel.LOG_LEVEL_INFO, $"MEGA DNS servers: {dnsServers}");
                MegaDnsServers = dnsServers;
                return MegaDnsServers;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error getting MEGA DNS servers.", e);
                return null;
            }
        }

        #endregion
    }
}
