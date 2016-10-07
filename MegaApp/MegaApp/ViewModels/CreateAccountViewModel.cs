using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    public class CreateAccountViewModel : BaseSdkViewModel 
    {
        private readonly MegaSDK _megaSdk;
     
        public CreateAccountViewModel(MegaSDK megaSdk)
            : base(megaSdk)
        {
            this._megaSdk = megaSdk;
            this.ControlState = true;
        }

        #region Methods

        public async void CreateAccount()
        {
            if (CheckInputParameters())
            {
                if (ValidationService.IsValidEmail(this.Email))
                {
                    if (CheckPassword())
                    {
                        if (this.TermOfService)
                        {
                            this._megaSdk.createAccount(this.Email, this.Password, this.FirstName, this.LastName,
                                new CreateAccountRequestListener(this));
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
            if (String.IsNullOrWhiteSpace(this.LastName))
                this.LastName = String.Empty;

            return !String.IsNullOrEmpty(this.Email) && !String.IsNullOrEmpty(this.FirstName) && 
                !String.IsNullOrEmpty(this.Password) && !String.IsNullOrEmpty(this.ConfirmPassword);
        }

        private bool CheckPassword()
        {
            return this.Password.Equals(this.ConfirmPassword);
        }

        #endregion

        #region Properties

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { SetField(ref _isReadOnly, value); }
        }

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
