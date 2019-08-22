using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using DnsClient;
using mega;

#if CAMERA_UPLOADS_SERVICE
using BackgroundTaskService.Network;
#else
using MegaApp.Network;
#endif

#if CAMERA_UPLOADS_SERVICE
namespace BackgroundTaskService.Services
#else
namespace MegaApp.Services
#endif
{
    static class NetworkService
    {
        #region Properties

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
        /// Returns if there is an available internet connection.
        /// </summary>        
        /// <param name="showMessageDialog">Boolean parameter to indicate if show a message if no Intenert connection</param>
        /// <returns>True if there is an available network connection., False in other case.</returns>
        public static bool HasInternetAccess(bool showMessageDialog = false)
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "NO INTERNET CONNECTION AVAILABLE.");
                NetworkHelper.LogConnectivityInfo();

#if !CAMERA_UPLOADS_SERVICE
                if (showMessageDialog)
                {
                    var task = DialogService.ShowAlertAsync(
                        ResourceService.UiResources.GetString("UI_NoInternetConnection"),
                        ResourceService.AppMessages.GetString("AM_NoInternetConnectionMessage"));
                }
#endif
            }

            return NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;            
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

        /// <summary>
        /// Code to detect if the network has changed and refresh all open connections on this case
        /// </summary>
        public static void CheckNetworkChange()
        {
            var ipAddressChanged = HasChangedIP();
            var networkNameChanged = HasChangedNetworkName();

            if (ipAddressChanged || networkNameChanged)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Network or IP address changed.");
                SdkService.SetDnsServers();
            }
        }

        /// <summary>
        /// Code to detect if the IP has changed
        /// </summary>
        /// <returns>TRUE if the IP has changed or FALSE in other case.</returns>
        private static bool HasChangedIP()
        {
            try
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile?.NetworkAdapter == null) return false;

                var hostname = NetworkInformation.GetHostNames().SingleOrDefault(hn =>
                    hn?.IPInformation?.NetworkAdapter?.NetworkAdapterId == profile.NetworkAdapter.NetworkAdapterId);

                if (string.IsNullOrWhiteSpace(hostname?.CanonicalName) ||
                    hostname?.CanonicalName == NetworkHelper.Instance.ConnectionInformation.IpAddress)
                    return false;

                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "IP address has changed. New IP: " + hostname?.CanonicalName);
                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error checking a possible IP address change", e);
                return false;
            }
        }

        /// <summary>
        /// Code to detect if the network profile name has changed
        /// </summary>
        /// <returns>TRUE if the has changed or FALSE in other case.</returns>
        private static bool HasChangedNetworkName()
        {
            try
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile == null) return false;

                if (profile.ProfileName == NetworkHelper.Instance.ConnectionInformation.NetworkName)
                    return false;

                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Network name has changed.");
                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error checking a possible network name change", e);
                return false;
            }
        }

        /// <summary>
        /// Gets the system DNS servers IP addresses.
        /// </summary>
        /// <param name="refresh">Indicates if should refresh the previously stored addresses.</param>
        /// <returns>String with the system DNS servers IP addresses separated by commas.</returns>
        public static string GetSystemDnsServers(bool refresh = true)
        {
            try
            {
                if (!refresh && !string.IsNullOrWhiteSpace(SystemDnsServers))
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO, $"System DNS servers (cached): {SystemDnsServers}");
                    return SystemDnsServers;
                }

                if (!HasInternetAccess()) return null;

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
        public static async Task<string> GetMegaDnsServersAsync(bool refresh = true)
        {
            try
            {
                if (!refresh && !string.IsNullOrWhiteSpace(MegaDnsServers))
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO, $"MEGA DNS servers (cached): {MegaDnsServers}");
                    return MegaDnsServers;
                }

                if (!HasInternetAccess()) return null;

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
