using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using mega;
using MegaApp.Services;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.ViewModels
{
    class DebugSettingsViewModel : BaseSdkViewModel
    {
        public DebugSettingsViewModel() : base(SdkService.MegaSdk)
        {
            this.Initialize();
        }

        #region Methods

        private async void Initialize()
        {
            this._isDebugMode = await SettingsService.LoadSettingAsync(ResourceService.SettingsResources.GetString("SR_DebugModeIsEnabled"), false);

            if (this._isDebugMode)
            {
                ShowDebugAlert = true;
                LogService.SetLogLevel(MLogLevel.LOG_LEVEL_MAX);
            }
            else
            {
                ShowDebugAlert = false;
                LogService.SetLogLevel(MLogLevel.LOG_LEVEL_DEBUG);
            }
        }

        /// <summary>
        /// Method to enable the DEBUG mode.
        /// <para>Saves the setting, sets the log level and creates the log file.</para>
        /// </summary>
        public void EnableDebugMode()
        {
            this._isDebugMode = true;
            SettingsService.Save(ResourceService.SettingsResources.GetString("SR_DebugModeIsEnabled"), true);
            LogService.SetLogLevel(MLogLevel.LOG_LEVEL_MAX);
            try
            {
                using (StreamWriter sw = File.AppendText(AppService.GetFileLogPath()))
                {
                    sw.WriteLine(string.Format("######## LOG FILE - {0} - VERSION: {1} ########",
                        AppService.GetAppName(), AppService.GetAppVersion()));
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                /* No problem, because it will be created when writes the first line if not exists. */
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error creating the log file", e);
            }
            finally
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Enabling DEBUG mode");
            }
        }

        /// <summary>
        /// Method to disable the DEBUG mode.
        /// <para>Saves the setting, sets the log level and creates the log file.</para>
        /// </summary>
        public async void DisableDebugMode()
        {
            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Disabling DEBUG mode");

            this._isDebugMode = false;
            SettingsService.Save(ResourceService.SettingsResources.GetString("SR_DebugModeIsEnabled"), false);
            LogService.SetLogLevel(MLogLevel.LOG_LEVEL_DEBUG);

            // Need force to false to show the options dialog
            App.AppInformation.PickerOrAsyncDialogIsOpen = false;

            // Only ask to save the log file if it contents anything
            var logFile = await StorageFile.GetFileFromPathAsync(AppService.GetFileLogPath());
            var basicPropertiesAsync = logFile?.GetBasicPropertiesAsync();
            if (basicPropertiesAsync != null && (await basicPropertiesAsync).Size > 0)
            {
                var result = await DialogService.ShowOkCancelAsync(
                    ResourceService.AppMessages.GetString("AM_SaveLogFile_Title"),
                    ResourceService.AppMessages.GetString("AM_SaveLogFile"),
                    OkCancelDialogButtons.YesNo);

                if (result)
                {
                    // Ask the user a save location
                    var folder = await FolderService.SelectFolder();
                    if (folder != null)
                    {
                        await Task.Run(async () => await FileService.MoveFile(AppService.GetFileLogPath(), folder.Path));
                        return;
                    }
                }
            }

            FileService.DeleteFile(AppService.GetFileLogPath());
        }

        #endregion

        #region Properties

        private bool _isDebugMode;
        public bool IsDebugMode
        {
            get { return _isDebugMode; }
            set
            {
                if (_isDebugMode == value) return;

                string message, title = string.Empty;
                if (value)
                {
                    title = ResourceService.AppMessages.GetString("AM_EnableDebugMode_Title");
                    message = string.Format(ResourceService.AppMessages.GetString("AM_EnableDebugMode_Message"),
                        ResourceService.AppResources.GetString("AR_LogFileName"));
                }
                else
                {
                    title = ResourceService.AppMessages.GetString("AM_DisableDebugMode_Title");
                    message = string.Format(ResourceService.AppMessages.GetString("AM_DisableDebugMode_Message"),
                        ResourceService.AppResources.GetString("AR_LogFileName"));
                }

                OnUiThread(async () =>
                {
                    var result = await DialogService.ShowOkCancelAsync(title, message);
                    if (!result) return;

                    if (value)
                        EnableDebugMode();
                    else
                        DisableDebugMode();

                    SetField(ref _isDebugMode, value);
                });
            }
        }

        /// <summary>
        /// Boolean property to indicate if is necessary to show the DEBUG alert.
        /// <para>Is set to TRUE during the app launching if the DEBUG mode is enabled.
        /// Once the DEBUG alert has been shown, is set to FALSE.</para>
        /// </summary>
        private bool _showDebugAlert;
        public bool ShowDebugAlert
        {
            get { return _showDebugAlert; }
            set { SetField(ref _showDebugAlert, value); }
        }

        #endregion
    }
}
