using mega;

namespace MegaApp.MegaApi
{
    internal class GetThumbnailRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_GET_ATTR_FILE &&
                request.getParamType() == (int)MAttrType.ATTR_TYPE_THUMBNAIL)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull get thumbnail process
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
