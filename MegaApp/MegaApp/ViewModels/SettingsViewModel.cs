using System.Collections.Generic;
using MegaApp.Services;
using MegaApp.ViewModels.Settings;

namespace MegaApp.ViewModels
{
    public class SettingsViewModel: BaseSdkViewModel
    {
        public SettingsViewModel() : base(SdkService.MegaSdk)
        {
            this.SettingSections = new List<SettingSectionViewModel>();

            // Camera uploads section
            var cameraUploadSettings = new SettingSectionViewModel
            {
                Header = ResourceService.UiResources.GetString("UI_CameraUploads"),
            };
            cameraUploadSettings.Items.Add(new DescriptionSettingViewModel(null,
                ResourceService.UiResources.GetString("UI_CameraUploadsDescription")));
            cameraUploadSettings.Items.Add(new CameraUploadsSettingViewModel());

            // Security section
            var securitySettings = new SettingSectionViewModel
            {
                Header = ResourceService.UiResources.GetString("UI_SecuritySettings")
            };
            securitySettings.Items.Add(new DescriptionSettingViewModel(null,
               ResourceService.UiResources.GetString("UI_SecuritySettingsDescription")));
            securitySettings.Items.Add(new RecoveryKeySettingViewModel());

            // About section
            var aboutSettings = new SettingSectionViewModel
            {
                Header = ResourceService.UiResources.GetString("UI_About")
            };
            aboutSettings.Items.Add(new DescriptionSettingViewModel(null,
               ResourceService.UiResources.GetString("UI_AboutDescription")));
            aboutSettings.Items.Add(new AppVersionSettingViewModel());
            aboutSettings.Items.Add(new SdkVersionSettingViewModel());
            aboutSettings.Items.Add(new AcknowledgementsSettingViewModel());

            this.SettingSections.Add(cameraUploadSettings);
            this.SettingSections.Add(securitySettings);
            this.SettingSections.Add(aboutSettings);
        }

        public void Initialize()
        {
            foreach (var settingSection in this.SettingSections)
            {
                settingSection.Initialize();
            }
        }       

        #region Properties

        public IList<SettingSectionViewModel> SettingSections { get; }

        #endregion

        #region UiResources

        public string OnText => ResourceService.UiResources.GetString("UI_On");
        public string OffText => ResourceService.UiResources.GetString("UI_Off");

        #endregion
    }
}
