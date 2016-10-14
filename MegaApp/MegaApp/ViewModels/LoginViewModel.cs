using System.Threading.Tasks;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class LoginViewModel : BaseSdkViewModel
    {
        public LoginViewModel()
        {
            this.ControlState = true;
        }

        #region Methods

        public async void DoLogin()
        {
            if (await CheckInputParameters())
                this.MegaSdk.login(this.Email, this.Password, new LoginRequestListener(this));
            //else if (_loginAndCreateAccountPage != null)
            //    Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
        }

        private async Task<bool> CheckInputParameters()
        {
            if (string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.Password))
            {
                await this.OnUiThread(() =>
                {
                    new CustomMessageDialog(
                            ResourceService.AppMessages.GetString("AM_RequiredFields_Title"),
                            ResourceService.AppMessages.GetString("AM_RequiredFieldsLogin"),
                            App.AppInformation,
                            MessageDialogButtons.Ok)
                        .ShowDialogAsync();
                });
                
                return false;
            }
            
            if(!ValidationService.IsValidEmail(this.Email))
            {
                await this.OnUiThread(() =>
                {
                    new CustomMessageDialog(
                            ResourceService.AppMessages.GetString("AM_LoginFailed_Title"),
                            ResourceService.AppMessages.GetString("AM_MalformedEmail"),
                            App.AppInformation,
                            MessageDialogButtons.Ok)
                        .ShowDialogAsync();
                });

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

        #region UI Strings Resources

        public string UI_Login { get { return ResourceService.UiResources.GetString("UI_Login"); } }
        public string UI_EmailWatermark { get { return ResourceService.UiResources.GetString("UI_EmailWatermark"); } }
        public string UI_PasswordWatermark { get { return ResourceService.UiResources.GetString("UI_PasswordWatermark"); } }

        #endregion
    }
}
