using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    class ConfirmChangeEmailRequestListenerAsync : BaseRequestListenerAsync<ConfirmChangeEmailResult>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_CONFIRM_CHANGE_EMAIL_LINK)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull change email process
                        Tcs?.TrySetResult(ConfirmChangeEmailResult.Success);
                        return;
                    case MErrorType.API_EARGS: // User logged into an incorrect account
                        Tcs?.TrySetResult(ConfirmChangeEmailResult.WrongAccount);
                        return;
                    case MErrorType.API_ENOENT: // Wrong password
                        Tcs?.TrySetResult(ConfirmChangeEmailResult.WrongPassword);
                        return;
                    case MErrorType.API_EACCESS: // No user is logged in
                        Tcs?.TrySetResult(ConfirmChangeEmailResult.UserNotLoggedIn);
                        return;
                    default: // Unknown result, but not successful
                        Tcs?.TrySetResult(ConfirmChangeEmailResult.Unknown);
                        return;
                }
            }
        }

        #endregion
    }
}
