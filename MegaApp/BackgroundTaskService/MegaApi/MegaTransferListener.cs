using System;
using System.Threading;
using System.Threading.Tasks;
using BackgroundTaskService.Services;
using mega;

namespace BackgroundTaskService.MegaApi
{
    internal class MegaTransferListener : MTransferListenerInterface
    {
        // Helper Timer
        private Timer _timer;

        private TaskCompletionSource<string> _tcs;
        private string _dateSetting = TaskService.ImageDateSetting;

        public async Task<string> ExecuteAsync(Action action, string dateSetting)
        {
            _dateSetting = dateSetting;
            _tcs = new TaskCompletionSource<string>();

            action.Invoke();

            return await _tcs.Task;
        }

        public bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }

        public async void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            _timer?.Dispose();

            try
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK:
                        ulong mtime = api.getNodeByHandle(transfer.getNodeHandle()).getModificationTime();
                        var fileDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(mtime));
                        await SettingsService.SaveSettingToFileAsync(_dateSetting, fileDate.ToLocalTime());
                        _tcs.TrySetResult(null);
                        break;

                    // An error occured. Log and process it.
                    case MErrorType.API_EFAILED:
                    case MErrorType.API_EEXIST:
                    case MErrorType.API_EARGS:
                    case MErrorType.API_EREAD:
                    case MErrorType.API_EWRITE:
                        // Error will be loggend by caller
                        _tcs.TrySetResult(e.getErrorString());
                        break;

                    default:
                        _tcs.TrySetResult(null);
                        break;
                }
            }
            catch (Exception ex)
            {
                _tcs.TrySetResult(ex.Message);
                // Setting could not be saved. Just continue the run
            }
        }

        public void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            _timer = new Timer(state =>
            {
                api.retryPendingConnections();
            }, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            switch (e.getErrorCode())
            {
                case MErrorType.API_EGOINGOVERQUOTA:
                case MErrorType.API_EOVERQUOTA:
                    if (e.getValue() != 0) // TRANSFER OVERQUOTA ERROR
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                            string.Format("Transfer quota exceeded ({0})", e.getErrorCode().ToString()));
                    }
                    else // STORAGE OVERQUOTA ERROR
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                            string.Format("Storage quota exceeded ({0})", e.getErrorCode().ToString()));
                    }
                    break;
            }
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            // Do nothing
        }
    }
}
