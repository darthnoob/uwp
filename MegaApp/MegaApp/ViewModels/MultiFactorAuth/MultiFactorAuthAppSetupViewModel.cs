using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;
using ZXing;
using ZXing.QrCode;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels.MultiFactorAuth
{
    public class MultiFactorAuthAppSetupViewModel : BasePageViewModel
    {
        public MultiFactorAuthAppSetupViewModel()
        {
            this.CopySeedCommand = new RelayCommand(this.CopySeed);
            this.NextCommand = new RelayCommand(this.EnableMultiFactorAuth);
            this.OpenInCommand = new RelayCommand(this.OpenIn);            

            this.Initialize();
        }

        #region Commands

        public ICommand CopySeedCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand OpenInCommand { get; }

        #endregion

        private async void Initialize()
        {
            var multiFactorAuthGetCode = new MultiFactorAuthGetCodeRequestListenerAsync();
            this.MultiFactorAuthCode = await multiFactorAuthGetCode.ExecuteAsync(() =>
                SdkService.MegaSdk.multiFactorAuthGetCode(multiFactorAuthGetCode));

            this.SetQR();
        }

        private async void CopySeed()
        {
            try
            {
                var data = new DataPackage();
                data.SetText(this.MultiFactorAuthCode);

                Clipboard.SetContent(data);

                ToastService.ShowTextNotification(
                    ResourceService.AppMessages.GetString("AM_MFA_SeedCopied_Title"),
                    ResourceService.AppMessages.GetString("AM_MFA_SeedCopied"));
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_MFA_SeedCopiedFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_MFA_SeedCopiedFailed"));
            }
        }

        private async void OpenIn() =>
            await Launcher.LaunchUriAsync(new Uri(this.codeURI, UriKind.RelativeOrAbsolute));

        private async void EnableMultiFactorAuth()
        {
            await DialogService.ShowAsyncMultiFactorAuthCodeInputDialogAsync(
                this.EnableMultiFactorAuthAsync, TwoFactorAuthText, SetupStep2Text, false);
        }

        /// <summary>
        /// Set the QR code image to set up the Multi-Factor Authentication
        /// </summary>
        private void SetQR()
        {
            var options = new QrCodeEncodingOptions()
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 100,
                Height = 100
            };

            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = options;
            QRImage = writer.Write(this.codeURI);
        }

        /// <summary>
        /// Enable the Multi-Factor Authentication
        /// </summary>
        private async Task<bool> EnableMultiFactorAuthAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;

            var enableMultiFactorAuth = new MultiFactorAuthEnableRequestListenerAsync();
            var result = await enableMultiFactorAuth.ExecuteAsync(() =>
                SdkService.MegaSdk.multiFactorAuthEnable(code, enableMultiFactorAuth));

            if (!result)
            {
                DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();
                return result;
            }

            DialogService.ShowMultiFactorAuthEnabledDialog();

            NavigateService.Instance.Navigate(typeof(SettingsPage), false,
                NavigationObject.Create(typeof(MultiFactorAuthAppSetupViewModel),
                NavigationActionType.SecuritySettings));

            return result;
        }

        private ObservableCollection<string> SplitMultiFactorAuthCode(string str, int chunkSize)
        {
            if (string.IsNullOrWhiteSpace(str)) return new ObservableCollection<string>();

            var parts = new ObservableCollection<string>(
                Enumerable.Range(0, str.Length / chunkSize).Select(i => str.Substring(i * chunkSize, chunkSize)));
            parts.Insert(10, string.Empty); //For a correct alignment of the three last blocks
            return parts;
        }

        #region Properties

        private WriteableBitmap _qrImage;
        /// <summary>
        /// Image of the QR code to set up the Multi-Factor Authentication
        /// </summary>
        public WriteableBitmap QRImage
        {
            get { return _qrImage; }
            set { SetField(ref _qrImage, value); }
        }

        private string _multiFactorAuthCode;
        /// <summary>
        /// Code or seed needed to enable the Multi-Factor Authentication
        /// </summary>
        public string MultiFactorAuthCode
        {
            get { return _multiFactorAuthCode; }
            set
            {
                if (!SetField(ref _multiFactorAuthCode, value)) return;
                OnPropertyChanged(nameof(this.MultiFactorAuthCodeParts));
            }
        }

        /// <summary>
        /// Code or seed needed to enable the Multi-Factor Authentication
        /// divided in 4-digits groups
        /// </summary>
        public ObservableCollection<string> MultiFactorAuthCodeParts =>
            SplitMultiFactorAuthCode(MultiFactorAuthCode, 4);

        private string codeURI => string.Format("otpauth://totp/MEGA:{0}?secret={1}&issuer=MEGA",
                SdkService.MegaSdk.getMyEmail(), this.MultiFactorAuthCode);

        #endregion

        #region UiResources

        public string CopyText => ResourceService.UiResources.GetString("UI_Copy");
        public string CopySeedText => ResourceService.UiResources.GetString("UI_CopySeed");
        public string SetupStep1Text => ResourceService.UiResources.GetString("UI_MFA_SetupStep1");
        public string SetupStep2Text => ResourceService.UiResources.GetString("UI_MFA_SetupStep2");
        public string NextText => ResourceService.UiResources.GetString("UI_Next");
        public string SectionNameText => ResourceService.UiResources.GetString("UI_SecuritySettings");
        public string TwoFactorAuthText => ResourceService.UiResources.GetString("UI_TwoFactorAuth");
        public string OpenInText => ResourceService.UiResources.GetString("UI_OpenIn");

        #endregion
    }
}
