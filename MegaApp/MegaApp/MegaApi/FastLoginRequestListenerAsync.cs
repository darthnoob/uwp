using mega;
using MegaApp.Classes;

namespace MegaApp.MegaApi
{
    internal class FastLoginRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_LOGIN)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Login was successful
                        Tcs?.TrySetResult(true);
                        return;
                    case MErrorType.API_ESID: // Bad session ID
                        Tcs?.TrySetException(new BadSessionIdException());
                        Tcs?.TrySetResult(false);
                        return;
                    default: // Unknown result, but not successful
                        Tcs?.TrySetResult(false);
                        return;
                }
            }
        }

        #endregion
    }
}
