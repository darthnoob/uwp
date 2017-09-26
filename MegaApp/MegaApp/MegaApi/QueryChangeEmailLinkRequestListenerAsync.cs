using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    class QueryChangeEmailLinkRequestListenerAsync : BaseRequestListenerAsync<QueryChangeEmailLinkResult>
    {
        #region Properties

        public string Email;

        #endregion

        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_QUERY_RECOVERY_LINK)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull change email link query
                        this.Email = request.getEmail();
                        Tcs?.TrySetResult(QueryChangeEmailLinkResult.Success);
                        return;
                    case MErrorType.API_ENOENT: // Invalid link
                        Tcs?.TrySetResult(QueryChangeEmailLinkResult.InvalidLink);
                        return;
                    case MErrorType.API_EACCESS: // No user is logged in
                        Tcs?.TrySetResult(QueryChangeEmailLinkResult.UserNotLoggedIn);
                        return;
                    default: // Unknown result, but not successful
                        Tcs?.TrySetResult(QueryChangeEmailLinkResult.Unknown);
                        return;
                }
            }
        }

        #endregion
    }
}
