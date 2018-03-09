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

        /// <summary>
        /// Event invocator method called when start to decrypt nodes.
        /// </summary>
        protected virtual void OnDecryptNodes()
        {
            DecryptNodes?.Invoke(this, EventArgs.Empty);
        }

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
                        AccountService.GetUserData();
                        AccountService.GetAccountDetails();
                        AccountService.GetAccountAchievements();
                        Tcs?.TrySetResult(true);
                        break;
                    default: // Default error processing
                        Tcs?.TrySetResult(false);
                        break;
                }
            }
        }

        public override void onRequestUpdate(MegaSDK api, MRequest request)
        {
            base.onRequestUpdate(api, request);

            if (request.getType() == MRequestType.TYPE_FETCH_NODES)
            {
                if (request.getTotalBytes() > 0)
                {
                    double progressValue = 100.0 * request.getTransferredBytes() / request.getTotalBytes();
                    if ((progressValue > 99) || (progressValue < 0))
                    {
                        OnDecryptNodes();
                    }
                }
            }
        }

        #endregion
    }
}
