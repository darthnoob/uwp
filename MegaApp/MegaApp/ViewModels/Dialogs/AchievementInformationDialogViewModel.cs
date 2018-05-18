using System;
using System.Windows.Input;
using Windows.System;
using mega;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.ViewModels.Dialogs
{
    public class AchievementInformationDialogViewModel: BaseContentDialogViewModel
    {
        public AchievementInformationDialogViewModel()
        {
            this.InstallButtonCommand = new RelayCommand(Install);
            this.CloseButtonCommand = new RelayCommand(Close);
        }

        #region Methods

        private void Close()
        {
            OnClosed();
        }

        private async void Install()
        {
            switch (this.Award.AchievementClass)
            {
                case MAchievementClass.MEGA_ACHIEVEMENT_DESKTOP_INSTALL:
                {
                    await Launcher.LaunchUriAsync(new Uri(ResourceService.AppResources.GetString("AR_MegaSyncUrl")));
                    break;
                }
                case MAchievementClass.MEGA_ACHIEVEMENT_MOBILE_INSTALL:
                {
                    await Launcher.LaunchUriAsync(new Uri(ResourceService.AppResources.GetString("AR_MobileAppsUrl")));
                    break;
                }
            }

            OnClosed();
        }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the user saves the email change.
        /// </summary>
        public event EventHandler Closed;

        protected virtual void OnClosed()
        {
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Commands

        public ICommand InstallButtonCommand { get; }
        public ICommand CloseButtonCommand { get; }

        #endregion

        #region Properties
        

        private AwardClassViewModel _award;
        public AwardClassViewModel Award
        {
            get { return _award; }
            set { SetField(ref _award, value); }
        }

        #endregion

        #region UiResources

        public string StorageText => ResourceService.UiResources.GetString("UI_Storage");
        public string TransferText => ResourceService.UiResources.GetString("UI_Transfer");
        public string InstallText => ResourceService.UiResources.GetString("UI_Install");
        public string AchievedOnText => ResourceService.UiResources.GetString("UI_AchievedOn");
        public string ExpiresInText => ResourceService.UiResources.GetString("UI_ExpiresIn");

        #endregion
    }
}
