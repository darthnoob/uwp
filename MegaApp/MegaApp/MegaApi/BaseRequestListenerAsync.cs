using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using mega;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    internal abstract class BaseRequestListenerAsync<T>: MRequestListenerInterface
    {
        protected TaskCompletionSource<T> Tcs;

        /// <summary>
        /// Timer to ignore the received API_EAGAIN (-3) during request.
        /// </summary>
        private readonly DispatcherTimer TimerApiEagain;

        /// <summary>
        /// Flag to check if is the first API_EAGAIN (-3) received.
        /// </summary>
        private bool IsFirstApiEagain;

        /// <summary>
        /// Event triggered when servers are busy (receive API_EAGAIN (-3) during more than 10 seconds).
        /// </summary>
        public EventHandler ServerBusy;   
        
        public BaseRequestListenerAsync()
        {
            // Set the timer to trigger the event after 10 seconds
            TimerApiEagain = new DispatcherTimer();
            TimerApiEagain.Tick += TimerApiEagainOnTick;
            TimerApiEagain.Interval = new TimeSpan(0, 0, 10);
        }

        private async void TimerApiEagainOnTick(object sender, object o)
        {
            await UiService.OnUiThread(() => TimerApiEagain.Stop());
            ServerBusy.Invoke(this, EventArgs.Empty);
        }

        public async Task<T> ExecuteAsync(Action action)
        {
            Tcs = new TaskCompletionSource<T>();

            action.Invoke();

            return await Tcs.Task;
        }

        public virtual void onRequestStart(MegaSDK api, MRequest request)
        {
            IsFirstApiEagain = true;
        }

        public virtual void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // Do nothing
        }

        public virtual async void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Starts the timer when receives the first API_EAGAIN (-3)
            if (e.getErrorCode() == MErrorType.API_EAGAIN && IsFirstApiEagain)
            {
                IsFirstApiEagain = false;
                await UiService.OnUiThread(() => TimerApiEagain.Start());
            }
        }

        public virtual async void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            await UiService.OnUiThread(() => TimerApiEagain.Stop());

            if (e.getErrorCode() != MErrorType.API_EBLOCKED) return;
            
            // If the account has been blocked, always logout
            api.logout(new LogOutRequestListener(false));
            // Throw task exception to catch and do nothing on logging out
            Tcs?.TrySetException(new BlockedAccountException());
        }
    }
}
