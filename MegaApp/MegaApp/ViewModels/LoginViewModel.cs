using System;
using System.Threading.Tasks;
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
            this.ProgressSubHeaderText = ResourceService.ProgressMessages.GetString("PM_DecryptNodesSubHeader");
        }

        private void OnServerBusy(object sender, EventArgs e)
        {
            this.ProgressSubHeaderText = ResourceService.ProgressMessages.GetString("PM_ServersTooBusySubHeader");
        }

        public async void Login()
        {
            if (!await CheckInputParametersAsync()) return;

            var login = new LoginRequestListenerAsync();
            login.ServerBusy += OnServerBusy;

            LoginResult result;
            try
            {
                this.ControlState = false;
                this.IsBusy = true;

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
            await DialogService.ShowAlertAsync(this.LoginText, errorContent);
        }

        private async Task<bool> CheckInputParametersAsync()
        {
            if (string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.Password))
            {
                await DialogService.ShowAlertAsync(this.LoginText,
                    ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
                return false;
            }
            
            if(!ValidationService.IsValidEmail(this.Email))
            {
                await DialogService.ShowAlertAsync(this.LoginText,
                   ResourceService.AppMessages.GetString("AM_MalformedEmail"));
                return false;
            }

            return true;
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
                if (!await fetchNodes.ExecuteAsync(() => this.MegaSdk.fetchNodes(fetchNodes)))
                {
                    this.ControlState = true;
                    this.IsBusy = false;

                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_FetchNodesFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_FetchNodesFailed"));
                    return false;
                }

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
       
        public string ProgressHeaderText => ResourceService.ProgressMessages.GetString("PM_LoginHeader");

        private string _progressSubHeaderText;
        public string ProgressSubHeaderText
        {
            get { return _progressSubHeaderText; }
            set { SetField(ref _progressSubHeaderText, value); }
        }

        #endregion
    }
}
