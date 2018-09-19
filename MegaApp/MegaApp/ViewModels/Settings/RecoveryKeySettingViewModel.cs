using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class RecoveryKeySettingViewModel : SettingViewModel<string>
    {
        public RecoveryKeySettingViewModel()
            : base(null, ResourceService.UiResources.GetString("UI_RecoveryKeyDescription"),
                "CameraUploadsSettingsKey")
        {
            this.CopyKeyCommand = new RelayCommand(CopyKey);
            this.SaveKeyCommand = new RelayCommandAsync<bool>(SaveKey);
        }

        #region Private Methods

        private async Task<bool> SaveKey()
        {
            var file = await FileService.SaveFile(new KeyValuePair<string, IList<string>>(
                ResourceService.UiResources.GetString("UI_PlainText"), new List<string>() { ".txt" }),
                ResourceService.AppResources.GetString("AR_RecoveryKeyFileName"));
            if (file == null) return false;
            try
            {
                await FileIO.WriteTextAsync(file, this.Value);
                SdkService.MegaSdk.masterKeyExported();
                ToastService.ShowTextNotification(
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyExported_Title"),
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyExported"));
                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);

                if (DialogService.IsAnyDialogVisible())
                {
                    ToastService.ShowAlertNotification(
                        ResourceService.AppMessages.GetString("AM_RecoveryKeyExportFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_RecoveryKeyExportFailed"));
                    return false;
                }

                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyExportFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyExportFailed"));
                return false;
            }
        }

        private async void CopyKey()
        {
            var data = new DataPackage();
            data.SetText(this.Value);

            try
            {
                Clipboard.SetContent(data);
                SdkService.MegaSdk.masterKeyExported();
                ToastService.ShowTextNotification(
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyCopied_Title"),
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyCopied"));
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);

                if (DialogService.IsAnyDialogVisible())
                {
                    ToastService.ShowAlertNotification(
                        ResourceService.AppMessages.GetString("AM_RecoveryKeyCopiedFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_RecoveryKeyCopiedFailed"));
                    return;
                }

                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyCopiedFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyCopiedFailed"));
            }
        }

        #endregion

        public override string GetValue(string defaultValue)
        {
            return SdkService.MegaSdk.exportMasterKey();
        }

        #region Commands

        public ICommand CopyKeyCommand { get; }
        public ICommand SaveKeyCommand { get; }

        #endregion

        #region UiResources

        public string CopyKeyText => ResourceService.UiResources.GetString("UI_CopyKey");
        public string SaveKeyText => ResourceService.UiResources.GetString("UI_SaveKey");

        #endregion
    }
}
