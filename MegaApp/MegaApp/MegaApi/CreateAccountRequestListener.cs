using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using mega;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.ViewModels;
using MegaApp.Views;

namespace MegaApp.MegaApi
{
    class CreateAccountRequestListener : BaseRequestListener
    {
        private readonly CreateAccountViewModel _createAccountViewModel;

        public CreateAccountRequestListener(CreateAccountViewModel createAccountViewModel)
        {
            _createAccountViewModel = createAccountViewModel;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ResourceService.ProgressMessages.GetString("PM_CreateAccount"); }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_CreateAccountFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return ResourceService.UiResources.GetString("UI_CreateAccount"); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get
            {
                return string.Format(ResourceService.AppMessages.GetString("AM_ConfirmEmail"), 
                    _createAccountViewModel.Email);
            }
        }

        protected override string SuccessMessageTitle
        {
            get { return ResourceService.AppMessages.GetString("AM_ConfirmEmail_Title"); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return true; }
        }

        protected override bool NavigateOnSucces
        {
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return false; }
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

        #region MRequestListenerInterface

        public override void onRequestStart(MegaSDK api, MRequest request)
        {            
            if (request.getType() == MRequestType.TYPE_CREATE_ACCOUNT)
                base.onRequestStart(api, request);
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            UiService.OnUiThread(() =>
            {
                //ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                //ProgressService.SetProgressIndicator(false);
                
                _createAccountViewModel.ControlState = true;
                //_loginPage.SetApplicationBar(true);
            });
            
            if (request.getType() == MRequestType.TYPE_QUERY_SIGNUP_LINK)
            {
                switch(e.getErrorCode())
                {
                    case MErrorType.API_OK: // Valid and operative #newsignup link
                        UiService.OnUiThread(() =>
                        {
                            _createAccountViewModel.Email = request.getEmail();
                           
                            if (!string.IsNullOrWhiteSpace(_createAccountViewModel.Email))
                                _createAccountViewModel.IsReadOnly = true;
                        });
                        break;

                    case MErrorType.API_EARGS: // Invalid #newsignup link
                        new CustomMessageDialog(
                            ResourceService.AppMessages.GetString("AM_InvalidLink"),
                            ResourceService.AppMessages.GetString("AM_NewSignUpInvalidLink"),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                        break;

                    default: // Default error processing
                        base.onRequestFinish(api, request, e);
                        break;
                }
            }
            
            if (request.getType() == MRequestType.TYPE_CREATE_ACCOUNT)
            {
                switch(e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull create account process
                        base.onRequestFinish(api, request, e);
                        break;

                    case MErrorType.API_EEXIST: // Email already registered
                        new CustomMessageDialog(
                            ErrorMessageTitle,
                            ResourceService.AppMessages.GetString("AM_EmailAlreadyRegistered"),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                        break;

                    default: // Default error processing
                        base.onRequestFinish(api, request, e);
                        break;
                }
            }
        }

        #endregion
    }
}
