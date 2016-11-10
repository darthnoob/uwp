using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    public class LoginViewModel : BaseSdkViewModel
    {
        public LoginViewModel()
        {
            this.ControlState = true;
        }

        #region Methods

        public void DoLogin()
        {
            if (CheckInputParameters())
                this.MegaSdk.login(this.Email, this.Password, new LoginRequestListener(this));
            //else if (_loginAndCreateAccountPage != null)
            //    Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
        }

        private bool CheckInputParameters()
        {
            if (string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.Password))
            {
                new CustomMessageDialog(
                    ResourceService.AppMessages.GetString("AM_RequiredFields_Title"),
                    ResourceService.AppMessages.GetString("AM_RequiredFieldsLogin"),
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
                
                return false;
            }
            
            if(!ValidationService.IsValidEmail(this.Email))
            {
                new CustomMessageDialog(
                    ResourceService.AppMessages.GetString("AM_LoginFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_MalformedEmail"),
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();

                return false;
            }

            return true;
        }

        private static void SaveLoginData(string email, string session)
        {
            SettingsService.SaveMegaLoginData(email, session);
        }
        
        #endregion
        
        #region Properties

        public string Email { get; set; }
        public string Password { get; set; }
        public string SessionKey { get; set; }

        #endregion

        #region UiResources

        public string EmailWatermarkText { get { return ResourceService.UiResources.GetString("UI_EmailWatermark"); } }
        public string LoginText { get { return ResourceService.UiResources.GetString("UI_Login"); } }
        public string PasswordWatermarkText { get { return ResourceService.UiResources.GetString("UI_PasswordWatermark"); } }

        #endregion
    }
}
