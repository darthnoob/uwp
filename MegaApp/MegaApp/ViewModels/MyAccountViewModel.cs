using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class MyAccountViewModel : BaseSdkViewModel
    {
        public MyAccountViewModel() : base(SdkService.MegaSdk)
        {
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

        private async void LogOut()
        {
            if (!await NetworkService.IsNetworkAvailableAsync(true)) return;

            this.MegaSdk.logout(new LogOutRequestListener());
        }

        #endregion

        #region UiResources

        public string SectionNameText => ResourceService.UiResources.GetString("UI_MyAccount");

        public string GeneralTitle => ResourceService.UiResources.GetString("UI_General");
        public string ProfileTitle => ResourceService.UiResources.GetString("UI_Profile");
        public string StorageAndTransferTitle => ResourceService.UiResources.GetString("UI_StorageAndTransfer");
        public string UpgradeTitle => ResourceService.UiResources.GetString("UI_Upgrade");

        public string LogOutText => ResourceService.UiResources.GetString("UI_Logout");

        #endregion
    }
}
