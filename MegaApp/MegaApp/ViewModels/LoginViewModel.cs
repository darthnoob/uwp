using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class LoginViewModel : BaseSdkViewModel
    {
        private readonly MegaSDK _megaSdk;

        public LoginViewModel(MegaSDK megaSdk)
            :base(megaSdk)
        {
            this._megaSdk = megaSdk;
            this.ControlState = true;
        }

        #region Methods

        public async void DoLogin()
        {
            if (await CheckInputParameters())
                this._megaSdk.login(this.Email, this.Password, new LoginRequestListener(this));
            //else if (_loginAndCreateAccountPage != null)
            //    Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
        }

        private async Task<bool> CheckInputParameters()
        {
            if (String.IsNullOrEmpty(this.Email) || String.IsNullOrEmpty(this.Password))
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        App.ResourceLoaders.AppMessages.GetString("AM_RequiredFields_Title"),
                        App.ResourceLoaders.AppMessages.GetString("AM_RequiredFieldsLogin"),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
                });
                
                return false;
            }
            
            if(!ValidationService.IsValidEmail(this.Email))
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new CustomMessageDialog(
                        App.ResourceLoaders.AppMessages.GetString("AM_LoginFailed_Title"),
                        App.ResourceLoaders.AppMessages.GetString("AM_MalformedEmail"),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialogAsync();
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

        public string UI_Login { get { return App.ResourceLoaders.UiResources.GetString("UI_Login"); } }
        public string UI_EmailWatermark { get { return App.ResourceLoaders.UiResources.GetString("UI_EmailWatermark"); } }
        public string UI_PasswordWatermark { get { return App.ResourceLoaders.UiResources.GetString("UI_PasswordWatermark"); } }

        #endregion
    }
}
