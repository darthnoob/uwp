using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class SettingsMyAccountViewModel: BaseSdkViewModel
    {
        public SettingsMyAccountViewModel()
        {
            this.LogOutCommand = new RelayCommand(LogOut);
            this.Settings = new SettingsViewModel();
        }

        #region Commands

        public ICommand LogOutCommand { get; }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            this.Settings.Initialize();
        }

        #endregion

        #region Private Methods

        private void LogOut()
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            this.MegaSdk.logout(new LogOutRequestListener());
        }

        #endregion

        #region Properties

        public SettingsViewModel Settings { get; }

        #endregion

        #region UiResources

        public string LogOutText => ResourceService.UiResources.GetString("UI_Logout");
        public string OnText => ResourceService.UiResources.GetString("UI_On");
        public string OffText => ResourceService.UiResources.GetString("UI_Off");

        #endregion
    }
}
