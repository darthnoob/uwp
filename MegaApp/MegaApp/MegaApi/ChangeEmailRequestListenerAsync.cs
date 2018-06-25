using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    class ChangeEmailRequestListenerAsync : BaseRequestListenerAsync<ChangeEmailResult>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_GET_CHANGE_EMAIL_LINK)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull get change email link process
                        Tcs?.TrySetResult(ChangeEmailResult.Success);
                        return;
                    case MErrorType.API_EEXIST: // Change email already requested
                        Tcs?.TrySetResult(ChangeEmailResult.AlreadyRequested);
                        return;
                    case MErrorType.API_EACCESS: // No user is logged in
                        Tcs?.TrySetResult(ChangeEmailResult.UserNotLoggedIn);
                        return;
                    case MErrorType.API_EFAILED: // Wrong MFA pin.
                    case MErrorType.API_EEXPIRED: // MFA pin is being re-used and is being denied to prevent a replay attack
                        Tcs?.TrySetResult(ChangeEmailResult.MultiFactorAuth);
                        return;
                    default: // Unknown result, but not successful
                        Tcs?.TrySetResult(ChangeEmailResult.Unknown);
                        return;
                }
            }
        }

        #endregion
    }
}
