using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using mega;
using MegaApp.Extensions;
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
                OnPropertyChanged("IsProAccount");
                OnPropertyChanged("AccountTypeText");
                OnPropertyChanged("AccountTypePathData");
                OnPropertyChanged("AccountTypePathDataColorBrush");
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

        public Brush AccountTypePathDataColorBrush
        {
            get
            {
                switch (AccountType)
                {
                    case MAccountType.ACCOUNT_TYPE_FREE:
                        return (Brush)Application.Current.Resources["MegaGreenColorBrush"];
                    case MAccountType.ACCOUNT_TYPE_LITE:
                        return (Brush)Application.Current.Resources["MegaOrangeColorBrush"];
                    case MAccountType.ACCOUNT_TYPE_PROI:
                    case MAccountType.ACCOUNT_TYPE_PROII:
                    case MAccountType.ACCOUNT_TYPE_PROIII:
                        return (Brush)Application.Current.Resources["MegaRedColorBrush"];
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
                OnPropertyChanged("TotalSpaceText");
                OnPropertyChanged("FreeSpace");
                OnPropertyChanged("FreeSpaceText");
                OnPropertyChanged("IsInStorageOverquota");
                OnPropertyChanged("IsInOverquota");
                OnPropertyChanged("StorageProgressBarColor");
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
                OnPropertyChanged("FreeSpace");
                OnPropertyChanged("FreeSpaceText");
                OnPropertyChanged("IsInStorageOverquota");
                OnPropertyChanged("IsInOverquota");
                OnPropertyChanged("StorageProgressBarColor");
            }
        }

        public string UsedSpaceText => UsedSpace.ToStringAndSuffix(1);

        public ulong FreeSpace => TotalSpace - UsedSpace;
        public string FreeSpaceText => FreeSpace.ToStringAndSuffix(1);

        public bool IsInStorageOverquota => (UsedSpace > TotalSpace);

        public ulong _cloudDriveUsedSpace;
        public ulong CloudDriveUsedSpace
        {
            get { return _cloudDriveUsedSpace; }
            set
            {
                SetField(ref _cloudDriveUsedSpace, value);
                OnPropertyChanged("CloudDriveUsedSpaceText");
            }
        }

        public string CloudDriveUsedSpaceText => CloudDriveUsedSpace.ToStringAndSuffix(1);

        public ulong _rubbishBinUsedSpace;
        public ulong RubbishBinUsedSpace
        {
            get { return _rubbishBinUsedSpace; }
            set
            {
                SetField(ref _rubbishBinUsedSpace, value);
                OnPropertyChanged("RubbishBinUsedSpaceText");
            }
        }        

        public string RubbishBinUsedSpaceText => RubbishBinUsedSpace.ToStringAndSuffix(1);

        public ulong _inSharesUsedSpace;
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
                OnPropertyChanged("TransferQuotaText");
                OnPropertyChanged("IsInTransferOverquota");
                OnPropertyChanged("IsInOverquota");
                OnPropertyChanged("TransferQuotaProgressBarColor");
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
                OnPropertyChanged("IsInTransferOverquota");
                OnPropertyChanged("IsInOverquota");
                OnPropertyChanged("TransferQuotaProgressBarColor");
            }
        }

        public string UsedTransferQuotaText => IsProAccount ?
            UsedTransferQuota.ToStringAndSuffix(1) : ResourceService.UiResources.GetString("UI_NotAvailable");

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

        private string _subscriptionType;
        public string SubscriptionType
        {
            get { return _subscriptionType; }
            set { SetField(ref _subscriptionType, value); }
        }

        #endregion
    }
}
