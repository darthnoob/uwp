using System;
using System.Collections.Generic;
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
            : base(ResourceService.UiResources.GetString("UI_RecoveryKey"),
                ResourceService.UiResources.GetString("UI_RecoveryKeyDescription"),
                "CameraUploadsSettingsKey")
        {
            this.CopyKeyCommand = new RelayCommand(CopyKey);
            this.SaveKeyCommand = new RelayCommand(SaveKey);
        }

        #region Private Methods

        private async void SaveKey()
        {
            var file = await FileService.SaveFile(new KeyValuePair<string, IList<string>>(
                    ResourceService.UiResources.GetString("UI_PlainText"), 
                    new List<string>() { ".txt" }));
            if (file == null) return;
            try
            {
                await FileIO.WriteTextAsync(file, this.Value);
                SdkService.MegaSdk.masterKeyExported();
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_SavedFile_Title"),
                    string.Format(
                        ResourceService.AppMessages.GetString("AM_SavedFile"),
                        file.Name));
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_SaveFileFailed_Title"),
                    string.Format(
                        ResourceService.AppMessages.GetString("AM_SaveFileFailed"),
                        file.Name));
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
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyCopied_Title"),
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyCopied"));
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyCopiedFailed"),
                    ResourceService.AppMessages.GetString("AM_RecoveryKeyCopiedFailed_Title"));
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
