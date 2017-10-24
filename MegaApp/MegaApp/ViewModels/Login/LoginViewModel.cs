using System;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;
using ForgotPasswordPage = MegaApp.Views.Login.ForgotPasswordPage;

namespace MegaApp.ViewModels.Login
{
    public class LoginViewModel : BaseSdkViewModel
    {
        public LoginViewModel()
        {
            this.ControlState = true;
            this.LoginCommand = new RelayCommand(Login);
            this.ForgotMyPasswordCommand = new RelayCommand(ForgotMyPassword);
        }

        #region Methods

        private void OnDecryptNodes(object sender, EventArgs e)
        {
            OnUiThread(() => this.ProgressSubHeaderText = ResourceService.ProgressMessages.GetString("PM_DecryptNodesSubHeader"));
        }

        private void OnServerBusy(object sender, EventArgs e)
        {
            OnUiThread(() => this.ProgressSubHeaderText = ResourceService.ProgressMessages.GetString("PM_ServersTooBusySubHeader"));
        }

        /// <summary>
        /// Log in to a MEGA account.
        /// </summary>
        public async void Login()
        {
            SetWarning(false, string.Empty);
            SetInputState();

            if (!CheckInputParameters()) return;

            var login = new LoginRequestListenerAsync();
            login.ServerBusy += OnServerBusy;

            LoginResult result;
            try
            {
                this.ControlState = false;
                this.LoginButtonState = false;
                this.IsBusy = true;

                this.ProgressHeaderText = ResourceService.ProgressMessages.GetString("PM_LoginHeader");
                this.ProgressSubHeaderText = ResourceService.ProgressMessages.GetString("PM_LoginSubHeader");

                result = await login.ExecuteAsync(() => this.MegaSdk.login(this.Email, this.Password, login));
            }
            catch (BlockedAccountException)
            {
                // Do nothing, app is already logging out
                return;
            }

            // Set default error content
            var errorContent = ResourceService.AppMessages.GetString("AM_LoginFailed");
            switch (result)
            {
                case LoginResult.Success:
                    SettingsService.SaveMegaLoginData(this.Email, this.MegaSdk.dumpSession());

                    // Validate product subscription license on background thread
                    Task.Run(() => LicenseService.ValidateLicensesAsync());

                    // Fetch nodes from MEGA
                    if (!await this.FetchNodes()) return;

                    // Navigate to the main page to load the main application for the user
                    NavigateService.Instance.Navigate(typeof(MainPage), true,
                        NavigationObject.Create(this.GetType(), NavigationActionType.Login));
                    return;

                case LoginResult.UnassociatedEmailOrWrongPassword:
                    errorContent = ResourceService.AppMessages.GetString("AM_WrongEmailPasswordLogin");
                    break;

                case LoginResult.TooManyLoginAttempts:
                    // Too many failed login attempts. Wait one hour.
                    errorContent = string.Format(ResourceService.AppMessages.GetString("AM_TooManyFailedLoginAttempts"),
                        DateTime.Now.AddHours(1).ToString("HH:mm:ss"));
                    break;

                case LoginResult.AccountNotConfirmed:
                    errorContent = ResourceService.AppMessages.GetString("AM_AccountNotConfirmed");
                    break;

                case LoginResult.Unknown:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.ControlState = true;
            this.LoginButtonState = true;
            this.IsBusy = false;
           
            // Show error message
            SetWarning(true, errorContent);
            SetInputState(InputState.Warning, InputState.Warning);
        }

        private void ForgotMyPassword()
        {
            NavigateService.Instance.Navigate(typeof(ForgotPasswordPage), true);
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

        private void SetInputState(
            InputState email = InputState.Normal, 
            InputState password = InputState.Normal)
        {
            OnUiThread(() =>
            {
                this.EmailInputState = email;
                this.PasswordInputState = password;
            });
        }

        private void SetState()
        {
            var enabled = !string.IsNullOrEmpty(this.Email) &&
                          !string.IsNullOrWhiteSpace(this.Email) &&
                          !string.IsNullOrEmpty(this.Password);

            OnUiThread(() => this.LoginButtonState = enabled);
        }

        private bool CheckInputParameters()
        {
            if (string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.Password))
            {
                SetWarning(true, ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
                SetInputState(InputState.Warning, InputState.Warning);
                return false;
            }

            if (ValidationService.IsValidEmail(this.Email)) return true;

            SetWarning(true, ResourceService.AppMessages.GetString("AM_IncorrectEmailFormat"));
            SetInputState(InputState.Warning);
            return false;
        }

        /// <summary>
        /// Log in to a MEGA account using a session key and show an alert if something went wrong.
        /// </summary>
        /// <returns>TRUE if all was well or FALSE in other case.</returns>
        public async Task<bool> FastLogin()
        {
            var fastLogin = new FastLoginRequestListenerAsync();
            fastLogin.ServerBusy += OnServerBusy;

            bool fastLoginResult;
            try
            {
                this.ControlState = false;
                this.IsBusy = true;

                this.ProgressHeaderText = ResourceService.ProgressMessages.GetString("PM_FastLoginHeader");
                this.ProgressSubHeaderText = ResourceService.ProgressMessages.GetString("PM_LoginSubHeader");

                fastLoginResult = await fastLogin.ExecuteAsync(() =>
                {
                    SdkService.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(
                        ResourceService.SettingsResources.GetString("SR_UserMegaSession")),
                        fastLogin);
                });
            }
            // Do nothing, app is already logging out
            catch (BadSessionIdException) { return false; }
            catch (BlockedAccountException) { return false; }

            if (!fastLoginResult)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Resume session failed.");
                await DialogService.ShowAlertAsync(
                    ResourceService.UiResources.GetString("UI_ResumeSession"),
                    ResourceService.AppMessages.GetString("AM_ResumeSessionFailed"));
                return false;
            }

            // Validate product subscription license on background thread
            Task.Run(() => LicenseService.ValidateLicensesAsync());

            // Fetch nodes from MEGA
            return await this.FetchNodes();
        }

