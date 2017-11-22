using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    internal class QuerySignUpLinkRequestListenerAsync : BaseRequestListenerAsync<SignUpLinkType>
    {
        public string EmailAddress { get; set; }

        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_QUERY_SIGNUP_LINK)
            {
                switch(e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successful reset
                        this.EmailAddress = request.getEmail();
                        Tcs?.TrySetResult(SignUpLinkType.Valid);
                        break;
                    case MErrorType.API_EEXPIRED:
                        Tcs?.TrySetResult(SignUpLinkType.Expired);
                        break;
                    case MErrorType.API_ENOENT: // Already confirmed account
                        Tcs?.TrySetResult(SignUpLinkType.AlreadyConfirmed);
                        break;
                    case MErrorType.API_EINCOMPLETE: // Incomplete confirmation link
                        Tcs?.TrySetResult(SignUpLinkType.Invalid);
                        break;
                    default: // Default error processing
                        Tcs?.TrySetResult(SignUpLinkType.Unknown);
                        break;
                }
            }
        }

        #endregion
    }
}
