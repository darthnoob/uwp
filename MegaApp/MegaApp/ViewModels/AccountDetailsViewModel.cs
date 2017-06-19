using System;
using Windows.UI;
using Windows.UI.Xaml;
using mega;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class AccountDetailsViewModel : BaseViewModel
    {
        public AccountDetailsViewModel()
        {
            AccountType = MAccountType.ACCOUNT_TYPE_FREE; // Default value
        }

        #region Properties

        private MAccountType _accountType;
        public MAccountType AccountType
        {
            get { return _accountType; }
            set
            {
                SetField(ref _accountType, value);
                OnPropertyChanged("AccountTypeText");
                OnPropertyChanged("IsProAccount");
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

        public Uri AccountTypeUri
        {
            get
            {
                switch (AccountType)
                {
                    case MAccountType.ACCOUNT_TYPE_FREE:
                        return null;
                    case MAccountType.ACCOUNT_TYPE_LITE:
                        return null;
                    case MAccountType.ACCOUNT_TYPE_PROI:
                        return null;
                    case MAccountType.ACCOUNT_TYPE_PROII:
                        return null;
                    case MAccountType.ACCOUNT_TYPE_PROIII:
                        return null;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool IsProAccount => AccountType != MAccountType.ACCOUNT_TYPE_FREE;

        private ulong _totalSpace;
        public ulong TotalSpace
        {
            get { return _totalSpace; }
            set
            {
                SetField(ref _totalSpace, value);
                OnPropertyChanged("TotalSpaceWithUnits");
                OnPropertyChanged("IsInStorageOverquota");
                OnPropertyChanged("IsInOverquota");
                OnPropertyChanged("StorageProgressBarColor");
            }
        }

        private string _totalSpaceUnits;
        public string TotalSpaceUnits
        {
            get { return _totalSpaceUnits; }
            set
            {
                SetField(ref _totalSpaceUnits, value);
                OnPropertyChanged("TotalSpaceWithUnits");
            }
        }

        public string TotalSpaceWithUnits => TotalSpace + " " + TotalSpaceUnits;

        private ulong _usedSpace;
        public ulong UsedSpace
        {
            get { return _usedSpace; }
            set
            {
                SetField(ref _usedSpace, value);
                OnPropertyChanged("UsedSpaceWithUnits");
                OnPropertyChanged("IsInStorageOverquota");
                OnPropertyChanged("IsInOverquota");
                OnPropertyChanged("StorageProgressBarColor");
            }
        }

        private string _usedSpaceUnits;
        public string UsedSpaceUnits
        {
            get { return _usedSpaceUnits; }
            set
            {
                SetField(ref _usedSpaceUnits, value);
                OnPropertyChanged("UsedSpaceWithUnits");
            }
        }

        public string UsedSpaceWithUnits => UsedSpace + " " + UsedSpaceUnits;

        public bool IsInStorageOverquota => (UsedSpace > TotalSpace);

        public Color StorageProgressBarColor
        {
            get
            {
                return IsInStorageOverquota ? (Color)Application.Current.Resources["MegaRedColor"] :
                    (Color)Application.Current.Resources["MegaBlueColor"];
            }
        }

        private ulong _transferQuota;
        public ulong TransferQuota
        {
            get { return _transferQuota; }
            set
            {
                SetField(ref _transferQuota, value);
                OnPropertyChanged("TransferQuotaWithUnits");
                OnPropertyChanged("IsInTransferOverquota");
                OnPropertyChanged("IsInOverquota");
                OnPropertyChanged("TransferQuotaProgressBarColor");
            }
        }

        private string _transferQuotaUnits;
        public string TransferQuotaUnits
        {
            get { return _transferQuotaUnits; }
            set
            {
                SetField(ref _transferQuotaUnits, value);
                OnPropertyChanged("TransferQuotaWithUnits");
            }
        }

        public string TransferQuotaWithUnits => IsProAccount ? 
            TransferQuota + " " + TransferQuotaUnits : 
            ResourceService.UiResources.GetString("UI_Dynamic");

        private ulong _usedTransferQuota;
        public ulong UsedTransferQuota
        {
            get { return _usedTransferQuota; }
            set
            {
                SetField(ref _usedTransferQuota, value);
                OnPropertyChanged("UsedTransferQuotaWithUnits");
                OnPropertyChanged("IsInTransferOverquota");
                OnPropertyChanged("IsInOverquota");
                OnPropertyChanged("TransferQuotaProgressBarColor");
            }
        }

        private string _usedTransferQuotaUnits;
        public string UsedTransferQuotaUnits
        {
            get { return _usedTransferQuotaUnits; }
            set
            {
                SetField(ref _usedTransferQuotaUnits, value);
                OnPropertyChanged("UsedTransferQuotaWithUnits");
            }
        }

        public string UsedTransferQuotaWithUnits => IsProAccount ?
            UsedTransferQuota + " " + UsedTransferQuotaUnits :
            ResourceService.UiResources.GetString("UI_NotAvailable");

        public bool IsInTransferOverquota => (UsedTransferQuota > TransferQuota);

        public Color TransferQuotaProgressBarColor
        {
            get
            {
                return IsInStorageOverquota ? (Color)Application.Current.Resources["MegaRedColor"] :
                    (Color)Application.Current.Resources["MegaGreenColor"];
            }
        }

        public bool IsInOverquota => IsInStorageOverquota || IsInTransferOverquota;

        private string _paymentMethod;
        public string PaymentMethod
        {
            get { return _paymentMethod; }
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

        #endregion
    }
}
