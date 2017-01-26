using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class LoginAndCreateAccountViewModel : BaseSdkViewModel
    {
        public LoginViewModel LoginViewModel { get; }
        public CreateAccountViewModel CreateAccountViewModel { get; }

        public LoginAndCreateAccountViewModel()
        {
            this.LoginViewModel = new LoginViewModel();
            this.CreateAccountViewModel = new CreateAccountViewModel();
            this.ActiveViewModel = this.LoginViewModel;
        }


        private BaseSdkViewModel _activeViewModel;
        public BaseSdkViewModel ActiveViewModel
        {
            get { return _activeViewModel; }
            set { SetField(ref _activeViewModel, value); }
        }

        #region UiResources

        public string ConfirmText => ResourceService.UiResources.GetString("UI_Confirm");

        #endregion
    }
}
