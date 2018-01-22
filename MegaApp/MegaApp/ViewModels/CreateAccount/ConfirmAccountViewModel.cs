using System;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels.CreateAccount
{
    public class ConfirmAccountViewModel : BasePageViewModel
    {
        public ConfirmAccountViewModel()
        {
            this.ControlState = true;
            this.ConfirmAccountCommand = new RelayCommand<object>(this.ConfirmAccount);
        }

        #region Methods

        private async void ConfirmAccount(object obj)
        {
            SetWarning(false, string.Empty);
            this.EmailInputState = InputState.Normal;
            this.PasswordInputState = InputState.Normal;

            if (!await NetworkService.IsNetworkAvailableAsync(true)) return;

            if (!CheckInputParameters()) return;


            this.ProgressHeaderText = ResourceService.ProgressMessages.GetString("PM_ConfirmAccountHeader");
            this.ProgressSubHeaderText = ResourceService.ProgressMessages.GetString("PM_ConfirmAccountSubHeader");
            this.ProgressText = ResourceService.ProgressMessages.GetString("PM_Patient");

            this.IsBusy = true;
            this.ControlState = false;
            this.ConfirmAccountButtonState = false;

            var confirm = new ConfirmAccountRequestListenerAsync();
            var result = await confirm.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.confirmAccount(ConfirmLink, Password, confirm);

            });

            this.ControlState = true;
            this.ConfirmAccountButtonState = true;
            this.IsBusy = false;

            string messageContent;
            switch (result)
            {
                case ConfirmAccountResult.Success:
                    messageContent = ResourceService.AppMessages.GetString("AM_ConfirmAccountSucces");
                    break;
                case ConfirmAccountResult.WrongPassword:
                    messageContent = ResourceService.AppMessages.GetString("AM_WrongPassword");
                    break;
                case ConfirmAccountResult.Unknown:
                    messageContent = ResourceService.AppMessages.GetString("AM_ConfirmAccountFailed");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await DialogService.ShowAlertAsync(
                ResourceService.UiResources.GetString("UI_ConfirmAccount"),
                messageContent);

            if (result != ConfirmAccountResult.Success) return;

            if (Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn()))
                SdkService.MegaSdk.logout(new LogOutRequestListener(false));

            var login = new LoginRequestListenerAsync();
            login.ServerBusy += OnServerBusy;

            this.ProgressHeaderText = ResourceService.ProgressMessages.GetString("PM_LoginHeader");
            this.ProgressSubHeaderText = null;
            this.ProgressText = ResourceService.ProgressMessages.GetString("PM_LoginSubHeader");

            this.IsBusy = true;
            this.ControlState = false;
            this.ConfirmAccountButtonState = false;

            var loginResult = await login.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.login(this.Email, this.Password, login);
            });

            string errorContent;
            switch (loginResult)
            {
                case LoginResult.Success:
                    SettingsService.SaveMegaLoginData(this.Email, SdkService.MegaSdk.dumpSession());

                    // Fetch nodes from MEGA
                    if (!await this.FetchNodes())
                    {
                        NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true);
                    }
                    else
                    {   // Navigate to the main page to load the main application for the user
                        NavigateService.Instance.Navigate(typeof(MainPage), true,
                            NavigationObject.Create(this.GetType(), NavigationActionType.Login));
                    }
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
                    errorContent = ResourceService.AppMessages.GetString("AM_LoginFailed");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.ControlState = true;
            this.ConfirmAccountButtonState = true;
            this.IsBusy = false;

            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_LoginFailed_Title"),
                errorContent);

            NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true);
        }

        private async Task<bool> FetchNodes()
        {
            try
            {
                this.ProgressText = ResourceService.ProgressMessages.GetString("PM_FetchNodesSubHeader");

                var fetchNodes = new FetchNodesRequestListenerAsync();
                fetchNodes.DecryptNodes += OnDecryptNodes;
                fetchNodes.ServerBusy += OnServerBusy;

                var fetchNodesResult = await fetchNodes.ExecuteAsync(() => SdkService.MegaSdk.fetchNodes(fetchNodes));
                if (fetchNodesResult != FetchNodesResult.Success)
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
                this.ConfirmAccountButtonState = true;
                this.IsBusy = false;

                return true;
            }
            catch (BlockedAccountException)
            {
                // Do nothing, app is already logging out
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Fetch nodes failed. Blocked account.");
                return false;
            }
        }

        private void OnDecryptNodes(object sender, EventArgs e)
        {
            OnUiThread(() => this.ProgressText = ResourceService.ProgressMessages.GetString("PM_DecryptNodesSubHeader"));
        }

        private void OnServerBusy(object sender, EventArgs e)
        {
            OnUiThread(() => this.ProgressText = ResourceService.ProgressMessages.GetString("PM_ServersTooBusySubHeader"));
        }

        private bool CheckInputParameters()
        {
            if (string.IsNullOrWhiteSpace(this.Email))
            {
                SetWarning(true, ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
                EmailInputState = InputState.Warning;
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.Password))
            {
                SetWarning(true, ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
                PasswordInputState = InputState.Warning;
                return false;
            }

            if (ValidationService.IsValidEmail(this.Email)) return true;

            SetWarning(true, ResourceService.AppMessages.GetString("AM_IncorrectEmailFormat"));
            EmailInputState = InputState.Warning;
            return false;
        }

        private void SetButtonState()
        {
            this.ConfirmAccountButtonState = !string.IsNullOrWhiteSpace(this.Password) &&
                                             !string.IsNullOrWhiteSpace(this.Email);
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

        public ICommand ConfirmAccountCommand { get; set; }

        #endregion

        #region Properties

        public string ConfirmLink { get; set; }

        private bool _confirmAccountButtonState;
        public bool ConfirmAccountButtonState
        {
            get { return _confirmAccountButtonState; }
            set { SetField(ref _confirmAccountButtonState, value); }
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
     
        public string Email { get; set; }

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

        private InputState _passwordInputState;
        public InputState PasswordInputState
        {
            get { return _passwordInputState; }
            set { SetField(ref _passwordInputState, value); }
        }

        private InputState _emailInputState;
        public InputState EmailInputState
        {
            get { return _emailInputState; }
            set { SetField(ref _emailInputState, value); }
        }
        #endregion

        #region UiResources

        public string EmailWatermarkText => ResourceService.UiResources.GetString("UI_EmailWatermark");
        public string ConfirmAccountText => ResourceService.UiResources.GetString("UI_CreateAccount");
        public string ConfirmAccountHeaderText => ResourceService.UiResources.GetString("UI_ConfirmYourAccountTitle");
        public string ConfirmAccountDescriptionText => ResourceService.UiResources.GetString("UI_ConfirmYourAccount");
        public string PasswordWatermarkText => ResourceService.UiResources.GetString("UI_PasswordWatermark");

        #endregion

        #region Visual Resources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

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

        private string _progressText;
        public string ProgressText
        {
            get { return _progressText; }
            set { SetField(ref _progressText, value); }
        }

        #endregion
    }
}
