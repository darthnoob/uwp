using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.Views.Login;

namespace MegaApp.ViewModels.Login
{
    public class RecoverViewModel : BasePageViewModel
    {
        public RecoverViewModel()
        {
            this.ControlState = true;
            this.VerifyCommand = new RelayCommand(Verify);
            this.UploadKeyCommand = new RelayCommand(UploadKey);
        }

        #region Methods

        private async void Verify()
        {
            SetWarning(false, string.Empty);
            this.RecoveryKeyInputState = InputState.Normal;

            if (!CheckInputParametersAsync()) return;

            this.IsBusy = true;
            this.ControlState = false;
            this.VerifyButtonState = false;

            await VerifyKey(this.RecoveryKey);

            this.ControlState = true;
            this.VerifyButtonState = true;
            this.IsBusy = false;
        }

        private async Task<bool> VerifyKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length != 22)
            {
                SetWarning(true, ResourceService.AppMessages.GetString("AM_InvalidKeyFile_Title"));
                this.RecoveryKeyInputState = InputState.Warning;
                await DialogService.ShowAlertAsync(ResourceService.AppMessages.GetString("AM_InvalidKeyFile_Title"),
                    ResourceService.AppMessages.GetString("AM_InvalidKeyFile"));
                return false;
            }
           
            NavigateService.Instance.Navigate(typeof(ConfirmRecoveryPage), true,
                new NavigationObject
                {
                    Action = NavigationActionType.Default,
                    Parameters = new Dictionary<NavigationParamType, object>
                    {
                        { NavigationParamType.Data, key }
                    }
                });

            return true;
        }

        private async void UploadKey()
        {
            var file = await FileService.SelectSingleFile(new[] { ".txt" });
            if (file == null) return;

            string key;
            try
            {
                key = await FileIO.ReadTextAsync(file);
            }
            catch
            {
                key = null;
            }

            this.RecoveryKey = key;
            Verify();
        }

        private bool CheckInputParametersAsync()
        {
            if (!string.IsNullOrWhiteSpace(this.RecoveryKey)) return true;

            SetWarning(true, ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
            this.RecoveryKeyInputState = InputState.Warning;
            return false;
        }

        private void SetWarning(bool isVisible, string warningText)
        {
            OnUiThread(() =>
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
            });
        }


        #endregion

        #region Commands

        public ICommand VerifyCommand { get; }
        public ICommand UploadKeyCommand { get; }

        #endregion

        #region Properties

        private string _recoveryKey;
        public string RecoveryKey
        {
            get { return _recoveryKey; }
            set
            {
                SetField(ref _recoveryKey, value);
                this.VerifyButtonState = !string.IsNullOrWhiteSpace(_recoveryKey);
            }
        }

        private InputState _recoveryKeyInputState;
        public InputState RecoveryKeyInputState
        {
            get { return _recoveryKeyInputState; }
            set { SetField(ref _recoveryKeyInputState, value); }
        }

        private bool _verifyButtonState;
        public bool VerifyButtonState
        {
            get { return _verifyButtonState; }
            set { SetField(ref _verifyButtonState, value); }
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

        #endregion

        #region UiResources

        public string VerifyText => ResourceService.UiResources.GetString("UI_Verify");
        public string UploadKeyText => ResourceService.UiResources.GetString("UI_UploadKey");
        public string RecoverHeaderText => ResourceService.UiResources.GetString("UI_RecoverHeader");
        public string RecoverDescriptionText => ResourceService.UiResources.GetString("UI_RecoverDescription");
        public string RecoveryKeyWatermarkText => ResourceService.UiResources.GetString("UI_RecoveryKey");

        #endregion

        #region Visual Resources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }
}
