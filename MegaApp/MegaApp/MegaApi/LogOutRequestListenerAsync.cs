using mega;

namespace MegaApp.MegaApi
{
    internal class LogOutRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_LOGOUT)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK:     // Successfull log out from folder
                    case MErrorType.API_ESID:   // Under certain circumstances, this request might return this error code.
                                                // It should not be taken as an error, since the reason is that the logout
                                                // action has been notified before the reception of the logout response itself.
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
