using Windows.UI;
using Windows.UI.Xaml;
using MegaApp.Services;

namespace MegaApp.ViewModels.MyAccount
{
    public class StorageAndTransferViewModel : MyAccountBaseViewModel
    {
        public StorageAndTransferViewModel()
        {
            this.AccountDetails.CloudDriveUsedSpaceChanged += (sender, args) => UpdateProgressBarValues();
            this.AccountDetails.IncomingSharesUsedSpaceChanged += (sender, args) => UpdateProgressBarValues();
            this.AccountDetails.RubbishBinUsedSpaceChanged += (sender, args) => UpdateProgressBarValues();
            this.AccountDetails.IsInStorageOverquotaChanged += (sender, args) => ChangeColors();
        }

        private void UpdateProgressBarValues()
        {
            this.CloudDriveUsedSpaceProgressVarValue = this.AccountDetails.CloudDriveUsedSpace;
            this.IncomingSharesUsedSpaceProgressVarValue = this.CloudDriveUsedSpaceProgressVarValue
                + this.AccountDetails.IncomingSharesUsedSpace;
            this.RubbishBinUsedSpaceProgressVarValue = this.IncomingSharesUsedSpaceProgressVarValue
                + this.AccountDetails.RubbishBinUsedSpace;
        }

        private void ChangeColors()
        {
            OnPropertyChanged("IncomingSharesColor");
            OnPropertyChanged("RubbishBinColor");
        }

        #region Properties

        private ulong _cloudDriveUsedSpaceProgressVarValue;
        public ulong CloudDriveUsedSpaceProgressVarValue
        {
            get { return _cloudDriveUsedSpaceProgressVarValue; }
            set { SetField(ref _cloudDriveUsedSpaceProgressVarValue, value); }
        }

        private ulong _incomingSharesUsedSpaceProgressVarValue;
        public ulong IncomingSharesUsedSpaceProgressVarValue
        {
            get { return _incomingSharesUsedSpaceProgressVarValue; }
            set { SetField(ref _incomingSharesUsedSpaceProgressVarValue, value); }
        }

        public Color IncomingSharesColor => AccountDetails.IsInStorageOverquota ? 
            (Color)Application.Current.Resources["MegaRedColor"] :
            (Color)Application.Current.Resources["IncomingSharesColor"];

        private ulong _rubbishBinUsedSpaceProgressVarValue;
        public ulong RubbishBinUsedSpaceProgressVarValue
        {
            get { return _rubbishBinUsedSpaceProgressVarValue; }
            set { SetField(ref _rubbishBinUsedSpaceProgressVarValue, value); }
        }

        public Color RubbishBinColor => AccountDetails.IsInStorageOverquota ? 
            (Color)Application.Current.Resources["MegaRedColor"] :
            (Color)Application.Current.Resources["RubbishBinColor"];

        #endregion

        #region UiResources

        // Title
        public string Title => ResourceService.UiResources.GetString("UI_StorageAndTransfer");

        // Used storage
        public string UsedStorageTitle => ResourceService.UiResources.GetString("UI_UsedStorage");
        public string TotalStorageText => ResourceService.UiResources.GetString("UI_TotalStorage");
        public string UsedStorageText => ResourceService.UiResources.GetString("UI_UsedStorage");
        public string AvailableStorageText => ResourceService.UiResources.GetString("UI_AvailableStorage");
        public string CloudDriveText => ResourceService.UiResources.GetString("UI_CloudDriveName");
        public string IncomingSharesText => ResourceService.UiResources.GetString("UI_IncomingShares");

        // Transfer quota
        public string TransferQuotaTitle => ResourceService.UiResources.GetString("UI_TransferQuota");
        public string TotalTransferQuotaText => ResourceService.UiResources.GetString("UI_TotalTransferQuota");
        public string UsedTransferQuotaText => ResourceService.UiResources.GetString("UI_UsedTransferQuota");
        public string AvailableTransferQuotaText => ResourceService.UiResources.GetString("UI_AvailableTransferQuota");

        #endregion
    }
}
