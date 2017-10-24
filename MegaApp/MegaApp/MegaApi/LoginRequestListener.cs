using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.ViewModels.Login;
using MegaApp.Views;

namespace MegaApp.MegaApi
{
    class LoginRequestListener : BaseRequestListener
    {
        private readonly LoginViewModel _loginViewModel;

        // Timer for ignore the received API_EAGAIN (-3) during login
        private DispatcherTimer timerAPI_EAGAIN;
        private bool isFirstAPI_EAGAIN;

        public LoginRequestListener(LoginViewModel loginViewModel)
        {
            _loginViewModel = loginViewModel;

            timerAPI_EAGAIN = new DispatcherTimer();
            timerAPI_EAGAIN.Tick += timerTickAPI_EAGAIN;
            timerAPI_EAGAIN.Interval = new TimeSpan(0, 0, 10);            
        }

        // Method which is call when the timer event is triggered
        private void timerTickAPI_EAGAIN(object sender, object e)
        {
            UiService.OnUiThread(() =>
            {
                timerAPI_EAGAIN.Stop();
                //ProgressService.SetProgressIndicator(true, ProgressMessages.PM_ServersTooBusy);
            });
        }

        #region  Base Properties

        protected override string ProgressMessage
        {
            get { return ResourceService.ProgressMessages.GetString("PM_Login"); }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_LoginFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return ResourceService.UiResources.GetString("UI_Login"); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return true; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { return (typeof(MainPage)); }
        }

        protected override NavigationObject NavigationObject
        {
            get { return NavigationObject.Create(typeof(LoginViewModel), NavigationActionType.Login); }
        }

        #endregion

        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            UiService.OnUiThread(() =>
            {
                //ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                //ProgressService.SetProgressIndicator(false);

                _loginViewModel.ControlState = true;

                timerAPI_EAGAIN.Stop();                
            });            

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                _loginViewModel.SessionKey = api.dumpSession();
            }
            else
            {
                //if (_loginAndCreateAccountPage != null)
                //    Deployment.Current.Dispatcher.BeginInvoke(() => _loginAndCreateAccountPage.SetApplicationBar(true));

                switch (e.getErrorCode())
                {
                    case MErrorType.API_ENOENT: // Email unassociated with a MEGA account or Wrong password
                        new CustomMessageDialog(ErrorMessageTitle, ResourceService.AppMessages.GetString("AM_WrongEmailPasswordLogin"),
                            App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                        return;

                    case MErrorType.API_ETOOMANY: // Too many failed login attempts. Wait one hour.
                        new CustomMessageDialog(ErrorMessageTitle,
                            string.Format(ResourceService.AppMessages.GetString("AM_TooManyFailedLoginAttempts"), DateTime.Now.AddHours(1).ToString("HH:mm:ss")),
                            App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                        return;

                    case MErrorType.API_EINCOMPLETE: // Account not confirmed
                        new CustomMessageDialog(ErrorMessageTitle, ResourceService.AppMessages.GetString("AM_AccountNotConfirmed"),
                            App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                        return;

                    case MErrorType.API_EBLOCKED: // Account blocked
                        base.onRequestFinish(api, request, e);
                        return;
                }
            }            

            base.onRequestFinish(api, request, e);
        }

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            this.isFirstAPI_EAGAIN = true;
            base.onRequestStart(api, request);
        }

        public override void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Starts the timer when receives the first API_EAGAIN (-3)
            if (e.getErrorCode() == MErrorType.API_EAGAIN && this.isFirstAPI_EAGAIN)
            {
                this.isFirstAPI_EAGAIN = false;
                UiService.OnUiThread(() => timerAPI_EAGAIN.Start());
            }

            base.onRequestTemporaryError(api, request, e);
        }

        #endregion

        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            SettingsService.SaveMegaLoginData(_loginViewModel.Email, 
                _loginViewModel.SessionKey);

            // Validate product subscription license on background thread
            Task.Run(() => LicenseService.ValidateLicensesAsync());
        }

        #endregion
    }
}
