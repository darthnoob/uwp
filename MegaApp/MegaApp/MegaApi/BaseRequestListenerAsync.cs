using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.Views;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    internal abstract class BaseRequestListenerAsync<T>: MRequestListenerInterface
    {
        protected TaskCompletionSource<T> Tcs;

        /// <summary>
        /// Timer to ignore the received API_EAGAIN (-3) during request.
        /// </summary>
        private DispatcherTimer TimerApiEagain;

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
            UiService.OnUiThread(() =>
            {
                TimerApiEagain = new DispatcherTimer();
                TimerApiEagain.Tick += TimerApiEagainOnTick;
                TimerApiEagain.Interval = new TimeSpan(0, 0, 10);
            });
        }

        private void TimerApiEagainOnTick(object sender, object o)
        {
            UiService.OnUiThread(() => TimerApiEagain?.Stop());
            ServerBusy?.Invoke(this, EventArgs.Empty);
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

        public virtual void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Starts the timer when receives the first API_EAGAIN (-3)
            if (e.getErrorCode() == MErrorType.API_EAGAIN && IsFirstApiEagain)
            {
                IsFirstApiEagain = false;
                UiService.OnUiThread(() => TimerApiEagain?.Start());
            }
        }

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            UiService.OnUiThread(() => TimerApiEagain?.Stop());

            switch(e.getErrorCode())
            {
                case MErrorType.API_EBLOCKED: // If the account has been blocked
                    api.logout(new LogOutRequestListener(false));

                    UiService.OnUiThread(() =>
                    {
                        NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true,
                            NavigationObject.Create(typeof(MainViewModel), NavigationActionType.API_EBLOCKED));
                    });

                    // Throw task exception to catch and do nothing on logging out
                    Tcs?.TrySetException(new BlockedAccountException());
                    break;

                case MErrorType.API_EGOINGOVERQUOTA: // Not enough quota
                case MErrorType.API_EOVERQUOTA: // Storage overquota error
                    UiService.OnUiThread(DialogService.ShowOverquotaAlert);

                    // Stop all upload transfers
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                        string.Format("Storage quota exceeded ({0}) - Canceling uploads", e.getErrorCode().ToString()));
                    api.cancelTransfers((int)MTransferType.TYPE_UPLOAD);

                    // Disable the "Camera Uploads" service if is enabled
                    if (TaskService.IsBackGroundTaskActive(TaskService.CameraUploadTaskEntryPoint, TaskService.CameraUploadTaskName))
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                            string.Format("Storage quota exceeded ({0}) - Disabling CAMERA UPLOADS service", e.getErrorCode().ToString()));
                        TaskService.UnregisterBackgroundTask(TaskService.CameraUploadTaskEntryPoint, TaskService.CameraUploadTaskName);
                    }
                    break;
            }
        }
    }
}
