using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    public class ConfirmChangeEmailViewModel : BaseSdkViewModel
    {
        public ConfirmChangeEmailViewModel() : base(SdkService.MegaSdk)
        {
            this.ControlState = true;
            this.ConfirmEmailCommand = new RelayCommand(ConfirmEmail);
            this.OkButtonCommand = new RelayCommand(OkButton);
        }

        #region Commands

        public ICommand ConfirmEmailCommand { get; set; }
        public ICommand OkButtonCommand { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the user changes the email successfully.
        /// </summary>
        public event EventHandler EmailChanged;

        /// <summary>
        /// Event invocator method called when the user changes the email successfully.
        /// </summary>
        protected virtual void OnEmailChanged()
        {
            this.EmailChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the password is wrong.
        /// </summary>
        public event EventHandler PasswordError;

        /// <summary>
        /// Event invocator method called when the password is wrong.
        /// </summary>
        protected virtual void OnPasswordError()
        {
            this.ErrorMessage = ResourceService.AppMessages.GetString("AM_WrongPassword");
            this.PasswordError?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Confirm the email change
        /// </summary>
        private async void ConfirmEmail()
        {
            if(string.IsNullOrWhiteSpace(this.Password))
            {
                OnPasswordError();
                return;
            }

            var changeEmail = new ConfirmChangeEmailRequestListenerAsync();
            var result = await changeEmail.ExecuteAsync(() =>
                    this.MegaSdk.confirmChangeEmail(this.VerifyEmailLink, this.Password, changeEmail));

            switch(result)
            {
                case ConfirmChangeEmailResult.Success:
                    AccountService.UserData.UserEmail = this.Email;
                    OnEmailChanged();
                    return;

                case ConfirmChangeEmailResult.UserNotLoggedIn:
                    await DialogService.ShowAlertAsync(ResourceService.UiResources.GetString("UI_ChangeEmail"),
                        ResourceService.AppMessages.GetString("AM_UserNotOnline"));
                    break;

                case ConfirmChangeEmailResult.WrongPassword:
                    OnPasswordError();
                    return;

                case ConfirmChangeEmailResult.WrongAccount:
                    await DialogService.ShowAlertAsync(ResourceService.UiResources.GetString("UI_ChangeEmail"),
                        ResourceService.AppMessages.GetString("AM_WrongAccount"));
                    break;

                case ConfirmChangeEmailResult.Unknown:
                default:
                    await DialogService.ShowAlertAsync(ResourceService.UiResources.GetString("UI_ChangeEmail"),
                        ResourceService.AppMessages.GetString("AM_ChangeEmailGenericError"));
                    break;
            }

            OnUiThread(() =>
            {
                NavigateService.Instance.Navigate(typeof(MainPage), true,
                    NavigationObject.Create(typeof(ConfirmChangeEmailViewModel), NavigationActionType.Default));
            });
        }

        private void OkButton()
        {
            OnUiThread(() =>
            {
                NavigateService.Instance.Navigate(typeof(MainPage), true,
                    NavigationObject.Create(typeof(ConfirmChangeEmailViewModel), NavigationActionType.Default));
            });
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Process the verify email link
        /// </summary>
        public async void ProcessVerifyEmailLink()
        {
            if (string.IsNullOrWhiteSpace(App.LinkInformation.ActiveLink) ||
                !App.LinkInformation.ActiveLink.Contains("#verify")) return;

            this.VerifyEmailLink = App.LinkInformation.ActiveLink;
            App.LinkInformation.Reset();

            var verifyEmail = new QueryChangeEmailLinkRequestListenerAsync();
            var result = await verifyEmail.ExecuteAsync(() =>
                this.MegaSdk.queryChangeEmailLink(this.VerifyEmailLink, verifyEmail));

            switch (result)
            {
                case QueryChangeEmailLinkResult.Success:
                    this.Email = verifyEmail.Email;
                    return;

                case QueryChangeEmailLinkResult.InvalidLink:
                    await DialogService.ShowAlertAsync(ResourceService.UiResources.GetString("UI_ChangeEmail"),
                        ResourceService.AppMessages.GetString("AM_ChangeEmailInvalidLink"));
                    break;

                case QueryChangeEmailLinkResult.UserNotLoggedIn:
                    await DialogService.ShowAlertAsync(ResourceService.UiResources.GetString("UI_ChangeEmail"),
                        ResourceService.AppMessages.GetString("AM_UserNotOnline"));
                    break;

                case QueryChangeEmailLinkResult.Unknown:
                default:
                    await DialogService.ShowAlertAsync(ResourceService.UiResources.GetString("UI_ChangeEmail"),
                        ResourceService.AppMessages.GetString("AM_ChangeEmailGenericError"));
                    break;
            }

            OnUiThread(() =>
            {
                NavigateService.Instance.Navigate(typeof(MainPage), true,
                    NavigationObject.Create(typeof(ConfirmChangeEmailViewModel), NavigationActionType.Default));
            });
        }

        #endregion

        #region Properties

        private string _headerText;
        public string HeaderText
        {
            get { return _headerText; }
            set { SetField(ref _headerText, value); }
        }

        private string _subHeaderText;
        public string SubHeaderText
        {
            get { return _subHeaderText; }
            set { SetField(ref _subHeaderText, value); }
        }

        private string VerifyEmailLink { get; set; }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { SetField(ref _email, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetField(ref _password, value); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetField(ref _errorMessage, value); }
        }

        #endregion

        #region UiResources

        public string EnterPasswordText => ResourceService.UiResources.GetString("UI_PasswordWatermark");
        public string ConfirmEmailText => ResourceService.UiResources.GetString("UI_ConfirmEmail");
        public string OkText => ResourceService.UiResources.GetString("UI_Ok");

        #endregion

        #region VisualResources

        public string MegaIconPathData => ResourceService.VisualResources.GetString("VR_MegaIconPathData");

        #endregion
    }
}
