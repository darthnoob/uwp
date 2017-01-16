using mega;
using MegaApp.Services;
using System;

namespace MegaApp.MegaApi
{
    class FetchNodesRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        /// <summary>
        /// Event triggered when start to decrypt nodes.
        /// </summary>
        public EventHandler DecryptNodes;

        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_FETCH_NODES)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull fetch nodes process
                        App.AppInformation.HasFetchedNodes = true;
                        Tcs?.TrySetResult(true);
                        break;
                    default: // Default error processing
                        Tcs?.TrySetResult(false);
                        break;
                }
            }
        }

        public override async void onRequestUpdate(MegaSDK api, MRequest request)
        {
            base.onRequestUpdate(api, request);

            if (request.getType() == MRequestType.TYPE_FETCH_NODES)
            {
                if (request.getTotalBytes() > 0)
                {
                    double progressValue = 100.0 * request.getTransferredBytes() / request.getTotalBytes();
                    if ((progressValue > 99) || (progressValue < 0))
                    {
                        await UiService.OnUiThread(() => DecryptNodes?.Invoke(this, EventArgs.Empty));
                    }
                }
            }
        }

        #endregion
    }
}
