using System;
using mega;
using MegaApp.Enums;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    /// <summary>
    /// Request to fetch nodes at one account (login) or a folder link (login to folder).
    /// </summary>
    class FetchNodesRequestListenerAsync : BaseRequestListenerAsync<FetchNodesResult>
    {
        /// <summary>
        /// Variable to indicate if has already shown the decryption alert (folder links).
        /// </summary>
        public bool DecryptionAlert { get; set; }

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
                        //If getFlag() returns true, the folder link key is invalid.
                        if (request.getFlag())
                        {
                            Tcs?.TrySetResult(DecryptionAlert ?
                                FetchNodesResult.InvalidDecryptionKey : // No valid decryption key
                                FetchNodesResult.InvalidHandleOrDecryptionKey); // Handle length or Key length no valid
                        }

                        AccountService.GetUserData();
                        AccountService.GetAccountDetails();
                        AccountService.GetAccountAchievements();
                        Tcs?.TrySetResult(FetchNodesResult.Success);
                        break;

                    case MErrorType.API_ETOOMANY: // Taken down link and the link owner's account is blocked
                        Tcs?.TrySetResult(FetchNodesResult.AssociatedUserAccountTerminated);
                        return;

                    case MErrorType.API_ENOENT: // Link not exists or has been deleted by user
                    case MErrorType.API_EBLOCKED: // Taken down link
                        Tcs?.TrySetResult(FetchNodesResult.UnavailableLink);
                        break;

                    default: // Default error processing
                        Tcs?.TrySetResult(FetchNodesResult.Unknown);
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
