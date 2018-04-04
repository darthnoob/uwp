using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels.Dialogs
{
    public class TransferOverquotaWarningDialogViewModel : BaseContentDialogViewModel
    {
        public TransferOverquotaWarningDialogViewModel() : base()
        {
            this.UpgradeButtonCommand = new RelayCommand(Upgrade);
            this.WaitButtonCommand = new RelayCommand(Wait);

            AccountService.AccountDetails.TimerTransferOverquotaChanged += (sender, args) =>
                OnPropertyChanged(nameof(this.WaitMessageText));

            if (string.IsNullOrWhiteSpace(AccountService.UpgradeAccount.LiteMonthlyFormattedPrice))
                this.GetLiteMonthlyFormattedPrice();

            AccountService.GetTransferOverquotaDetails();
        }

        #region Commands

        public ICommand UpgradeButtonCommand { get; }
        public ICommand WaitButtonCommand { get; }

        #endregion

        #region Private Methods

        private void Upgrade()
        {
            NavigateService.Instance.Navigate(typeof(MyAccountPage), false,
                NavigationObject.Create(typeof(MainViewModel), NavigationActionType.Upgrade));
        }

        private void Wait()
        {
            
        }

        private async void GetLiteMonthlyFormattedPrice()
        {
            AccountService.UpgradeAccount.LiteMonthlyFormattedPrice = await AccountService.GetLiteMonthlyFormattedPrice();
            OnPropertyChanged(nameof(this.UpgradeMessageText));
        }

        #endregion
        
        #region UiResources

        public string TitleText => ResourceService.AppMessages.GetString("AM_TransferOverquotaWarning_Title");
        public string MessageText => ResourceService.AppMessages.GetString("AM_TransferOverquotaWarning");
        public string UpgradeMessageText => string.Format(ResourceService.AppMessages.GetString("AM_TransferOverquotaWarningUpgrade"),
            AccountService.UpgradeAccount.LiteMonthlyFormattedPrice);
        public string WaitMessageText => string.Format(ResourceService.AppMessages.GetString("AM_TransferOverquotaWarningWaitTime"),
            AccountService.AccountDetails.TransferOverquotaDelayText);

        public string UpgradeText => ResourceService.UiResources.GetString("UI_Upgrade");
        public string WaitText => ResourceService.UiResources.GetString("UI_Wait");

        #endregion

        #region VisualResources

        public string ProLitePathData => ResourceService.VisualResources.GetString("VR_AccountTypeProLitePathData");

        #endregion
    }
}
