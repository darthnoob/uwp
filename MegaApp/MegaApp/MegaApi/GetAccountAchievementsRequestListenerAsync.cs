using mega;

namespace MegaApp.MegaApi
{
    internal class GetAccountAchievementsRequestListenerAsync : BaseRequestListenerAsync<MAchievementsDetails>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_GET_ACHIEVEMENTS)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successful get account achievements process
                        Tcs?.TrySetResult(request.getMAchievementsDetails());
                        break;
                    default: // Default error processing
                        Tcs?.TrySetResult(null);
                        break;
                }
            }
        }

        #endregion
    }
}
