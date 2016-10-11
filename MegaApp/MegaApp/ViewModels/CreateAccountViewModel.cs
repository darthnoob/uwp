using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class CreateAccountViewModel : BaseSdkViewModel 
    {
        public CreateAccountViewModel()
        {
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
                            this.MegaSdk.createAccount(this.Email, this.Password, this.FirstName, this.LastName,
                                new CreateAccountRequestListener(this));
                        }
                        else
                        {
                            await this.OnUiThread(() =>
                            {
                                //_loginPage.SetApplicationBar(true)
                                new CustomMessageDialog(
                                    ResourceService.AppMessages.GetString("AM_CreateAccountFailed_Title"),
                                    ResourceService.AppMessages.GetString("AM_AgreeTermsOfService"),
                                    App.AppInformation,
                                    MessageDialogButtons.Ok).ShowDialogAsync();
                            });
                        }
                    }
                    else
                    {
                        await this.OnUiThread(() =>
                        {
                            //_loginPage.SetApplicationBar(true)
                            new CustomMessageDialog(
                                ResourceService.AppMessages.GetString("AM_CreateAccountFailed_Title"),
                                ResourceService.AppMessages.GetString("AM_PasswordsDoNotMatch"),
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialogAsync();
                        });
                    }
                }
                else
                {
                    await this.OnUiThread(() =>
                    {
                        //_loginPage.SetApplicationBar(true)
                        new CustomMessageDialog(
                            ResourceService.AppMessages.GetString("AM_CreateAccountFailed_Title"),
                            ResourceService.AppMessages.GetString("AM_MalformedEmail"),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialogAsync();
                    });
                }
            }
            else
            {
                await this.OnUiThread(() =>
                {
                    //_loginPage.SetApplicationBar(true)
                    new CustomMessageDialog(
                        ResourceService.AppMessages.GetString("AM_CreateAccountFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_RequiredFieldsCreateAccount"),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
            }            
        }        

        private bool CheckInputParameters()
        {
            //Because lastname is not an obligatory parameter, if the lastname field is null or empty,
            //force it to be an empty string to avoid "ArgumentNullException" when call the createAccount method.
            if (string.IsNullOrWhiteSpace(this.LastName))
                this.LastName = string.Empty;

            return !string.IsNullOrEmpty(this.Email) && !string.IsNullOrEmpty(this.FirstName) && 
                !string.IsNullOrEmpty(this.Password) && !string.IsNullOrEmpty(this.ConfirmPassword);
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

        public Uri AR_TermsOfServiceUri { get { return new Uri(ResourceService.AppResources.GetString("AR_TermsOfServiceUri")); } }

        #endregion

        #region UiResources

        public string UI_CreateAccount { get { return ResourceService.UiResources.GetString("UI_CreateAccount"); } }
        public string UI_FirstNameWatermark { get { return ResourceService.UiResources.GetString("UI_FirstNameWatermark"); } }
        public string UI_LastNameWatermark { get { return ResourceService.UiResources.GetString("UI_LastNameWatermark"); } }
        public string UI_EmailWatermark { get { return ResourceService.UiResources.GetString("UI_EmailWatermark"); } }
        public string UI_PasswordWatermark { get { return ResourceService.UiResources.GetString("UI_PasswordWatermark"); } }
        public string UI_ConfirmPasswordWatermark { get { return ResourceService.UiResources.GetString("UI_ConfirmPasswordWatermark"); } }
        public string UI_AgreeCreateAccount { get { return ResourceService.UiResources.GetString("UI_AgreeCreateAccount"); } }
        public string UI_TermsOfService { get { return ResourceService.UiResources.GetString("UI_TermsOfService"); } }

        #endregion
    }
}
