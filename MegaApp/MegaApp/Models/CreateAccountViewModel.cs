using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Services;

namespace MegaApp.Models
{    
    class CreateAccountViewModel : BaseSdkViewModel 
    {
        private readonly MegaSDK _megaSdk;
        private readonly LoginAndCreateAccountPage _loginPage;

        public CreateAccountViewModel(MegaSDK megaSdk, LoginAndCreateAccountPage loginPage)
            : base(megaSdk)
        {
            this._megaSdk = megaSdk;
            this._loginPage = loginPage;
            this.ControlState = true;
        }

        #region Methods

        public async void CreateAccount()
        {
            if (CheckInputParameters())
            {
                if (ValidationService.IsValidEmail(Email))
                {
                    if (CheckPassword())
                    {
                        if (TermOfService)
                        {
                            this._megaSdk.createAccount(Email, Password, FirstName, LastName,
                                new CreateAccountRequestListener(this, _loginPage));
                        }
                        else
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                //_loginPage.SetApplicationBar(true)
                                new CustomMessageDialog(
                                    App.ResourceLoaders.AppMessages.GetString("AM_CreateAccountFailed_Title"),
                                    App.ResourceLoaders.AppMessages.GetString("AM_AgreeTermsOfService"),
                                    App.AppInformation,
                                    MessageDialogButtons.Ok).ShowDialogAsync();
                            });                            
                        }
                    }
                    else
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            //_loginPage.SetApplicationBar(true)
                            new CustomMessageDialog(
                                App.ResourceLoaders.AppMessages.GetString("AM_CreateAccountFailed_Title"),
                                App.ResourceLoaders.AppMessages.GetString("AM_PasswordsDoNotMatch"),
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialogAsync();
                        });
                    }
                }
                else 
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        //_loginPage.SetApplicationBar(true)
                        new CustomMessageDialog(
                            App.ResourceLoaders.AppMessages.GetString("AM_CreateAccountFailed_Title"),
                            App.ResourceLoaders.AppMessages.GetString("AM_MalformedEmail"),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialogAsync();
                    });
                }
            }
            else
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //_loginPage.SetApplicationBar(true)
                    new CustomMessageDialog(
                        App.ResourceLoaders.AppMessages.GetString("AM_CreateAccountFailed_Title"),
                        App.ResourceLoaders.AppMessages.GetString("AM_RequiredFieldsCreateAccount"),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
            }            
        }        

        private bool CheckInputParameters()
        {
            //Because lastname is not an obligatory parameter, if the lastname field is null or empty,
            //force it to be an empty string to avoid "ArgumentNullException" when call the createAccount method.
            if (String.IsNullOrWhiteSpace(LastName))
                LastName = String.Empty;

            return !String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(FirstName) && 
                !String.IsNullOrEmpty(Password) && !String.IsNullOrEmpty(ConfirmPassword);
        }

        private bool CheckPassword()
        {
            return Password.Equals(ConfirmPassword);
        }

        #endregion

        #region Properties

        public string NewSignUpCode { get; set; }

        private string _email;
        public string Email 
        {
            get { return _email; }
            set { SetField(ref _email, value); }
        }
        
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool TermOfService { get; set; }

        #endregion

        #region AppResources

        public Uri AR_TermsOfServiceUri { get { return new Uri(App.ResourceLoaders.AppResources.GetString("AR_TermsOfServiceUri")); } }

        #endregion

        #region UiResources

        public string UI_CreateAccount { get { return App.ResourceLoaders.UiResources.GetString("UI_CreateAccount"); } }
        public string UI_FirstNameWatermark { get { return App.ResourceLoaders.UiResources.GetString("UI_FirstNameWatermark"); } }
        public string UI_LastNameWatermark { get { return App.ResourceLoaders.UiResources.GetString("UI_LastNameWatermark"); } }
        public string UI_EmailWatermark { get { return App.ResourceLoaders.UiResources.GetString("UI_EmailWatermark"); } }
        public string UI_PasswordWatermark { get { return App.ResourceLoaders.UiResources.GetString("UI_PasswordWatermark"); } }
        public string UI_ConfirmPasswordWatermark { get { return App.ResourceLoaders.UiResources.GetString("UI_ConfirmPasswordWatermark"); } }
        public string UI_AgreeCreateAccount { get { return App.ResourceLoaders.UiResources.GetString("UI_AgreeCreateAccount"); } }
        public string UI_TermsOfService { get { return App.ResourceLoaders.UiResources.GetString("UI_TermsOfService"); } }

        #endregion
    }
}
