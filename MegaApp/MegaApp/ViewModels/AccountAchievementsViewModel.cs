using System.Collections.Generic;
using MegaApp.Extensions;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.ViewModels
{
    public class AccountAchievementsViewModel: BaseViewModel
    {
        #region Properties

        private ulong _currentStorageQuota;
        public ulong CurrentStorageQuota
        {
            get { return _currentStorageQuota; }
            set { SetField(ref _currentStorageQuota, value); }
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
            set { SetField(ref _currentTransferQuota, value); }
        }

        public ulong CurrentTransferQuotaReadableSize => CurrentTransferQuota.ToReadableSize();

        public string CurrentTransferQuotaText => CurrentTransferQuota > 0
            ? CurrentTransferQuota.ToStringAndSuffix()
            : "- GB";

        public string CurrentTransferQuotaReadableUnits => CurrentTransferQuota > 0
            ? CurrentTransferQuota.ToReadableUnits()
            : "GB";

        private IList<AwardViewModel> _awards;
        public IList<AwardViewModel> Awards
        {
            get { return _awards; }
            set { SetField(ref _awards, value); }
        }

        private IList<AwardViewModel> _availableAwards;
        public IList<AwardViewModel> AvailableAwards
        {
            get { return _availableAwards; }
            set { SetField(ref _availableAwards, value); }
        }

        private IList<AwardViewModel> _completedAwards;
        public IList<AwardViewModel> CompletedAwards
        {
            get { return _completedAwards; }
            set { SetField(ref _completedAwards, value); }
        }

        #endregion
    }
}
