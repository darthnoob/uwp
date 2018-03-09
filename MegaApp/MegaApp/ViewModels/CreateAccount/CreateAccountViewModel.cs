using System;
using System.Collections.Generic;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views.Login;

namespace MegaApp.ViewModels.CreateAccount
{
    public class CreateAccountViewModel : BaseSdkViewModel
    {
        public CreateAccountViewModel() : base(SdkService.MegaSdk)
        {
            this.ControlState = true;
            this.CreateAccountCommand = new RelayCommand(CreateAccount);
        }

        #region Methods

        public async void CreateAccount()
        {
            SetWarning(false, string.Empty);
            SetInputState();

            if (!CheckInputParameters()) return;

            if (CheckPassword()){

                if (CheckPasswordStrenght())
                {
                    if (this.TermOfService)
                    {
                        this.IsBusy = true;
                        this.ControlState = false;
                        this.CreateAccountButtonState = false;

                        var createAccount = new CreateAccountRequestListenerAsync();
                        var result = await createAccount.ExecuteAsync(() =>
                        {
                            this.MegaSdk.createAccount(
                                this.Email,
                                this.Password,
                                this.FirstName,
                                this.LastName,
                                createAccount);
                        });

                        this.ControlState = true;
                        this.CreateAccountButtonState = true;
                        this.IsBusy = false;

                        string messageContent;
                        switch (result)
                        {
                            case CreateAccountResult.Success:
                                {
                                    NavigateService.Instance.Navigate(typeof(ConfirmEmailPage), true,
                                        new NavigationObject
                                        {
                                            Action = NavigationActionType.Default,
                                            Parameters = new Dictionary<NavigationParamType, object>
                                            {
                                                { NavigationParamType.Email, this.Email },
                                                { NavigationParamType.Password, this.Password },
                                                { NavigationParamType.FirstName, this.FirstName },
                                                { NavigationParamType.LastName, this.LastName },
                                            }
                                        });
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
                        OnUiThread(async () => await DialogService.ShowAlertAsync(this.CreateAccountText, messageContent));
                    }
                    else
                    {
                        SetWarning(true, ResourceService.AppMessages.GetString("AM_AgreeTermsOfService"));
                        SetInputState(termsOfService: InputState.Warning);
                    }
                }
                else
                {
                    SetInputState(password: InputState.Warning, confirmPassword: InputState.Warning);
                    SetWarning(true, ResourceService.AppMessages.GetString("AM_VeryWeakPassword"));
                }
               
            }
            else
            {
                SetInputState(password: InputState.Warning, confirmPassword: InputState.Warning);
                SetWarning(true, ResourceService.AppMessages.GetString("AM_PasswordsDoNotMatch"));
            }
           
        }        

        private bool CheckInputParameters()
        {
            //Because lastname is not an obligatory parameter, if the lastname field is null or empty,
            //force it to be an empty string to avoid "ArgumentNullException" when call the createAccount method.
            if (string.IsNullOrWhiteSpace(this.LastName))
                this.LastName = string.Empty;

            if (string.IsNullOrWhiteSpace(this.Email) || 
                string.IsNullOrWhiteSpace(this.Password) ||
                string.IsNullOrWhiteSpace(this.FirstName) ||
                string.IsNullOrWhiteSpace(this.ConfirmPassword))
            {
                SetWarning(true, ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
                SetInputState(
                    email: InputState.Warning,
                    password: InputState.Warning,
                    confirmPassword: InputState.Warning,
                    firstName: InputState.Warning);
                return false;
            }

            if (ValidationService.IsValidEmail(this.Email)) return true;

            SetWarning(true, ResourceService.AppMessages.GetString("AM_IncorrectEmailFormat"));
            SetInputState(InputState.Warning);
            return false;
        }

        private bool CheckPassword()
        {
            return this.Password.Equals(this.ConfirmPassword);
        }

        private bool CheckPasswordStrenght()
        {
            return PasswordStrength != MPasswordStrength.PASSWORD_STRENGTH_VERYWEAK;
        }

        private void CalculatePasswordStrength(string value)
        {
            this.PasswordStrength = ValidationService.CalculatePasswordStrength(value);
        }

        private void SetInputState(
            InputState email = InputState.Normal,
            InputState password = InputState.Normal,
            InputState firstName = InputState.Normal,
            InputState confirmPassword = InputState.Normal,
            InputState termsOfService = InputState.Normal)
        {
            OnUiThread(() =>
            {
                this.EmailInputState = email;
                this.PasswordInputState = password;
                this.FirstNameInputState = firstName;
                this.ConfirmPasswordInputState = confirmPassword;
                this.TermsOfServiceInputState = termsOfService;
            });
        }

        private void SetButtonState()
        {
            var enabled = !string.IsNullOrWhiteSpace(this.Email) &&
                          !string.IsNullOrWhiteSpace(this.FirstName) &&
                          !string.IsNullOrWhiteSpace(this.Password) &&
                          !string.IsNullOrWhiteSpace(this.ConfirmPassword) &&
                          this.TermOfService;

            OnUiThread(() => this.CreateAccountButtonState = enabled);
        }

        private void SetWarning(bool isVisible, string warningText)
        {
            if (isVisible)
            {
                // First text and then display
                this.WarningText = warningText;
                this.IsWarningVisible = true;
            }
            else
            {
                // First remove and than clean text
                this.IsWarningVisible = false;
                this.WarningText = warningText;
            }
        }

        #endregion

        #region Commands

        public ICommand CreateAccountCommand { get; }

        #endregion

        #region Properties

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { SetField(ref _isReadOnly, value); }
        }
        
        private string _email;
        public string Email 
        {
            get { return _email; }
            set
            {
                SetField(ref _email, value);
                SetButtonState();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if(SetField(ref _password, value))
                    CalculatePasswordStrength(value);
                SetButtonState();
            }
        }

        private MPasswordStrength _passwordStrength;
        public MPasswordStrength PasswordStrength
        {
            get { return _passwordStrength; }
            set { SetField(ref _passwordStrength, value); }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                SetField(ref _confirmPassword, value);
                SetButtonState();
            }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                SetField(ref _firstName, value);
                SetButtonState();
            }
        }

