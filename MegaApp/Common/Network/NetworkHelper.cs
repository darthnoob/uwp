using System;
using Windows.Networking.Connectivity;
using mega;

#if CAMERA_UPLOADS_SERVICE
using BackgroundTaskService.Services;
#else
using MegaApp.Services;
#endif

#if CAMERA_UPLOADS_SERVICE
namespace BackgroundTaskService.Network
#else
namespace MegaApp.Network
#endif
{
    /// <summary>
    /// This class exposes functionality of NetworkInformation through a singleton.
    /// </summary>
    public sealed class NetworkHelper
    {
        #region Properties

        /// <summary>
        /// Private singleton field.
        /// </summary>
        private static NetworkHelper _instance;

        /// <summary>
        /// Event raised when the network status changes.
        /// </summary>
        public event EventHandler<object> NetworkStatusChanged;

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static NetworkHelper Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new NetworkHelper();
                _instance.UpdateConnectionInformation();
                return _instance;
            }
        }

        /// <summary>
        /// Gets instance of <see cref="ConnectionInformation"/>.
        /// </summary>
        public ConnectionInformation ConnectionInformation { get; } = new ConnectionInformation();

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkHelper"/> class.
        /// </summary>
        public NetworkHelper()
        {
            ConnectionInformation = new ConnectionInformation();

            NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NetworkHelper"/> class.
        /// </summary>
        ~NetworkHelper()
        {
            NetworkInformation.NetworkStatusChanged -= OnNetworkStatusChanged;
        }

        private void UpdateConnectionInformation()
        {
            lock (ConnectionInformation)
            {
                try
                {
                    ConnectionInformation.UpdateConnectionInformation(NetworkInformation.GetInternetConnectionProfile());
                }
                catch (Exception e)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error updating connection info", e);
                    ConnectionInformation.Reset();
                }
            }
        }

        /// <summary>
        /// Method called when a network status changed event is triggered
        /// </summary>
        /// <param name="sender">Object that sent the event</param>
        private void OnNetworkStatusChanged(object sender)
        {
            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Network status has changed.");
            NetworkService.CheckNetworkChange();
            UpdateConnectionInformation();
            NetworkStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Logs relevant initial connectivity information
        /// </summary>
        public static void LogConnectivityInfo()
        {
            if (_instance == null)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "NETWORK CONNECTIVITY INFO NOT AVAILABLE");
                return;
            }

            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "NETWORK CONNECTIVITY INFO - " +
                "Connection Type: " + _instance.ConnectionInformation.ConnectionType +
                ", Network Type: " + NetworkService.GetNetworkType() +
                ", Connectivity Level: " + _instance.ConnectionInformation.ConnectivityLevel +
                ", IP address: " + _instance.ConnectionInformation.IpAddress);
        }

        #endregion
    }
}
