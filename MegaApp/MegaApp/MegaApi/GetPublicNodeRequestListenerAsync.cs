using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    internal class GetPublicNodeRequestListenerAsync : BaseRequestListenerAsync<GetPublicNodeResult>
    {
        /// <summary>
        /// Variable to indicate if has already shown the decryption alert
        /// </summary>
        public bool DecryptionAlert { get; set; }

        /// <summary>
        /// The node obtained after a successful request
        /// </summary>
        public MNode PublicNode { get; set; }

        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_GET_PUBLIC_NODE)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK:
                        //If getFlag() returns true, the file link key is invalid.
                        if (request.getFlag())
                        {
                            if (DecryptionAlert) // No valid decryption key
                                Tcs?.TrySetResult(GetPublicNodeResult.InvalidDecryptionKey);
                            else // Handle length or Key length no valid
                                Tcs?.TrySetResult(GetPublicNodeResult.InvalidHandleOrDecryptionKey);
                        }

                        // Get public node was successfull
                        this.PublicNode = request.getPublicMegaNode();
                        Tcs?.TrySetResult(GetPublicNodeResult.Success);
                        return;

                    case MErrorType.API_EARGS:
                        if (DecryptionAlert) // No valid decryption key
                            Tcs?.TrySetResult(GetPublicNodeResult.InvalidDecryptionKey);
                        else // Handle length or Key length no valid
                            Tcs?.TrySetResult(GetPublicNodeResult.InvalidHandleOrDecryptionKey);
                        return;

                    case MErrorType.API_ETOOMANY: // Taken down link and the link owner's account is blocked
                        Tcs?.TrySetResult(GetPublicNodeResult.AssociatedUserAccountTerminated);
                        return;

                    case MErrorType.API_ENOENT: // Link not exists or has been deleted by user
                    case MErrorType.API_EBLOCKED: // Taken down link
                        Tcs?.TrySetResult(GetPublicNodeResult.UnavailableLink);
                        break;

                    case MErrorType.API_EINCOMPLETE: // Link has not decryption key
                        Tcs?.TrySetResult(GetPublicNodeResult.NoDecryptionKey);
                        return;

                    default: // Unknown result, but not successful
                        Tcs?.TrySetResult(GetPublicNodeResult.Unknown);
                        return;
                }
            }
        }

        #endregion
    }
}
