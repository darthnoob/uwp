using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    internal class QueryPasswordLinkRequestListenerAsync : BaseRequestListenerAsync<RecoverLinkType>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_QUERY_RECOVERY_LINK)
            {
                switch(e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successful reset
                        Tcs?.TrySetResult(request.getFlag() ? RecoverLinkType.Recovery : RecoverLinkType.ParkAccount);
                        break;
                    case MErrorType.API_EEXPIRED:
                        Tcs?.TrySetResult(RecoverLinkType.Expired);
                        break;
                    default: // Default error processing
                        Tcs?.TrySetResult(RecoverLinkType.Unknown);
                        break;
                }
            }
        }

        #endregion
    }
}
