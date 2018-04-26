using mega;

namespace MegaApp.MegaApi
{
    class GetUserRequestListenerAsync : BaseRequestListenerAsync<MUser>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;
    
            if (request.getType() == MRequestType.TYPE_GET_USER_DATA)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successful get user avatar process
                        Tcs?.TrySetResult(null);
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
