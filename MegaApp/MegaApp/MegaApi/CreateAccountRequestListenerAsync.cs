using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.ViewModels;
using MegaApp.Views;

namespace MegaApp.MegaApi
{
    internal class CreateAccountRequestListenerAsync : BaseRequestListenerAsync<CreateAccountResult>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_CREATE_ACCOUNT)
            {
                switch(e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull create account process
                        Tcs?.TrySetResult(CreateAccountResult.Success);
                        break;
                    case MErrorType.API_EEXIST: // Email already registered
                        Tcs?.TrySetResult(CreateAccountResult.AlreadyExists);
                        break;
                    default: // Default error processing
                        Tcs?.TrySetResult(CreateAccountResult.Unknown);
                        break;
                }
            }
        }

        #endregion
    }
}
