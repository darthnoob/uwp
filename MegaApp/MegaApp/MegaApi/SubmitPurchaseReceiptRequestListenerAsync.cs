using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    internal class SubmitPurchaseReceiptRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_CREATE_ACCOUNT)
            {
                switch(e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successful create account process
                        Tcs?.TrySetResult(true);
                        break;
                    default: // Default error processing
                        Tcs?.TrySetResult(false);
                        break;
                }
            }
        }

        #endregion
    }
}
