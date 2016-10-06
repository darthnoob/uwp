using System;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Services;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Threading.Tasks;

namespace MegaApp.Models
{
    class LoginViewModel : BaseSdkViewModel
    {
        private readonly MegaSDK _megaSdk;
        private readonly LoginAndCreateAccountPage _loginAndCreateAccountPage;

        public LoginViewModel(MegaSDK megaSdk, LoginAndCreateAccountPage loginAndCreateAccountPage = null)
            :base(megaSdk)
        {
            this._megaSdk = megaSdk;
            this._loginAndCreateAccountPage = loginAndCreateAccountPage;
            this.ControlState = true;
        }

        #region Methods

        public async void DoLogin()
        {
            if (await CheckInputParameters())
                this._megaSdk.login(Email, Password, new LoginRequestListener(this, _loginAndCreateAccountPage));
            //else if (_loginAndCreateAccountPage != null)
            //    Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
        }

        private async Task<bool> CheckInputParameters()
        {
            if (String.IsNullOrEmpty(Email) || String.IsNullOrEmpty(Password))
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
            
            if(!ValidationService.IsValidEmail(Email))
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
