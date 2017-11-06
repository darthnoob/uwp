using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    class ConfirmAccountRequestListenerAsync : BaseRequestListenerAsync<ConfirmAccountResult>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_CONFIRM_ACCOUNT)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Valid and operative confirmation link
                        Tcs?.TrySetResult(ConfirmAccountResult.Success);
                        break;
                    case MErrorType.API_ENOENT: // Wrong password
                        Tcs?.TrySetResult(ConfirmAccountResult.WrongPassword);
                        break;
                    default: // failed confirm
                        Tcs?.TrySetResult(ConfirmAccountResult.Unknown);
                        break;
                }
            }
        }

        #endregion
    }
}
