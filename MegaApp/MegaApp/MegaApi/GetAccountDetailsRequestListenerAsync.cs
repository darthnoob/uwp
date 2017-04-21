using mega;

namespace MegaApp.MegaApi
{
    internal class GetAccountDetailsRequestListenerAsync : BaseRequestListenerAsync<MAccountDetails>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_ACCOUNT_DETAILS)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull get account details process
                        Tcs?.TrySetResult(request.getMAccountDetails());
                        break;
                    default: // Default error processing
                        Tcs?.TrySetResult(null);
                        break;
                }
            }
        }

        #endregion
    }
}
