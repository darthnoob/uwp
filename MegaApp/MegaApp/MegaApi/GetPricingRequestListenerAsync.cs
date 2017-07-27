using mega;

namespace MegaApp.MegaApi
{
    internal class GetPricingRequestListenerAsync : BaseRequestListenerAsync<MPricing>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_GET_PRICING)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull get pricing process
                        Tcs?.TrySetResult(request.getPricing());
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
