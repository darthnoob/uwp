using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class ChangeToStagingServerDialogViewModel : BaseContentDialogViewModel
    {
        public ChangeToStagingServerDialogViewModel() : base()
        {
            this.OkButtonCommand = new RelayCommand(ChangeToStagingServer);
            this.CancelButtonCommand = new RelayCommand(Cancel);

            this.TitleText = ResourceService.AppMessages.GetString("AM_ChangeToStagingServer_Title");
            this.MessageText = ResourceService.AppMessages.GetString("AM_ChangeToStagingServer");
        }

        #region Commands

        public ICommand OkButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Cancels the change API URL process
        /// </summary>
        private void Cancel() => this.OnHideDialog();

        /// <summary>
        /// Changes the API URL
        /// </summary>
        private void ChangeToStagingServer()
        {
            this.DialogResult = true;

            if (this.UseSpecificPort)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Changing API URL to staging server (port 444)...");
                SettingsService.Save(ResourceService.SettingsResources.GetString("SR_UseStagingServer"), false);
                SettingsService.Save(ResourceService.SettingsResources.GetString("SR_UseStagingServerPort444"), true);
                SdkService.MegaSdk.changeApiUrl(ResourceService.AppResources.GetString("AR_StagingUrlPort444"), true);
                SdkService.MegaSdkFolderLinks.changeApiUrl(ResourceService.AppResources.GetString("AR_StagingUrlPort444"), true);
            }
            else
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Changing API URL to staging server...");
                SettingsService.Save(ResourceService.SettingsResources.GetString("SR_UseStagingServer"), true);
                SettingsService.Save(ResourceService.SettingsResources.GetString("SR_UseStagingServerPort444"), false);
                SdkService.MegaSdk.changeApiUrl(ResourceService.AppResources.GetString("AR_StagingUrl"));
                SdkService.MegaSdkFolderLinks.changeApiUrl(ResourceService.AppResources.GetString("AR_StagingUrl"));
            }

            this.OnHideDialog();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Result value of the dialog.
        /// TRUE if the user continues with the change API URL process or FALSE in other case.
        /// </summary>
        public bool DialogResult = false;

        private bool _useSpecificPort;
        /// <summary>
        /// Store the "Use port (444)" checkbox value
        /// </summary>
        public bool UseSpecificPort
        {
            get { return _useSpecificPort; }
            set { SetField(ref _useSpecificPort, value); }
        }

        #endregion

        #region UiResources

        public string OkText => ResourceService.UiResources.GetString("UI_Ok");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string UseSpecificPortText => string.Format(ResourceService.UiResources.GetString("UI_UseSpecificPort"), 444);

        #endregion
    }
}
