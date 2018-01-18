using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    internal class LoginToFolderRequestListenerAsync : BaseRequestListenerAsync<LoginToFolderResult>
    {
        /// <summary>
        /// Variable to indicate if has already shown the decryption alert
        /// </summary>
        public bool DecryptionAlert { get; set; }

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
                        Tcs?.TrySetResult(LoginToFolderResult.Success);
                        return;
                    case MErrorType.API_EARGS:
                        Tcs?.TrySetResult(DecryptionAlert ?
                            LoginToFolderResult.InvalidDecryptionKey : // No valid decryption key
                            LoginToFolderResult.InvalidHandleOrDecryptionKey); // Handle length or Key length no valid
                        return;
                    case MErrorType.API_EINCOMPLETE: // Link has not decryption key
                        Tcs?.TrySetResult(LoginToFolderResult.NoDecryptionKey);
                        return;
                    default: // Unknown result, but not successful
                        Tcs?.TrySetResult(LoginToFolderResult.Unknown);
                        return;
                }
            }
        }

        #endregion
    }
}
