using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class SettingsMyAccountViewModel: BaseSdkViewModel
    {
        public SettingsMyAccountViewModel()
        {
            this.Settings = new SettingsViewModel();
        }

        #region Public Methods

        public void Initialize()
        {
            this.Settings.Initialize();
        }

        #endregion

        #region Properties

        public SettingsViewModel Settings { get; }

        #endregion

        #region UiResources

        public string OnText => ResourceService.UiResources.GetString("UI_On");
        public string OffText => ResourceService.UiResources.GetString("UI_Off");

        #endregion
    }
}
