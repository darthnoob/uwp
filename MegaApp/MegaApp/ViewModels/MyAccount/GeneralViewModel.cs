using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels.MyAccount
{
    public class GeneralViewModel : BaseViewModel
    {
        public EventHandler GoToUpgrade;

        public GeneralViewModel()
        {
            this.RubbishBinCommand = new RelayCommand(RubbishBin);
            this.UpgradeCommand = new RelayCommand(Upgrade);
        }

        #region Commands

        public ICommand RubbishBinCommand { get; }
        public ICommand UpgradeCommand { get; }

        #endregion

        #region Private Methods

        private void RubbishBin()
        {
            UiService.OnUiThread(() =>
            {
                NavigateService.Instance.Navigate(typeof(MainPage), true,
                    NavigationObject.Create(typeof(GeneralViewModel), NavigationActionType.RubbishBin));
            });
        }

        private void Upgrade()
        {
            GoToUpgrade?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public AccountDetailsViewModel AccountDetails => AccountService.AccountDetails;

        #endregion

        #region UiResources

        public string RecycleBinText => ResourceService.UiResources.GetString("UI_RecycleBin");
        public string UpgradeText => ResourceService.UiResources.GetString("UI_Upgrade");

        #endregion
    }
}
