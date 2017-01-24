using mega;

namespace MegaApp.MegaApi
{
    internal class PauseTransfersRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_PAUSE_TRANSFERS)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK:
                        Tcs?.TrySetResult(true);
                        break;                    
                    default: 
                        Tcs?.TrySetResult(false);
                        break;
                }
            }
        }

        #endregion
    }
}
