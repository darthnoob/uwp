using System.Collections.Generic;
using MegaApp.Services;
using MegaApp.ViewModels.Settings;

namespace MegaApp.ViewModels
{
    public class SettingsViewModel : BaseSdkViewModel
    {
        public SettingsViewModel() : base(SdkService.MegaSdk)
        {
            // General section
            this.GeneralSettingSections = new List<SettingSectionViewModel>();
                        
            var aboutSettings = new SettingSectionViewModel
            {
                Header = ResourceService.UiResources.GetString("UI_About")
            };
            aboutSettings.Items.Add(new DescriptionSettingViewModel(null,
                ResourceService.UiResources.GetString("UI_AboutDescription")));
            aboutSettings.Items.Add(new AppVersionSettingViewModel());
            aboutSettings.Items.Add(new SdkVersionSettingViewModel());
            aboutSettings.Items.Add(new AcknowledgementsSettingViewModel());

            this.GeneralSettingSections.Add(aboutSettings);

            var legalSettings = new SettingSectionViewModel
            {
                Header = ResourceService.UiResources.GetString("UI_LegalAndPolicies")
            };
            this.LegalAndPoliciesSetting = new LegalAndPoliciesSettingViewModel();
            legalSettings.Items.Add(this.LegalAndPoliciesSetting);

            this.GeneralSettingSections.Add(legalSettings);

            // Camera uploads section
            this.CameraUploadSettingSections = new List<SettingSectionViewModel>();

            var cameraUploadSettings = new SettingSectionViewModel()
            {
                Header = ResourceService.UiResources.GetString("UI_CameraUploads")
            };
            cameraUploadSettings.Items.Add(new CameraUploadsSettingViewModel());

            this.CameraUploadSettingSections.Add(cameraUploadSettings);

            // Security section
            this.SecuritySettingSections = new List<SettingSectionViewModel>();

            var recoveryKeySettings = new SettingSectionViewModel
            {
                Header = ResourceService.UiResources.GetString("UI_RecoveryKey")
            };
            recoveryKeySettings.Items.Add(SettingsService.RecoveryKeySetting);

            this.SecuritySettingSections.Add(recoveryKeySettings);
        }

        public void Initialize()
        {
            foreach (var settingSection in this.GeneralSettingSections)
                settingSection.Initialize();

            foreach (var settingSection in this.CameraUploadSettingSections)
                settingSection.Initialize();

            foreach (var settingSection in this.SecuritySettingSections)
                settingSection.Initialize();
        }

        public override void UpdateNetworkStatus()
        {
            base.UpdateNetworkStatus();
            SettingsService.RecoveryKeySetting.UpdateNetworkStatus();
        }

        #region Properties

        public List<SettingSectionViewModel> GeneralSettingSections { get; }
        public List<SettingSectionViewModel> CameraUploadSettingSections { get; }
        public List<SettingSectionViewModel> SecuritySettingSections { get; }

        public LegalAndPoliciesSettingViewModel LegalAndPoliciesSetting { get; }

        #endregion

        #region UiResources

        public string SectionNameText => ResourceService.UiResources.GetString("UI_Settings");

        public string GeneralHeaderText => ResourceService.UiResources.GetString("UI_General");

        public string CameraUploadsHeaderText => ResourceService.UiResources.GetString("UI_CameraUploads");
        public string CameraUploadsDescriptionText => ResourceService.UiResources.GetString("UI_CameraUploadsDescription");

        public string SecurityHeaderText => ResourceService.UiResources.GetString("UI_SecuritySettings");
        public string SecurityDescriptionText => ResourceService.UiResources.GetString("UI_SecuritySettingsDescription");

        public string OnText => ResourceService.UiResources.GetString("UI_On");
        public string OffText => ResourceService.UiResources.GetString("UI_Off");

        #endregion
    }
}
