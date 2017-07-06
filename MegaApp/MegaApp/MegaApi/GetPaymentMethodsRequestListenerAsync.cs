using mega;

namespace MegaApp.MegaApi
{
    internal class GetPaymentMethodsRequestListenerAsync : BaseRequestListenerAsync<ulong>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_GET_PAYMENT_METHODS)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull get payment methods process
                        Tcs?.TrySetResult(request.getNumber());
                        break;
                    default: // Default error processing
                        Tcs?.TrySetResult(0);
                        break;
                }
            }
        }

        #endregion
    }
}
