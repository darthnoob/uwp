using System;
using System.Threading.Tasks;
using BackgroundTaskService.Services;
using mega;

namespace BackgroundTaskService.MegaApi
{
    internal class MegaRequestListener<T>: MRequestListenerInterface
    {
        private TaskCompletionSource<T> _tcs;

        public async Task<T> ExecuteAsync(Action action)
        {
            _tcs = new TaskCompletionSource<T>();

            action.Invoke();

            return await _tcs.Task;
        }

        public void onRequestStart(MegaSDK api, MRequest request)
        {
            // Start the request
        }

        public void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            switch (e.getErrorCode())
            {
                case MErrorType.API_OK:
                {
                    if (typeof(T) == typeof(bool)) _tcs.TrySetResult((T)Convert.ChangeType(true, typeof(T)));
                    break;
                }
                default:
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error Request Listener: " +  e.getErrorString());
                    if (typeof(T) == typeof(bool)) _tcs.TrySetResult((T)Convert.ChangeType(false, typeof(T)));
                    break;
                }
            }
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // Update the request
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Do nothing on temp error
        }
    }
}
