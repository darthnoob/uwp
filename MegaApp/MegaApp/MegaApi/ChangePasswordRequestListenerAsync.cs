using mega;

namespace MegaApp.MegaApi
{
    internal class ChangePasswordRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_CHANGE_PW)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull change password process
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
