using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class ChangeEmailDialogViewModel : BaseContentDialogViewModel
    {
        public ChangeEmailDialogViewModel() : base()
        {
            this.SaveButtonCommand = new RelayCommand(Save);
            this.CancelButtonCommand = new RelayCommand(Cancel);

            this.TitleText = ResourceService.UiResources.GetString("UI_ChangeEmail");
            this.MessageText = ResourceService.UiResources.GetString("UI_ChangeEmailDescription");
        }

        #region Commands

        public ICommand SaveButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }

        #endregion

        #region Methods

        private async void Save()
        {
            this.WarningText = string.Empty;
            this.NewEmailInputState = InputState.Normal;

            if (!ValidationService.IsValidEmail(this.NewEmail))
            {
                this.WarningText = ResourceService.AppMessages.GetString("AM_IncorrectEmailFormat");
                this.NewEmailInputState = InputState.Warning;
                return;
            }

            this.IsBusy = true;
            this.ControlState = false;

            ChangeEmailResult result = ChangeEmailResult.Unknown;
            var changeEmail = new ChangeEmailRequestListenerAsync();

            var mfaStatus = await AccountService.CheckMultiFactorAuthStatusAsync();
            if (mfaStatus == MultiFactorAuthStatus.Enabled)
            {
                this.OnHideDialog();
                await DialogService.ShowAsyncMultiFactorAuthCodeInputDialogAsync(async (string code) =>
                {
                    result = await changeEmail.ExecuteAsync(() =>
                    {
                        SdkService.MegaSdk.multiFactorAuthChangeEmail(
                            this.NewEmail, code, changeEmail);
                    });

                    if (result == ChangeEmailResult.MultiFactorAuthInvalidCode)
                    {
                        DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();
                        return false;
                    }

                    return true;
                });

                this.OnShowDialog();
            }
            else
            {
                result = await changeEmail.ExecuteAsync(() =>
                    SdkService.MegaSdk.changeEmail(this.NewEmail, changeEmail));
            }

            this.IsBusy = false;
            this.ControlState = true;

            switch (result)
            {
                case ChangeEmailResult.Success:
                    this.OnHideDialog();
                    DialogService.ShowAwaitEmailConfirmationDialog(this.NewEmail);
                    break;

                case ChangeEmailResult.AlreadyRequested:
                    this.WarningText = ResourceService.AppMessages.GetString("AM_ChangeEmailAlreadyRequested");
                    break;

                case ChangeEmailResult.UserNotLoggedIn:
                    this.WarningText = ResourceService.AppMessages.GetString("AM_UserNotOnline");
                    break;

                case ChangeEmailResult.MultiFactorAuthInvalidCode:
                case ChangeEmailResult.Unknown:
                    this.WarningText = ResourceService.AppMessages.GetString("AM_ChangeEmailGenericError");
                    break;
            }
        }

        private void Cancel() => this.OnHideDialog();

        #endregion

        #region Properties

        private string _newEmail;
        public string NewEmail
        {
            get { return _newEmail; }
            set
            {
                SetField(ref _newEmail, value);
                OnPropertyChanged(nameof(this.PrimaryButtonState));
                this.NewEmailInputState = InputState.Normal;
                this.WarningText = string.Empty;
            }
        }

        private string _warningText;
        public string WarningText
        {
            get { return _warningText; }
            set { SetField(ref _warningText, value); }
        }

        private InputState _newEmailInputState;
        public InputState NewEmailInputState
        {
            get { return _newEmailInputState; }
            set { SetField(ref _newEmailInputState, value); }
        }

        /// <summary>
        /// State of the primary button of the input dialog
        /// </summary>
        public bool PrimaryButtonState => this.ControlState &&
            !string.IsNullOrWhiteSpace(this.NewEmail);

        #endregion

        #region UiResources

        public string EnterNewEmailText => ResourceService.UiResources.GetString("UI_EnterNewEmail");
        public string SaveText => ResourceService.UiResources.GetString("UI_Save");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        
        #endregion
    }
}
