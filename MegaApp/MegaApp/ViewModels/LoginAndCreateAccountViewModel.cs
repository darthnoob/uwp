using System.Collections;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class LoginAndCreateAccountViewModel : BaseSdkViewModel
    {
        public LoginViewModel LoginViewModel { get; private set; }
        public CreateAccountViewModel CreateAccountViewModel { get; private set; }

        public LoginAndCreateAccountViewModel()
        {
            this.LoginViewModel = new LoginViewModel();
            this.CreateAccountViewModel = new CreateAccountViewModel();
        }

        #region UiResources

        public string ConfirmText => ResourceService.UiResources.GetString("UI_Confirm");

        #endregion
    }
}
