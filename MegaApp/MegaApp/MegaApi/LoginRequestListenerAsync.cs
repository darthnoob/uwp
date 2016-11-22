using System;
using Windows.UI.Xaml;
using mega;
using MegaApp.Enums;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    internal class LoginRequestListenerAsync : BaseRequestListenerAsync<LoginResult>
    {
        // Timer for ignore the received API_EAGAIN (-3) during login
        private readonly DispatcherTimer _timerApiEagain;
        private bool _isFirstApiEagain;

        public LoginRequestListenerAsync()
        {
            _timerApiEagain = new DispatcherTimer();
            _timerApiEagain.Tick += TimerApiEagainOnTick;
            _timerApiEagain.Interval = new TimeSpan(0, 0, 10);            
        }

        private void TimerApiEagainOnTick(object sender, object o)
        {
            UiService.OnUiThread(() =>
            {
                _timerApiEagain.Stop();
                //ProgressService.SetProgressIndicator(true, ProgressMessages.PM_ServersTooBusy);
            });
        }
       
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            UiService.OnUiThread(() =>
            {
                _timerApiEagain.Stop();  
            });

            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

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
                default: // Unknown result, but not successful
                    Tcs?.TrySetResult(LoginResult.Unknown);
                    return;
            }
        }

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            this._isFirstApiEagain = true;
            base.onRequestStart(api, request);
        }

        public override void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Starts the timer when receives the first API_EAGAIN (-3)
            if (e.getErrorCode() == MErrorType.API_EAGAIN && this._isFirstApiEagain)
            {
                this._isFirstApiEagain = false;
                UiService.OnUiThread(() => _timerApiEagain.Start());
            }

            base.onRequestTemporaryError(api, request, e);
        }

        #endregion
    }
}
