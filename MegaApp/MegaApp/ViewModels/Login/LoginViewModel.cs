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
        public LoginViewModel(MegaSDK megaSdk) : base(megaSdk)
        {
            this.ControlState = true;
            this.LoginCommand = new RelayCommand(Login);
            this.ForgotMyPasswordCommand = new RelayCommand(ForgotMyPassword);
        }

        #region Methods

        protected void OnDecryptNodes(object sender, EventArgs e)
        {
            OnUiThread(() => this.ProgressText = ResourceService.ProgressMessages.GetString("PM_DecryptNodesSubHeader"));
        }

        protected void OnIsWaiting(object sender, MRetryReason reason)
        {
            string message = string.Empty;
            switch(reason)
            {
                case MRetryReason.RETRY_CONNECTIVITY:
                    message = ResourceService.ProgressMessages.GetString("PM_ConnectivityIssue");
                    break;

                case MRetryReason.RETRY_SERVERS_BUSY:
                    message = ResourceService.ProgressMessages.GetString("PM_ServersBusy");
                    break;

                case MRetryReason.RETRY_API_LOCK:
                    message = ResourceService.ProgressMessages.GetString("PM_ApiLocked");
                    break;

                case MRetryReason.RETRY_RATE_LIMIT:
                    message = ResourceService.ProgressMessages.GetString("PM_ApiRateLimit");
                    break;

                default: return;
            }

            OnUiThread(() => this.ProgressText = message);
        }

        /// <summary>
        /// Log in to a MEGA account.
        /// </summary>
        public async void Login()
        {
            if (!await NetworkService.IsNetworkAvailableAsync(true)) return;

            // Reset the flag to store if the account has been blocked
            AccountService.IsAccountBlocked = false;

            SetWarning(false, string.Empty);
            SetInputState();

            if (!CheckInputParameters()) return;

            var login = new LoginRequestListenerAsync();
            login.IsWaiting += OnIsWaiting;

            this.ControlState = false;
            this.LoginButtonState = false;
            this.IsBusy = true;

            this.ProgressHeaderText = ResourceService.ProgressMessages.GetString("PM_LoginHeader");
            this.ProgressText = ResourceService.ProgressMessages.GetString("PM_LoginSubHeader");

            LoginResult result = await login.ExecuteAsync(() =>
                this.MegaSdk.login(this.Email, this.Password, login));

            if (result == LoginResult.MultiFactorAuthRequired)
            {
                await DialogService.ShowAsyncMultiFactorAuthCodeInputDialogAsync(async (string code) =>
                {
                    result = await login.ExecuteAsync(() =>
                    this.MegaSdk.multiFactorAuthLogin(this.Email, this.Password, code, login));

                    if (result == LoginResult.MultiFactorAuthInvalidCode)
                    {
                        DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();
                        return false;
                    }

                    return true;
                });
            }

            // Set default error content
            var errorContent = ResourceService.AppMessages.GetString("AM_LoginFailed");
            switch (result)
            {
                case LoginResult.Success:
                    SettingsService.SaveSessionToLocker(this.Email, this.MegaSdk.dumpSession());

                    // Validate product subscription license on background thread
                    Task.Run(() => LicenseService.ValidateLicensesAsync());

                    // Initialize the DB
                    AppService.InitializeDatabase();

                    // Fetch nodes from MEGA
                    var fetchNodesResult = await this.FetchNodes();
                    if (fetchNodesResult != FetchNodesResult.Success)
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Fetch nodes failed.");
                        if (!AccountService.IsAccountBlocked)
                            this.ShowFetchNodesFailedAlertDialog();
                        return;
                    }

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

                case LoginResult.MultiFactorAuthRequired:
                case LoginResult.MultiFactorAuthInvalidCode:
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

        protected virtual void SetButtonState()
        {
            var enabled = !string.IsNullOrWhiteSpace(this.Email) &&
                          !string.IsNullOrWhiteSpace(this.Password);

            OnUiThread(() => this.LoginButtonState = enabled);
        }

        private bool CheckInputParameters()
        {
            if (string.IsNullOrWhiteSpace(this.Email) || string.IsNullOrWhiteSpace(this.Password))
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
        /// <param name="progressHeaderText">Customized header text to display during the process (optional).</param>
        /// <returns>TRUE if all was well or FALSE in other case.</returns>
        public async Task<bool> FastLoginAsync(string progressHeaderText = null)
        {
            // Reset the flag to store if the account has been blocked
            AccountService.IsAccountBlocked = false;

            var fastLogin = new FastLoginRequestListenerAsync();
            fastLogin.IsWaiting += OnIsWaiting;

            bool fastLoginResult;
            try
            {
                this.ControlState = false;
                this.IsBusy = true;

                this.ProgressHeaderText = progressHeaderText ?? ResourceService.ProgressMessages.GetString("PM_FastLoginHeader");
                this.ProgressText = ResourceService.ProgressMessages.GetString("PM_LoginSubHeader");

                fastLoginResult = await fastLogin.ExecuteAsync(async () =>
                    this.MegaSdk.fastLogin(await SettingsService.LoadSessionFromLockerAsync(), fastLogin));
            }
            // Do nothing, app is already logging out
            catch (BadSessionIdException)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Login failed. Bad session ID.");
                return false;
            }

            if (!fastLoginResult)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Resume session failed.");

                if (!AccountService.IsAccountBlocked)
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.UiResources.GetString("UI_ResumeSession"),
                        ResourceService.AppMessages.GetString("AM_ResumeSessionFailed"));
                }
                
                return false;
            }

            // Validate product subscription license on background thread
            Task.Run(() => LicenseService.ValidateLicensesAsync());

            // Fetch nodes from MEGA
            var fetchNodesResult = await this.FetchNodes();
            if (fetchNodesResult != FetchNodesResult.Success)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Fetch nodes failed.");
                if (!AccountService.IsAccountBlocked)
                    this.ShowFetchNodesFailedAlertDialog();
                return false;
            }

            // If is required show the password reminder dialog on background thread
            Task.Run(async () =>
            {
                if (await AccountService.ShouldShowPasswordReminderDialogAsync(false))
                    UiService.OnUiThread(() => DialogService.ShowPasswordReminderDialog(false));
            });

            return true;
        }

        /// <summary>
        /// Fetch nodes and show an alert if something went wrong.
        /// </summary>
        /// <returns>TRUE if all was well or FALSE in other case.</returns>
        protected virtual async Task<FetchNodesResult> FetchNodes()
        {
            this.ProgressText = ResourceService.ProgressMessages.GetString("PM_FetchNodesSubHeader");

            this.ControlState = false;
            this.IsBusy = true;

            var fetchNodes = new FetchNodesRequestListenerAsync();
            fetchNodes.DecryptNodes += OnDecryptNodes;
            fetchNodes.IsWaiting  += OnIsWaiting;

            var fetchNodesResult = await fetchNodes.ExecuteAsync(() => this.MegaSdk.fetchNodes(fetchNodes));
            if (fetchNodesResult == FetchNodesResult.Success)
            {
                // Enable the transfer resumption for the MegaSDK instance
                this.MegaSdk.enableTransferResumption();
            }

            this.ControlState = true;
            this.IsBusy = false;

            return fetchNodesResult;
        }

        protected async void ShowFetchNodesFailedAlertDialog()
        {
            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_FetchNodesFailed_Title"),
                ResourceService.AppMessages.GetString("AM_FetchNodesFailed"));
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
                SetButtonState();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                SetField(ref _password, value);
                SetButtonState();
            }
        }

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

        private string _progressText;
        public string ProgressText
        {
            get { return _progressText; }
            set { SetField(ref _progressText, value); }
        }

        #endregion
    }

}
