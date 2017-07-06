using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.ViewModels
{
    public class MyAccountViewModel : BaseSdkViewModel
    {
        public EventHandler GoToUpgrade;

        public MyAccountViewModel()
        {
            this.GeneralViewModel = new GeneralViewModel();
            this.GeneralViewModel.GoToUpgrade += (sender, args) =>
                GoToUpgrade?.Invoke(this, EventArgs.Empty);

            this.StorageAndTransferViewModel = new StorageAndTransferViewModel();
            this.StorageAndTransferViewModel.GoToUpgrade += (sender, args) =>
                GoToUpgrade?.Invoke(this, EventArgs.Empty);

            this.UpgradeViewModel = new UpgradeViewModel();

            this.LogOutCommand = new RelayCommand(LogOut);
        }

        #region Commands

        public ICommand LogOutCommand { get; }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            AccountService.GetAccountDetails();
            AccountService.GetPricing();
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

        public GeneralViewModel GeneralViewModel { get; }
        public StorageAndTransferViewModel StorageAndTransferViewModel { get; }
        public UpgradeViewModel UpgradeViewModel { get; }

        #endregion

        #region UiResources

        public string ProfileTitle => ResourceService.UiResources.GetString("UI_Profile");
        
        public string LogOutText => ResourceService.UiResources.GetString("UI_Logout");

        #endregion
    }
}
