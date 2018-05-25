using mega;

namespace MegaApp.MegaApi
{
    class SetUserAttributeRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_SET_ATTR_USER)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull set user attribute process
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

    internal class SetPasswordReminderDialogResultListenerAsync : SetUserAttributeRequestListenerAsync { }
}
