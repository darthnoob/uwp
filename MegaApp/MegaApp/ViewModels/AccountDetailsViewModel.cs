using mega;

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
            }
        }

        public bool IsProAccount => AccountType != MAccountType.ACCOUNT_TYPE_FREE;

        #endregion
    }
}
