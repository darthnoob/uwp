using System.Collections.Generic;
using MegaApp.Extensions;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.ViewModels
{
    public class AccountAchievementsViewModel: BaseViewModel
    {
        #region Properties

        private bool _isAchievementsEnabled;
        public bool IsAchievementsEnabled
        {
            get { return _isAchievementsEnabled; }
            set { SetField(ref _isAchievementsEnabled, value); }
        }

        private ulong _currentStorageQuota;
        public ulong CurrentStorageQuota
        {
            get { return _currentStorageQuota; }
            set
            {
                SetField(ref _currentStorageQuota, value);
                OnPropertyChanged(nameof(CurrentStorageQuotaReadableSize),
                    nameof(CurrentStorageQuotaText),
                    nameof(CurrentStorageQuotaReadableUnits));
            }
        }

        public ulong CurrentStorageQuotaReadableSize => CurrentStorageQuota.ToReadableSize();

        public string CurrentStorageQuotaText => CurrentStorageQuota > 0
            ? CurrentStorageQuota.ToStringAndSuffix()
            : "- GB";

        public string CurrentStorageQuotaReadableUnits => CurrentStorageQuota > 0
            ? CurrentStorageQuota.ToReadableUnits()
            : "GB";

        private ulong _currentTransferQuota;
        public ulong CurrentTransferQuota
        {
            get { return _currentTransferQuota; }
            set
            {
                SetField(ref _currentTransferQuota, value);
                OnPropertyChanged(nameof(CurrentTransferQuotaReadableSize),
                    nameof(CurrentTransferQuotaText),
                    nameof(CurrentTransferQuotaReadableUnits));
            }
        }

        public ulong CurrentTransferQuotaReadableSize => CurrentTransferQuota.ToReadableSize();

        public string CurrentTransferQuotaText => CurrentTransferQuota > 0
            ? CurrentTransferQuota.ToStringAndSuffix()
            : "- GB";

        public string CurrentTransferQuotaReadableUnits => CurrentTransferQuota > 0
            ? CurrentTransferQuota.ToReadableUnits()
            : "GB";
        
        private IList<AwardClassViewModel> _awards;
        public IList<AwardClassViewModel> Awards
        {
            get { return _awards; }
            set { SetField(ref _awards, value); }
        }

        private IList<AwardClassViewModel> _awardedClasses;
        public IList<AwardClassViewModel> AwardedClasses
        {
            get { return _awardedClasses; }
            set { SetField(ref _awardedClasses, value); }
        }

        private IList<AwardClassViewModel> _availableAwards;
        public IList<AwardClassViewModel> AvailableAwards
        {
            get { return _availableAwards; }
            set { SetField(ref _availableAwards, value); }
        }

        private IList<AwardClassViewModel> _completedAwards;
        public IList<AwardClassViewModel> CompletedAwards
        {
            get { return _completedAwards; }
            set { SetField(ref _completedAwards, value); }
        }

        #endregion
    }
}
