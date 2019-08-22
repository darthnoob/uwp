using System.Linq;
using Windows.Networking.Connectivity;
using mega;

#if CAMERA_UPLOADS_SERVICE
using BackgroundTaskService.Enums;
using BackgroundTaskService.Services;
#else
using MegaApp.Enums;
using MegaApp.Services;
#endif

#if CAMERA_UPLOADS_SERVICE
namespace BackgroundTaskService.Network
#else
namespace MegaApp.Network
#endif
{
    /// <summary>
    /// This class exposes information about the network connectivity.
    /// </summary>
    public sealed class ConnectionInformation
    {
        /// <summary>
        /// Updates  the current object based on profile passed.
        /// </summary>
        /// <param name="profile">instance of <see cref="ConnectionProfile"/></param>
        public void UpdateConnectionInformation(ConnectionProfile profile)
        {
            if (profile == null)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "There is no connection profile");
                Reset();
                return;
            }

            uint ianaInterfaceType = profile.NetworkAdapter?.IanaInterfaceType ?? 0;

            switch (ianaInterfaceType)
            {
                case 6:
                    ConnectionType = ConnectionType.Ethernet;
                    break;

                case 71:
                    ConnectionType = ConnectionType.WiFi;
                    break;

                case 243:
                case 244:
                    ConnectionType = ConnectionType.Data;
                    break;

                default:
                    ConnectionType = ConnectionType.Unknown;
                    break;
            }

            NetworkName = profile.ProfileName;

            // Update the IP address
            if (profile?.NetworkAdapter != null)
            {
                var hostname = NetworkInformation.GetHostNames().SingleOrDefault(hn => 
                    hn?.IPInformation?.NetworkAdapter?.NetworkAdapterId == profile.NetworkAdapter.NetworkAdapterId);

                if (hostname != null)
                    IpAddress = hostname.CanonicalName;
            }

            ConnectivityLevel = profile.GetNetworkConnectivityLevel();

            switch (ConnectivityLevel)
            {
                case NetworkConnectivityLevel.None:
                case NetworkConnectivityLevel.LocalAccess:
                    IsInternetAvailable = false;
                    break;

                case NetworkConnectivityLevel.InternetAccess:
                case NetworkConnectivityLevel.ConstrainedInternetAccess:
                default:
                    IsInternetAvailable = true;
                    break;
            }

            ConnectionCost = profile.GetConnectionCost();
            SignalStrength = profile.GetSignalBars();

            NetworkHelper.LogConnectivityInfo();
        }

        /// <summary>
        /// Resets the current object to default values.
        /// </summary>
        internal void Reset()
        {
            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Reset connection information.");
            NetworkName = null;
            ConnectionType = ConnectionType.Unknown;
            ConnectivityLevel = NetworkConnectivityLevel.None;
            IsInternetAvailable = false;
            ConnectionCost = null;
            SignalStrength = null;
            IpAddress = null;

            NetworkHelper.LogConnectivityInfo();
        }

        /// <summary>
        /// Value indicating whether if the current internet connection is metered.
        /// </summary>
        public bool IsInternetOnMeteredConnection =>
            ConnectionCost != null && ConnectionCost.NetworkCostType != NetworkCostType.Unrestricted;

        /// <summary>
        /// Value indicating whether internet is available across all connections.
        /// </summary>
        public bool IsInternetAvailable { get; private set; }

        /// <summary>
        /// Connection type for the current Internet Connection Profile.
        /// </summary>
        public ConnectionType ConnectionType { get; private set; }

        /// <summary>
        /// Connectivity level for the current Internet Connection Profile.
        /// </summary>
        public NetworkConnectivityLevel ConnectivityLevel { get; private set; }

        /// <summary>
        /// Connection cost for the current Internet Connection Profile.
        /// </summary>
        public ConnectionCost ConnectionCost { get; private set; }

        /// <summary>
        /// Signal strength for the current Internet Connection Profile.
        /// An integer value within a range of 0-5 that corresponds to the number of signal bars displayed by the UI.
        /// </summary>
        public byte? SignalStrength { get; private set; }

        /// <summary>
        /// Name used to identify the current Internet Connection Profile.
        /// </summary>
        public string NetworkName { get; private set; }

        /// <summary>
        /// IP address of the current Internet Connection Profile.
        /// </summary>
        public string IpAddress { get; private set; }
    }
}
