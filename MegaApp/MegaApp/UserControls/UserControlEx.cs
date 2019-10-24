using System;
using Windows.UI.Xaml.Controls;
using MegaApp.Network;
using MegaApp.ViewModels;

namespace MegaApp.UserControls
{
    /// <summary>
    /// Typed UserControl extension that implements a view-model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UserControlEx<T> : UserControl
        where T:BaseUiViewModel, new()
    {
        public UserControlEx()
        {
            // Create the viewmodel and bind it to the view main datacontext
            this.ViewModel = (T)Activator.CreateInstance(typeof(T));
            this.DataContext = this.ViewModel;

            // Register for network connection changed events
            NetworkHelper.Instance.NetworkStatusChanged += OnNetworkStatusChanged;

            this.CheckNetworkAvailability();
        }

        /// <summary>
        /// Current view-model binded to the datacontext
        /// </summary>
        public T ViewModel { get; }

        /// <summary>
        /// Method called when a network status changed event is triggered
        /// </summary>
        /// <param name="sender">Object that sent the event</param>
        /// <param name="args">Event arguments</param>
        protected virtual void OnNetworkStatusChanged(object sender, object args) =>
            this.CheckNetworkAvailability();

        /// <summary>
        /// Checks the availability of the network connection 
        /// and update the GUI using the binded view model
        /// </summary>
        private void CheckNetworkAvailability()
        {
            this.ViewModel.UpdateNetworkStatus();
        }
    }
}
