using System;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.Login;
using MegaApp.Views;

namespace MegaApp.ViewModels.CreateAccount
{
    public class ConfirmAccountViewModel : LoginViewModel
    {
        public ConfirmAccountViewModel() : base(SdkService.MegaSdk)
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
            login.IsWaiting += OnIsWaiting;

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
                    var fetchNodesResult = await this.FetchNodes();
                    if (fetchNodesResult != FetchNodesResult.Success)
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Fetch nodes failed.");
                        this.ShowFetchNodesFailedAlertDialog();
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

        protected override async Task<FetchNodesResult> FetchNodes()
        {
            var result = await base.FetchNodes();
            this.ConfirmAccountButtonState = true;
            return result;
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

        protected override void SetButtonState()
        {
            var enabled = !string.IsNullOrWhiteSpace(this.Email) &&
                          !string.IsNullOrWhiteSpace(this.Password);

            OnUiThread(() => this.ConfirmAccountButtonState = enabled);
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

        #endregion

        #region UiResources

        public string ConfirmAccountText => ResourceService.UiResources.GetString("UI_CreateAccount");
        public string ConfirmAccountHeaderText => ResourceService.UiResources.GetString("UI_ConfirmYourAccountTitle");
        public string ConfirmAccountDescriptionText => ResourceService.UiResources.GetString("UI_ConfirmYourAccount");

        #endregion

        #region Visual Resources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion

        #region ProgressMessages

        private string _progressSubHeaderText;
        public string ProgressSubHeaderText
        {
            get { return _progressSubHeaderText; }
            set { SetField(ref _progressSubHeaderText, value); }
        }

        #endregion
    }
}
