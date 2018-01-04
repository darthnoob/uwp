using MegaApp.Services;
using MegaApp.ViewModels.CreateAccount;
using MegaApp.ViewModels.Login;

namespace MegaApp.ViewModels
{
    public class LoginAndCreateAccountViewModel : BaseSdkViewModel
    {
        public LoginViewModel LoginViewModel { get; }
        public CreateAccountViewModel CreateAccountViewModel { get; }

        public LoginAndCreateAccountViewModel() : base(SdkService.MegaSdk)
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
        
        #region Visual Resources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }
}
