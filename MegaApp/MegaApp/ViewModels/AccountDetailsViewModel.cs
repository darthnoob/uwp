using System;
using Windows.UI;
using Windows.UI.Xaml;
using mega;
using MegaApp.Extensions;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class AccountDetailsViewModel : BaseViewModel
    {
        public EventHandler CloudDriveUsedSpaceChanged;
        public EventHandler IncomingSharesUsedSpaceChanged;
        public EventHandler RubbishBinUsedSpaceChanged;
        public EventHandler IsInStorageOverquotaChanged;

        public EventHandler TimerTransferOverquotaChanged;

        public DispatcherTimer TimerTransferOverquota;

        public AccountDetailsViewModel()
        {
            AccountType = MAccountType.ACCOUNT_TYPE_FREE; // Default value

            UiService.OnUiThread(() =>
            {
                TimerTransferOverquota = new DispatcherTimer();
                TimerTransferOverquota.Tick += TimerTransferOverquotaOnTick;
                TimerTransferOverquota.Interval = new TimeSpan(0, 0, 1);
            });
        }

        private void TimerTransferOverquotaOnTick(object sender, object o)
        {
            TransferOverquotaDelay--;
            TimerTransferOverquotaChanged.Invoke(sender, EventArgs.Empty);
            if(TransferOverquotaDelay == 0)
            {
                UiService.OnUiThread(() => TimerTransferOverquota?.Stop());
                AccountService.GetAccountDetails();
            }
        }

        #region Properties

        private MAccountType _accountType;
        public MAccountType AccountType
        {
            get { return _accountType; }
            set
            {
                SetField(ref _accountType, value);
                OnPropertyChanged("IsProAccount");
                OnPropertyChanged("AccountTypeText");
                OnPropertyChanged("AccountTypePathData");
                OnPropertyChanged("AccountTypePathDataColor");
            }
        }

        public string AccountTypeText
        {
            get
            {
                switch(AccountType)
                {
                    case MAccountType.ACCOUNT_TYPE_FREE:
                        return ResourceService.AppResources.GetString("AR_AccountTypeFree");
                    case MAccountType.ACCOUNT_TYPE_LITE:
                        return ResourceService.AppResources.GetString("AR_AccountTypeLite");
                    case MAccountType.ACCOUNT_TYPE_PROI:
                        return ResourceService.AppResources.GetString("AR_AccountTypePro1");
                    case MAccountType.ACCOUNT_TYPE_PROII:
                        return ResourceService.AppResources.GetString("AR_AccountTypePro2");
                    case MAccountType.ACCOUNT_TYPE_PROIII:
                        return ResourceService.AppResources.GetString("AR_AccountTypePro3");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string AccountTypePathData
        {
            get
            {
                switch (AccountType)
                {
                    case MAccountType.ACCOUNT_TYPE_FREE:
                        return ResourceService.VisualResources.GetString("VR_AccountTypeFreePathData");
                    case MAccountType.ACCOUNT_TYPE_LITE:
                        return ResourceService.VisualResources.GetString("VR_AccountTypeProLitePathData");
                    case MAccountType.ACCOUNT_TYPE_PROI:
                        return ResourceService.VisualResources.GetString("VR_AccountTypePro1PathData");
                    case MAccountType.ACCOUNT_TYPE_PROII:
                        return ResourceService.VisualResources.GetString("VR_AccountTypePro2PathData");
                    case MAccountType.ACCOUNT_TYPE_PROIII:
                        return ResourceService.VisualResources.GetString("VR_AccountTypePro3PathData");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Color AccountTypePathDataColor
        {
            get
            {
                switch (AccountType)
                {
                    case MAccountType.ACCOUNT_TYPE_FREE:
                        return (Color)Application.Current.Resources["MegaFreeAccountColor"];
                    case MAccountType.ACCOUNT_TYPE_LITE:
                        return (Color)Application.Current.Resources["MegaProLiteAccountColor"];
                    case MAccountType.ACCOUNT_TYPE_PROI:
                    case MAccountType.ACCOUNT_TYPE_PROII:
                    case MAccountType.ACCOUNT_TYPE_PROIII:
                        return (Color)Application.Current.Resources["MegaProAccountColor"];
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool IsProAccount => AccountType != MAccountType.ACCOUNT_TYPE_FREE;

        private MStorageState _storageState = MStorageState.STORAGE_STATE_GREEN;
        public MStorageState StorageState
        {
            get { return _storageState; }
            set { SetField(ref _storageState, value); }
        }

        private ulong _totalSpace;
        public ulong TotalSpace
        {
            get { return _totalSpace; }
            set
            {
                SetField(ref _totalSpace, value);
                OnPropertyChanged("TotalSpaceText");
                this.UpdateUsedSpaceValues();
            }
        }

        public string TotalSpaceText => TotalSpace.ToStringAndSuffix();

        private ulong _usedSpace;
        public ulong UsedSpace
        {
            get { return _usedSpace; }
            set
            {
                SetField(ref _usedSpace, value);
                OnPropertyChanged("UsedSpaceText");
                this.UpdateUsedSpaceValues();
            }
        }

        public string UsedSpaceText => UsedSpace.ToStringAndSuffix(1);

        private void UpdateUsedSpaceValues()
        {
            OnPropertyChanged("FreeSpace");
            OnPropertyChanged("FreeSpaceText");
            OnPropertyChanged("IsInStorageOverquota");
            OnPropertyChanged("IsInOverquota");
            OnPropertyChanged("StorageProgressBarColor");
            this.IsInStorageOverquotaChanged?.Invoke(this, EventArgs.Empty);
        }

        public ulong FreeSpace => UsedSpace < TotalSpace ? TotalSpace - UsedSpace : 0;
        public string FreeSpaceText => FreeSpace.ToStringAndSuffix(1);

        private bool _isInStorageOverquota;
        public bool IsInStorageOverquota
        {
            get { return _isInStorageOverquota || (UsedSpace > TotalSpace); }
            set
            {
                SetField(ref _isInStorageOverquota, value);
                OnPropertyChanged(nameof(IsInOverquota), nameof(StorageProgressBarColor));
            }
        }

        private ulong _cloudDriveUsedSpace;
        public ulong CloudDriveUsedSpace
        {
            get { return _cloudDriveUsedSpace; }
            set
            {
                SetField(ref _cloudDriveUsedSpace, value);
                OnPropertyChanged("CloudDriveUsedSpaceText");
                this.CloudDriveUsedSpaceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string CloudDriveUsedSpaceText => CloudDriveUsedSpace.ToStringAndSuffix(1);

        private ulong _incomingSharesUsedSpace;
        public ulong IncomingSharesUsedSpace
        {
            get { return _incomingSharesUsedSpace; }
            set
            {
                SetField(ref _incomingSharesUsedSpace, value);
                OnPropertyChanged("IncomingSharesUsedSpaceText");
                this.IncomingSharesUsedSpaceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string IncomingSharesUsedSpaceText => IncomingSharesUsedSpace.ToStringAndSuffix(1);

        private ulong _rubbishBinUsedSpace;
        public ulong RubbishBinUsedSpace
        {
            get { return _rubbishBinUsedSpace; }
            set
            {
                SetField(ref _rubbishBinUsedSpace, value);
                OnPropertyChanged("RubbishBinUsedSpaceText");
                this.RubbishBinUsedSpaceChanged?.Invoke(this, EventArgs.Empty);
            }
        }        

        public string RubbishBinUsedSpaceText => RubbishBinUsedSpace.ToStringAndSuffix(1);

        private ulong _inSharesUsedSpace;
        public ulong InSharesUsedSpace
        {
            get { return _inSharesUsedSpace; }
            set
            {
                SetField(ref _inSharesUsedSpace, value);
                OnPropertyChanged("InSharesUsedSpaceText");
            }
        }

        public string InSharesUsedSpaceText => InSharesUsedSpace.ToStringAndSuffix(1);

        public Color StorageProgressBarColor => IsInStorageOverquota ? (Color)Application.Current.Resources["MegaRedColor"] :
            (Color)Application.Current.Resources["UsedStorageQuotaColor"];

        private ulong _transferQuota;
        public ulong TransferQuota
        {
            get { return _transferQuota; }
            set
            {
                SetField(ref _transferQuota, value);
                OnPropertyChanged("TransferQuotaText");
                this.UpdateUsedTransferQuotaValues();
            }
        }

        public string TransferQuotaText => IsProAccount ? 
            TransferQuota.ToStringAndSuffix() : ResourceService.UiResources.GetString("UI_Dynamic");

        private ulong _usedTransferQuota;
        public ulong UsedTransferQuota
        {
            get { return _usedTransferQuota; }
            set
            {
                SetField(ref _usedTransferQuota, value);
                OnPropertyChanged("UsedTransferQuotaText");
                this.UpdateUsedTransferQuotaValues();
            }
        }

        public string UsedTransferQuotaText => IsProAccount ?
            UsedTransferQuota.ToStringAndSuffix(1) : ResourceService.UiResources.GetString("UI_NotAvailable");

        private void UpdateUsedTransferQuotaValues()
        {
            OnPropertyChanged("AvailableTransferQuota");
            OnPropertyChanged("AvailableTransferQuotaText");
            OnPropertyChanged("IsInTransferOverquota");
            OnPropertyChanged("IsInOverquota");
            OnPropertyChanged("TransferQuotaProgressBarColor");
        }

        public ulong AvailableTransferQuota => UsedTransferQuota < TransferQuota ? TransferQuota - UsedTransferQuota : 0;

        public string AvailableTransferQuotaText => IsProAccount ?
            AvailableTransferQuota.ToStringAndSuffix(1) : ResourceService.UiResources.GetString("UI_NotAvailable");

        private bool _isInTransferOverquota;
        public bool IsInTransferOverquota
        {
            get { return _isInTransferOverquota || (UsedTransferQuota > TransferQuota); }
            set
            {
                SetField(ref _isInTransferOverquota, value);
                OnPropertyChanged("IsInOverquota");
                OnPropertyChanged("TransferQuotaProgressBarColor");
            }
        }

        private ulong _transferOverquotaDelay;
        public ulong TransferOverquotaDelay
        {
            get { return _transferOverquotaDelay; }
            set
            {
                SetField(ref _transferOverquotaDelay, value);
                OnPropertyChanged("TransferOverquotaDelayText");
            }
        }

        public string TransferOverquotaDelayText =>
            TimeSpan.FromSeconds(TransferOverquotaDelay).ToString();
            

        public Color TransferQuotaProgressBarColor => IsInStorageOverquota ? (Color)Application.Current.Resources["MegaRedColor"] :
            (Color)Application.Current.Resources["UsedTransferQuotaColor"];

        public bool IsInOverquota => IsInStorageOverquota || IsInTransferOverquota;

        private string _paymentMethod;
        public string PaymentMethod
        {
            get
            {
                return string.IsNullOrWhiteSpace(_paymentMethod) ? 
                    ResourceService.UiResources.GetString("UI_NotAvailable") : _paymentMethod;
            }
            set { SetField(ref _paymentMethod, value); }
        }

        private string _subscriptionRenewDate;
        public string SubscriptionRenewDate
        {
            get { return _subscriptionRenewDate; }
            set { SetField(ref _subscriptionRenewDate, value); }
        }

        private string _proExpirationDate;
        public string ProExpirationDate
        {
            get { return _proExpirationDate; }
            set { SetField(ref _proExpirationDate, value); }
        }

        private string _subscriptionType;
        public string SubscriptionType
        {
            get
            {
                return string.IsNullOrWhiteSpace(_subscriptionType) ?
                    ResourceService.UiResources.GetString("UI_NotAvailable") : _subscriptionType;
            }
            set { SetField(ref _subscriptionType, value); }
        }

        #endregion
    }
}
