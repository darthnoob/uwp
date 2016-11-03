using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using mega;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    class FastLoginRequestListener: BaseRequestListener
    {
        private readonly CloudDriveViewModel _cloudDriveViewModel;

        // Timer for ignore the received API_EAGAIN (-3) during login
        private DispatcherTimer timerAPI_EAGAIN;
        private bool isFirstAPI_EAGAIN;
        
        public FastLoginRequestListener(CloudDriveViewModel cloudDriveViewModel)
        {
            _cloudDriveViewModel = cloudDriveViewModel;

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
                //ProgressService.SetProgressIndicator(true, ProgressMessages.ServersTooBusy);
            });
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ResourceService.ProgressMessages.GetString("PM_FastLogin"); }
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
            get { return ResourceService.AppMessages.GetString("AM_LoginFailed_Title").ToUpper(); }
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
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { throw new NotImplementedException(); }
        }

        protected override NavigationObject NavigationObject
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Override Methods

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            UiService.OnUiThread(() => timerAPI_EAGAIN.Stop());

            if (e.getErrorCode() != MErrorType.API_OK)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_ENOENT: // E-mail unassociated with a MEGA account or Wrong password
                        new CustomMessageDialog(ErrorMessageTitle, ResourceService.AppMessages.GetString("AM_WrongEmailPasswordLogin"),
                            App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                        return;

                    case MErrorType.API_ETOOMANY: // Too many failed login attempts
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
            isFirstAPI_EAGAIN = true;
            base.onRequestStart(api, request);
        }

        public override void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Starts the timer when receives the first API_EAGAIN (-3)
            if (e.getErrorCode() == MErrorType.API_EAGAIN && isFirstAPI_EAGAIN)
            {
                isFirstAPI_EAGAIN = false;
                UiService.OnUiThread(() => timerAPI_EAGAIN.Start());
            }

            base.onRequestTemporaryError(api, request, e);
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            UiService.OnUiThread(() =>
            {
                //_cloudDriveViewModel.GetAccountDetails();
                _cloudDriveViewModel.FetchNodes();

                // Validate product subscription license on background thread
                //Task.Run(() => LicenseService.ValidateLicenses());
            });
        }

        #endregion
    }
}