        /// <summary>
        /// Fetch nodes and show an alert if something went wrong.
        /// </summary>
        /// <returns>TRUE if all was well or FALSE in other case.</returns>
        private async Task<bool> FetchNodes()
        {
            try
            {
                this.ProgressSubHeaderText = ResourceService.ProgressMessages.GetString("PM_FetchNodesSubHeader");

                var fetchNodes = new FetchNodesRequestListenerAsync();
                fetchNodes.DecryptNodes += OnDecryptNodes;
                fetchNodes.ServerBusy += OnServerBusy;

                var fetchNodesResult = await fetchNodes.ExecuteAsync(() => SdkService.MegaSdk.fetchNodes(fetchNodes));
                if (!fetchNodesResult)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Fetch nodes failed.");
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_FetchNodesFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_FetchNodesFailed"));
                    return false;
                }

                // Enable the transfer resumption for the main MegaSDK instance
                SdkService.MegaSdk.enableTransferResumption();

                this.ControlState = true;
                this.IsBusy = false;

                return true;
            }
            catch (BlockedAccountException)
            {
                // Do nothing, app is already logging out
                return false;
            }
        }

        #endregion

        #region Commands

        public ICommand LoginCommand { get; }
        public ICommand ForgotMyPasswordCommand { get; }

        #endregion

        #region Properties
       
        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                SetField(ref _email, value);
                SetState();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                SetField(ref _password, value);
                SetState();
            }
        }
       
        public string SessionKey { get; set; }

        private bool _isWarningVisible;
        public bool IsWarningVisible
        {
            get { return _isWarningVisible; }
            set { SetField(ref _isWarningVisible, value); }
        }

        private bool _loginButtonState;
        public bool LoginButtonState
        {
            get { return _loginButtonState; }
            set { SetField(ref _loginButtonState, value); }
        }

        private string _warningText;
        public string WarningText
        {
            get { return _warningText; }
            set { SetField(ref _warningText, value); }
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

        #endregion

        #region UiResources

        public string EmailWatermarkText => ResourceService.UiResources.GetString("UI_EmailWatermark");
        public string LoginText => ResourceService.UiResources.GetString("UI_LoginWithSpace");
        public string PasswordWatermarkText => ResourceService.UiResources.GetString("UI_PasswordWatermark");
        public string ForgotMyPasswordText => ResourceService.UiResources.GetString("UI_ForgotMyPassword");

        #endregion

        #region ProgressMessages

        private string _progressHeaderText;
        public string ProgressHeaderText
        {
            get { return _progressHeaderText; }
            set { SetField(ref _progressHeaderText, value); }
        }

        private string _progressSubHeaderText;
        public string ProgressSubHeaderText
        {
            get { return _progressSubHeaderText; }
            set { SetField(ref _progressSubHeaderText, value); }
        }

        #endregion
    }

}
