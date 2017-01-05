using mega;

namespace MegaApp.MegaApi
{
    internal class RenameNodeRequestListenerAsync : BaseRequestListenerAsync<string>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_RENAME)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull create folder process
                        Tcs?.TrySetResult(request.getName());
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
