using System;
using MegaApp.Classes;
using MegaApp.Enums;
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
            string messageContent = null;
            if (CheckInputParameters())
            {
                if (ValidationService.IsValidEmail(this.Email))
                {
                    if (CheckPassword())
                    {
                        if (this.TermOfService)
                        {
                            var createAccount = new CreateAccountRequestListenerAsync();
                            var result = await createAccount.ExecuteAsync(() =>
                            {
                                this.MegaSdk.createAccount(
                                    this.Email, this.Password, this.FirstName, this.LastName, createAccount);
                            });

                            switch (result)
                            {
                                case CreateAccountResult.Success:
                                    {
                                        await DialogService.ShowAlertAsync(ResourceService.AppMessages.GetString("AM_ConfirmEmail_Title"),
                                            string.Format(ResourceService.AppMessages.GetString("AM_ConfirmEmail"), this.Email));
                                        return;
                                    }
                                case CreateAccountResult.AlreadyExists:
                                    {
                                        messageContent = ResourceService.AppMessages.GetString("AM_EmailAlreadyRegistered");
                                        break;
                                    }
                                case CreateAccountResult.Unknown:
                                    {
                                        messageContent = ResourceService.AppMessages.GetString("AM_CreateAccountFailed");
                                        break;
                                    }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        else
                        {
                            messageContent = ResourceService.AppMessages.GetString("AM_AgreeTermsOfService");
                           
                        }
                    }
                    else
                    {
                        messageContent = ResourceService.AppMessages.GetString("AM_PasswordsDoNotMatch");
                    }
                }
                else
                {
                    messageContent = ResourceService.AppMessages.GetString("AM_MalformedEmail");
                }
            }
            else
            {
                messageContent = ResourceService.AppMessages.GetString("AM_EmptyRequiredFields");
            }    
            if(string.IsNullOrEmpty(messageContent)) return;
            await DialogService.ShowAlertAsync(this.CreateAccountText, messageContent);
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

        public Uri TermsOfServiceUri => new Uri(ResourceService.AppResources.GetString("AR_TermsOfServiceUri"));

        #endregion

        #region UiResources

        public string AgreeCreateAccountText => ResourceService.UiResources.GetString("UI_AgreeCreateAccount");
        public string CreateAccountText => ResourceService.UiResources.GetString("UI_CreateAccount");
        public string ConfirmPasswordWatermarkText => ResourceService.UiResources.GetString("UI_ConfirmPasswordWatermark");
        public string EmailWatermarkText => ResourceService.UiResources.GetString("UI_EmailWatermark");
        public string FirstNameWatermarkText => ResourceService.UiResources.GetString("UI_FirstNameWatermark");
        public string LastNameWatermarkText => ResourceService.UiResources.GetString("UI_LastNameWatermark");
        public string PasswordWatermarkText => ResourceService.UiResources.GetString("UI_PasswordWatermark");
        public string TermsOfServiceText => ResourceService.UiResources.GetString("UI_TermsOfService");

        #endregion
    }
}
