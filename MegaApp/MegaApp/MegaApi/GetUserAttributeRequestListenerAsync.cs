using mega;

namespace MegaApp.MegaApi
{
    class GetUserAttributeRequestListenerAsync : BaseRequestListenerAsync<string>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_GET_ATTR_USER)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull get user attribute process
                        Tcs?.TrySetResult(request.getText());
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
