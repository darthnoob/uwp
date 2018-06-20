using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    internal class LoginRequestListenerAsync : BaseRequestListenerAsync<LoginResult>
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
                        Tcs?.TrySetResult(LoginResult.Success);
                        return;
                    case MErrorType.API_ENOENT: // Email unassociated with a MEGA account or Wrong password
                        Tcs?.TrySetResult(LoginResult.UnassociatedEmailOrWrongPassword);
                        return;
                    case MErrorType.API_ETOOMANY: // Too many failed login attempts. Wait one hour.
                        Tcs?.TrySetResult(LoginResult.TooManyLoginAttempts);
                        return;
                    case MErrorType.API_EINCOMPLETE: // Account not confirmed
                        Tcs?.TrySetResult(LoginResult.AccountNotConfirmed);
                        return;
                    case MErrorType.API_EFAILED: // Wrong MFA pin.
                    case MErrorType.API_EEXPIRED: // MFA pin is being re-used and is being denied to prevent a replay attack
                        Tcs?.TrySetResult(LoginResult.MultiFactorAuth);
                        return;
                    default: // Unknown result, but not successful
                        Tcs?.TrySetResult(LoginResult.Unknown);
                        return;
                }
            }
        }

        #endregion
    }
}
