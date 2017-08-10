using System;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    public class LoginViewModel : BaseSdkViewModel
    {
        public LoginViewModel()
        {
            this.ControlState = true;
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
            if (!CheckInputParametersAsync()) return;

            var login = new LoginRequestListenerAsync();
            login.ServerBusy += OnServerBusy;

            LoginResult result;
            try
            {
                this.ControlState = false;
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
            this.IsBusy = false;

            // Show error message
            OnUiThread(async () => await DialogService.ShowAlertAsync(this.LoginText, errorContent));
        }

        private bool CheckInputParametersAsync()
        {
            if (string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.Password))
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(this.LoginText,
                        ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
                });
                return false;
            }
            
            if(!ValidationService.IsValidEmail(this.Email))
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(this.LoginText,
                        ResourceService.AppMessages.GetString("AM_IncorrectEmailFormat"));
                });
                return false;
            }

            return true;
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
        
        #region Properties

        public string Email { get; set; }
        public string Password { get; set; }
        public string SessionKey { get; set; }

        #endregion

        #region UiResources

        public string EmailWatermarkText => ResourceService.UiResources.GetString("UI_EmailWatermark");
        public string LoginText => ResourceService.UiResources.GetString("UI_Login");
        public string PasswordWatermarkText => ResourceService.UiResources.GetString("UI_PasswordWatermark");

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
