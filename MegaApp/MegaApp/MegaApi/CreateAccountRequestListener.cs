using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using mega;
using MegaApp.Enums;
using MegaApp.Classes;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class CreateAccountRequestListener : BaseRequestListener
    {
        private readonly CreateAccountViewModel _createAccountViewModel;
        private readonly LoginAndCreateAccountPage _loginPage;

        public CreateAccountRequestListener(CreateAccountViewModel createAccountViewModel, LoginAndCreateAccountPage loginPage)
        {
            _createAccountViewModel = createAccountViewModel;
            _loginPage = loginPage;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return App.ResourceLoaders.ProgressMessages.GetString("PM_CreateAccount"); }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return App.ResourceLoaders.AppMessages.GetString("AM_CreateAccountFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return App.ResourceLoaders.AppMessages.GetString("AM_CreateAccountFailed_Title").ToUpper(); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return App.ResourceLoaders.AppMessages.GetString("AM_ConfirmNeeded"); }
        }

        protected override string SuccessMessageTitle
        {
            get { return App.ResourceLoaders.AppMessages.GetString("AM_ConfirmNeeded_Title").ToUpper(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; } //Shown when navigates to the "InitTourPage"
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
            //get { return typeof(InitTourPage); }
            get { return typeof(LoginAndCreateAccountPage); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { return NavigationParameter.CreateAccount; }
        }

        #endregion

        #region MRequestListenerInterface

        public override void onRequestStart(MegaSDK api, MRequest request)
        {            
            if (request.getType() == MRequestType.TYPE_CREATE_ACCOUNT)
                base.onRequestStart(api, request);
        }

        public async override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            _createAccountViewModel.Email = request.getEmail();

                            if (!String.IsNullOrWhiteSpace(_createAccountViewModel.Email))
                                this._loginPage.SetStatusTxtEmailCreateAccount(true);
                        });
                        break;

                    case MErrorType.API_EARGS: // Invalid #newsignup link
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            new CustomMessageDialog(
                                App.ResourceLoaders.AppMessages.GetString("AM_InvalidLink"),
                                App.ResourceLoaders.AppMessages.GetString("AM_NewSignUpInvalidLink"),
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialogAsync();
                        });
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
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            new CustomMessageDialog(
                                ErrorMessageTitle,
                                App.ResourceLoaders.AppMessages.GetString("AM_EmailAlreadyRegistered"),
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialogAsync();
                        });
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
