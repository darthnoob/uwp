using System;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Controls;
using MegaApp.Services;
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
            NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;

            this.CheckNetworkAvailability();
        }

        /// <summary>
        /// Current view-model binded to the datacontext
        /// </summary>
        public T ViewModel { get; }

        protected virtual void OnNetworkStatusChanged(object sender) =>
            this.CheckNetworkAvailability();

        /// <summary>
        /// Checks the availability of the network connection 
        /// and update the GUI using the binded view model
        /// </summary>
        private async void CheckNetworkAvailability()
        {
            await NetworkService.IsNetworkAvailableAsync();
            this.ViewModel.UpdateGUI();
        }
    }
}
