using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MegaApp.Network;
using MegaApp.ViewModels;

namespace MegaApp.UserControls
{
    /// <summary>
    /// Typed Page extension that implements a view-model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageEx<T> : PageEx
        where T:BasePageViewModel, new()
    {
        public PageEx()
        {
            // Create the viewmodel and bind it to the page main datacontext
            this.ViewModel = (T)Activator.CreateInstance(typeof(T));
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// Current view-model binded to the datacontext
        /// </summary>
        public T ViewModel { get; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Register for network connection changed events
            NetworkHelper.Instance.NetworkStatusChanged += OnNetworkStatusChanged;

            this.ViewModel.UpdateNetworkStatus();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Unregister for network connection changed events
            NetworkHelper.Instance.NetworkStatusChanged -= OnNetworkStatusChanged;

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Method called when a network status changed event is triggered
        /// </summary>
        /// <param name="sender">Object that sent the event</param>
        /// <param name="args">Event arguments</param>
        protected virtual void OnNetworkStatusChanged(object sender, object args)
        {
            this.ViewModel.UpdateNetworkStatus();
        }
    }

    /// <summary>
    /// Page extension
    /// </summary>
    public class PageEx : Page
    {
        public virtual bool CanGoBack => false;

        public virtual void GoBack() {}
    }
}