        private bool _termOfService;
        public bool TermOfService
        {
            get { return _termOfService; }
            set
            {
                SetField(ref _termOfService, value);
                SetButtonState();
            }
        }

        public string LastName { get; set; }

        private bool _createAccountButtonState;
        public bool CreateAccountButtonState
        {
            get { return _createAccountButtonState; }
            set { SetField(ref _createAccountButtonState, value); }
        }

        private string _warningText;
        public string WarningText
        {
            get { return _warningText; }
            set { SetField(ref _warningText, value); }
        }

        private bool _isWarningVisible;
        public bool IsWarningVisible
        {
            get { return _isWarningVisible; }
            set { SetField(ref _isWarningVisible, value); }
        }

        private InputState _firstNameInputState;
        public InputState FirstNameInputState
        {
            get { return _firstNameInputState; }
            set { SetField(ref _firstNameInputState, value); }
        }

        private InputState _emailInputState;
        public InputState EmailInputState
        {
            get { return _emailInputState; }
            set { SetField(ref _emailInputState, value); }
        }

        private InputState _passwordInputState;
        public InputState PasswordInputState
        {
            get { return _passwordInputState; }
            set { SetField(ref _passwordInputState, value); }
        }

        private InputState _confirmPasswordInputState;
        public InputState ConfirmPasswordInputState
        {
            get { return _confirmPasswordInputState; }
            set { SetField(ref _confirmPasswordInputState, value); }
        }

        private InputState _termsOfServiceInputState;
        public InputState TermsOfServiceInputState
        {
            get { return _termsOfServiceInputState; }
            set { SetField(ref _termsOfServiceInputState, value); }
        }

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

        #region ProgressMessages

        public string ProgressHeaderText => ResourceService.ProgressMessages.GetString("PM_CreateAccountHeader");
        public string ProgressText => ResourceService.ProgressMessages.GetString("PM_CreateAccountSubHeader");

        #endregion
    }
}
