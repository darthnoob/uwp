using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    internal class ChangePasswordRequestListenerAsync : BaseRequestListenerAsync<ChangePasswordResult>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_CHANGE_PW)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull change password process
                        Tcs?.TrySetResult(ChangePasswordResult.Success);
                        break;
                    case MErrorType.API_EFAILED: // Wrong MFA pin.
                    case MErrorType.API_EEXPIRED: // MFA pin is being re-used and is being denied to prevent a replay attack
                        Tcs?.TrySetResult(ChangePasswordResult.MultiFactorAuthInvalidCode);
                        return;
                    default: // Default error processing
                        Tcs?.TrySetResult(ChangePasswordResult.Unknown);
                        break;
                }
            }
        }

        #endregion
    }
}
