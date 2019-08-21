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
        /// <summary>
        /// Private singleton field.
        /// </summary>
        private static NetworkHelper _instance;

        /// <summary>
        /// Event raised when the network changes.
        /// </summary>
        public event EventHandler<object> NetworkChanged;

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static NetworkHelper Instance => _instance ?? (_instance = new NetworkHelper());

        /// <summary>
        /// Gets instance of <see cref="ConnectionInformation"/>.
        /// </summary>
        public ConnectionInformation ConnectionInformation { get; } = new ConnectionInformation();

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkHelper"/> class.
        /// </summary>
        public NetworkHelper()
        {
            ConnectionInformation = new ConnectionInformation();

            UpdateConnectionInformation();

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

                    NetworkChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error updating connection info", e);
                    ConnectionInformation.Reset();
                }
            }
        }

        private void OnNetworkStatusChanged(object sender)
        {
            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Network status has changed.");
            NetworkService.CheckNetworkChange();
            UpdateConnectionInformation();
        }
    }
}
