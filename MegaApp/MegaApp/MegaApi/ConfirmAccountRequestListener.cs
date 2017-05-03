using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using mega;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.Views;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    class ConfirmAccountRequestListener : BaseRequestListener
    {
        private readonly ConfirmAccountViewModel _confirmAccountViewModel;

        public ConfirmAccountRequestListener(ConfirmAccountViewModel confirmAccountViewModel)
        {
            _confirmAccountViewModel = confirmAccountViewModel;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ResourceService.ProgressMessages.GetString("PM_ConfirmAccount"); }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_ConfirmAccountFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return ResourceService.UiResources.GetString("UI_ConfirmAccount"); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_ConfirmAccountSucces"); }
        }

        protected override string SuccessMessageTitle
        {
            get { return ResourceService.UiResources.GetString("UI_ConfirmAccount"); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return true; }
        }

        protected override bool NavigateOnSucces
        {
            get { return true; }
        }

        protected override bool ActionOnSucces
        {
            get { return false; }
        }

        protected override Type NavigateToPage
        {
            get { return typeof(LoginAndCreateAccountPage); }
        }

        protected override NavigationObject NavigationObject
        {
            get { return NavigationObject.Create(typeof(ConfirmAccountViewModel)); }
        }

        #endregion

        #region MRequestListenerInterface

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            UiService.OnUiThread(() => this._confirmAccountViewModel.ControlState = false);

            if (request.getType() == MRequestType.TYPE_CONFIRM_ACCOUNT)
                base.onRequestStart(api, request);
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            UiService.OnUiThread(() =>
            {
                //ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                //ProgressService.SetProgressIndicator(false);

                this._confirmAccountViewModel.ControlState = true;

                if (request.getType() == MRequestType.TYPE_QUERY_SIGNUP_LINK)
                {
                    switch (e.getErrorCode())
                    {
                        case MErrorType.API_OK: // Valid and operative confirmation link
                            this._confirmAccountViewModel.Email = request.getEmail();
                            break;

                        case MErrorType.API_ENOENT: // Already confirmed account
                            ShowErrorMesageAndNavigate(ErrorMessageTitle,
                                ResourceService.AppMessages.GetString("AM_AlreadyConfirmedAccount"));
                            break;

                        case MErrorType.API_EINCOMPLETE: // Incomplete confirmation link
                            ShowErrorMesageAndNavigate(ErrorMessageTitle,
                                ResourceService.AppMessages.GetString("AM_IncompleteConfirmationLink"));
                            break;

                        case MErrorType.API_EOVERQUOTA: // Storage overquota error
                        default: // Other error
                            base.onRequestFinish(api, request, e);
                            break;
                    }
                }
                else if (request.getType() == MRequestType.TYPE_CONFIRM_ACCOUNT)
                {
                    switch (e.getErrorCode())
                    {
                        case MErrorType.API_OK: // Successfull confirmation process
                            var customMessageDialog = new CustomMessageDialog(
                                SuccessMessageTitle, SuccessMessage,
                                App.AppInformation, MessageDialogButtons.Ok);
                            
                            customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                                OnSuccesAction(api, request);
                            
                                customMessageDialog.ShowDialog();
                            break;

                        case MErrorType.API_ENOENT: // Wrong password
                            new CustomMessageDialog(
                                ErrorMessageTitle,
                                ResourceService.AppMessages.GetString("AM_WrongPassword"),
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                            break;

                        case MErrorType.API_EOVERQUOTA: //Storage overquota error
                        default: // Other error
                            base.onRequestFinish(api, request, e);                            
                            break;
                    }
                }
            });
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            if (Convert.ToBoolean(api.isLoggedIn()))
                api.logout(new LogOutRequestListener(false));

            App.AppInformation.IsNewlyActivatedAccount = true;

            api.login(request.getEmail(), request.getPassword(),
                new LoginRequestListener(new LoginViewModel()));
        }

        private void ShowErrorMesageAndNavigate(string title, string message)
        {
            var customMessageDialog = new CustomMessageDialog(
                title, message, App.AppInformation, MessageDialogButtons.Ok);

            customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                NavigateService.Instance.Navigate(NavigateToPage, true, NavigationObject);

            customMessageDialog.ShowDialog();
        }

        #endregion
    }
}
