using mega;

namespace MegaApp.MegaApi
{
    internal class GetPaymentUrlRequestListenerAsync : BaseRequestListenerAsync<string>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_GET_PAYMENT_ID)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull get payment URL process
                        Tcs?.TrySetResult(request.getLink());
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
