using mega;

namespace MegaApp.MegaApi
{
    class ConfirmResetPasswordRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        public string EmailAddress { get; set; }

        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_CONFIRM_RECOVERY_LINK)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successful reset
                        this.EmailAddress = request.getEmail();
                        Tcs?.TrySetResult(true);
                        return;
                    default: // failed reset
                        Tcs?.TrySetResult(false);
                        return;
                }
            }
        }

        #endregion
    }
}
