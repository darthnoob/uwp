using System;
using mega;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    abstract class BaseRequestListener: MRequestListenerInterface
    {
        #region Properties

        abstract protected string ProgressMessage { get; }
        abstract protected bool ShowProgressMessage { get; }
        abstract protected string ErrorMessage { get; }
        abstract protected string ErrorMessageTitle { get; }
        abstract protected bool ShowErrorMessage { get; }
        abstract protected string SuccessMessage { get; }
        abstract protected string SuccessMessageTitle { get; }
        abstract protected bool ShowSuccesMessage { get; }
        abstract protected bool NavigateOnSucces { get; }
        abstract protected bool ActionOnSucces { get; }
        abstract protected Type NavigateToPage { get; }
        abstract protected NavigationObject NavigationObject { get; }

        #endregion

        #region MRequestListenerInterface

        public async virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            switch(e.getErrorCode())
            {
                case MErrorType.API_OK:
                    if (ShowSuccesMessage)
                    {
                        await DialogService.ShowAlertAsync(
                            SuccessMessageTitle, SuccessMessage);
                    }

                    if (ActionOnSucces)
                        OnSuccesAction(api, request);

                    if (NavigateOnSucces)
                        UiService.OnUiThread(() => NavigateService.Instance.Navigate(NavigateToPage, true, NavigationObject));
                    break;

                case MErrorType.API_EGOINGOVERQUOTA: // Not enough storage quota
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                        string.Format("Not enough storage quota ({0})", e.getErrorCode().ToString()));
                    UiService.OnUiThread(() => DialogService.ShowStorageOverquotaAlert(true));
                    break;

                case MErrorType.API_EOVERQUOTA: //Storage overquota error
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                        string.Format("Storage quota exceeded ({0})", e.getErrorCode().ToString()));
                    UiService.OnUiThread(() =>
                    {
                        AccountService.AccountDetails.IsInStorageOverquota = true;
                        DialogService.ShowStorageOverquotaAlert(false);
                    });
                    break;

                default:
                    if (e.getErrorCode() != MErrorType.API_EINCOMPLETE)
                    {
                        if (ShowErrorMessage)
                        {
                            UiService.OnUiThread(async() =>
                            {
                                await DialogService.ShowAlertAsync(ErrorMessageTitle,
                                    string.Format(ErrorMessage, e.getErrorString()));
                            });
                        }
                    }
                    break;
            }
        }

        public virtual void onRequestStart(MegaSDK api, MRequest request)
        {
            // Not necessary
        }

        public virtual void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Not necessary
        }

        public virtual void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // Not necessary
        }

        #endregion

        #region Virtual Methods

        protected virtual void OnSuccesAction(MegaSDK api, MRequest request)
        {
            // No standard succes action
        }

        #endregion
    }
}
