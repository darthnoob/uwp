using mega;

namespace MegaApp.MegaApi
{
    class GetUserAvatarRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_GET_ATTR_USER &&
                request.getParamType() == (int)MUserAttrType.USER_ATTR_AVATAR)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull get user avatar process
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
