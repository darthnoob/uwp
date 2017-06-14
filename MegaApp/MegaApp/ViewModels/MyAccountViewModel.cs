using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class MyAccountViewModel : BaseSdkViewModel
    {
        public MyAccountViewModel()
        {
            this.LogOutCommand = new RelayCommand(LogOut);
        }

        #region Commands

        public ICommand LogOutCommand { get; }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            
        }

        #endregion

        #region Private Methods

        private void LogOut()
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            this.MegaSdk.logout(new LogOutRequestListener());
        }

        #endregion

        #region UiResources

        public string LogOutText => ResourceService.UiResources.GetString("UI_Logout");

        #endregion
    }
}
