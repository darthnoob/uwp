using System.Collections.Generic;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.Dialogs;
using MegaApp.ViewModels.Settings;

namespace MegaApp.ViewModels
{
    public class SettingsViewModel : BaseSdkViewModel
    {
        public SettingsViewModel() : base(SdkService.MegaSdk)
        {
            // General section
            this.GeneralSettingSections = new List<SettingSectionViewModel>();

            var storageLocationSettings = new SettingSectionViewModel
            {
                Header = ResourceService.UiResources.GetString("UI_StorageLocation")
            };
            storageLocationSettings.Items.Add(new ClearCacheSettingViewModel());

            this.GeneralSettingSections.Add(storageLocationSettings);

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

            var cameraUploads = new CameraUploadsSettingViewModel();
            cameraUploads.Initialize();
            cameraUploadSettings.Items.Add(cameraUploads);

            var howCameraUploads = new CameraUploadsSelectionSettingViewModel(
                ResourceService.UiResources.GetString("UI_HowToUpload"), null, "CameraUploadsSettingsHowKey",
                    new[]
                    {
                            new SelectionSettingViewModel.SelectionOption
                            {
                                    Description = ResourceService.UiResources.GetString("UI_EthernetWifiOnly"),
                                    Value = (int) CameraUploadsConnectionType.EthernetWifiOnly
                            },
                            new SelectionSettingViewModel.SelectionOption
                            {
                                    Description = ResourceService.UiResources.GetString("UI_AnyConnectionType"),
                                    Value = (int) CameraUploadsConnectionType.Any
                            }
                    }) {IsVisible = cameraUploads.Value};
            cameraUploadSettings.Items.Add(howCameraUploads);

            var fileCameraUploads = new CameraUploadsSelectionSettingViewModel(
                ResourceService.UiResources.GetString("UI_FileToUpload"), null, "CameraUploadsSettingsFileKey",
                    new[]
                    {
                            new SelectionSettingViewModel.SelectionOption
                            {
                                    Description = ResourceService.UiResources.GetString("UI_PhotoAndVideo"),
                                    Value = (int) CameraUploadsFileType.PhotoAndVideo
                            },
                            new SelectionSettingViewModel.SelectionOption
                            {
                                    Description = ResourceService.UiResources.GetString("UI_PhotoOnly"),
                                    Value = (int) CameraUploadsFileType.PhotoOnly
                            },
                            new SelectionSettingViewModel.SelectionOption
                            {
                                    Description = ResourceService.UiResources.GetString("UI_VideoOnly"),
                                    Value = (int) CameraUploadsFileType.VideoOnly
                            }
                    })
                    { IsVisible = cameraUploads.Value };

            cameraUploadSettings.Items.Add(fileCameraUploads);

            cameraUploads.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != "Value") return;

                howCameraUploads.IsVisible = cameraUploads.Value;
                fileCameraUploads.IsVisible = cameraUploads.Value;
            };

            this.CameraUploadSettingSections.Add(cameraUploadSettings);

            // Security section
            this.SecuritySettingSections = new List<SettingSectionViewModel>();

            var recoveryKeySettings = new SettingSectionViewModel
            {
                Header = ResourceService.UiResources.GetString("UI_RecoveryKey")
            };
            recoveryKeySettings.Items.Add(SettingsService.RecoveryKeySetting);

            this.SecuritySettingSections.Add(recoveryKeySettings);

            var sessionManagementSettings = new SettingSectionViewModel
            {
                Header = ResourceService.UiResources.GetString("UI_SessionManagement")
            };

            var closeOtherSessionsSetting = new ButtonSettingViewModel(null,
                ResourceService.UiResources.GetString("UI_SessionManagementDescription"),
                ResourceService.UiResources.GetString("UI_CloseOtherSessions"),
                this.CloseOtherSessions);
            sessionManagementSettings.Items.Add(closeOtherSessionsSetting);

            this.SecuritySettingSections.Add(sessionManagementSettings);
        }

        #region Methods

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

        private async void CloseOtherSessions()
        {
            var result = await DialogService.ShowOkCancelAsync(
                ResourceService.UiResources.GetString("UI_Warning"),
                ResourceService.AppMessages.GetString("AM_CloseOtherSessionsQuestionMessage"),
                TwoButtonsDialogType.YesNo);

            if (!result) return;

            var killAllSessions = new KillAllSessionsListenerAsync();
            result = await killAllSessions.ExecuteAsync(() =>
                this.MegaSdk.killAllSessions(killAllSessions));

            if (!result)
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.UiResources.GetString("UI_Warning"),
                    ResourceService.AppMessages.GetString("AM_CloseOtherSessionsFailed"));
                return;
            }

            ToastService.ShowTextNotification(
                ResourceService.UiResources.GetString("UI_CloseOtherSessions"),
                ResourceService.AppMessages.GetString("AM_CloseOtherSessionsSuccess"));
        }

        #endregion

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
