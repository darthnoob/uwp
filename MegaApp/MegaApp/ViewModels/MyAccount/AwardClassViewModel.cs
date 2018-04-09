using System;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Services;
using MegaApp.ViewModels.Contacts;
using MegaApp.Views.MyAccount;

namespace MegaApp.ViewModels.MyAccount
{
    public class AwardClassViewModel: BaseUiViewModel
    {
        public AwardClassViewModel(MAchievementClass? achievementClass, bool isBaseAward = false)
        {
            this.AchievementClass = achievementClass;
            this.IsBaseAward = isBaseAward;
            this.Contacts = new ContactsListViewModel();
            this.ActionCommand = new RelayCommand(DoAction);
        }

        #region Methods

        private string GetAwardTitle()
        {
            if (!AchievementClass.HasValue) return null;
            switch (AchievementClass)
            {
                case MAchievementClass.MEGA_ACHIEVEMENT_WELCOME:
                    return ResourceService.UiResources.GetString("UI_RegistrationBonus");
                case MAchievementClass.MEGA_ACHIEVEMENT_INVITE:
                    return IsGranted
                        ? ResourceService.UiResources.GetString("UI_ReferralBonus")
                        : ResourceService.UiResources.GetString("UI_InviteFriends");
                case MAchievementClass.MEGA_ACHIEVEMENT_DESKTOP_INSTALL:
                    return ResourceService.UiResources.GetString("UI_InstallDesktopApp"); ;
                case MAchievementClass.MEGA_ACHIEVEMENT_MOBILE_INSTALL:
                    return ResourceService.UiResources.GetString("UI_InstallMobileApp"); ;
                default:
                    return null;
            }
        }

        private Uri GetAwardImageUri()
        {
            if (!AchievementClass.HasValue) return null;
            switch (AchievementClass)
            {
                case MAchievementClass.MEGA_ACHIEVEMENT_WELCOME:
                    return new Uri("ms-appx:///Assets/Achievements/gettingStarted.png");
                case MAchievementClass.MEGA_ACHIEVEMENT_DESKTOP_INSTALL:
                    return new Uri("ms-appx:///Assets/Achievements/desktopApp.png");
                case MAchievementClass.MEGA_ACHIEVEMENT_MOBILE_INSTALL:
                    return new Uri("ms-appx:///Assets/Achievements/mobileApp.png");
                case MAchievementClass.MEGA_ACHIEVEMENT_INVITE:
                    return new Uri("ms-appx:///Assets/Achievements/inviteFriend.png");
                default:
                    return null;
            }
        }

        private string GetDays(long days)
        {
            if (days < 1) return "-";
            return days == 1
                ? String.Format("1 {0}", ResourceService.UiResources.GetString("UI_Day"))
                : String.Format("{0} {1}", days, ResourceService.UiResources.GetString("UI_Days"));

        }

        private void DoAction()
        {
            if (!AchievementClass.HasValue) return;
            switch (AchievementClass)
            {
                case MAchievementClass.MEGA_ACHIEVEMENT_WELCOME:
                case MAchievementClass.MEGA_ACHIEVEMENT_DESKTOP_INSTALL:
                case MAchievementClass.MEGA_ACHIEVEMENT_MOBILE_INSTALL:
                {
                    DialogService.ShowAchievementInformationDialog(this);
                    break;
                }
                case MAchievementClass.MEGA_ACHIEVEMENT_INVITE:
                    AchievementInvitationsViewModel.InvitationAward = this;
                    NavigateService.Instance.Navigate(typeof(AchievementInvitationsPage));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetAwardDescription()
        {
            if (!AchievementClass.HasValue) return null;
            if (IsGranted)
            {
                return AchievementClass == MAchievementClass.MEGA_ACHIEVEMENT_INVITE
                    ? ResourceService.UiResources.GetString("UI_ViewStatus")
                    : AccountService.GetDaysRemaining(ExpiresIn);
            }

            switch (AchievementClass)
            {
                case MAchievementClass.MEGA_ACHIEVEMENT_INVITE:
                    return ResourceService.UiResources.GetString("UI_SendInvites");
                case MAchievementClass.MEGA_ACHIEVEMENT_DESKTOP_INSTALL:
                case MAchievementClass.MEGA_ACHIEVEMENT_MOBILE_INSTALL:
                    return ResourceService.UiResources.GetString("UI_Download"); ;
                default:
                    return null;
            }
        }

        private string GetAwardInformation()
        {
            if (!AchievementClass.HasValue) return null;
            switch (AchievementClass)
            {
                case MAchievementClass.MEGA_ACHIEVEMENT_WELCOME:
                    return string.Format(ResourceService.UiResources.GetString("UI_AccountRegistrationInformation"),
                        StorageRewardText,
                        DurationInDays);
                case MAchievementClass.MEGA_ACHIEVEMENT_DESKTOP_INSTALL:
                    return string.Format(ResourceService.UiResources.GetString("UI_InstallDesktopAppInformation"),
                        StorageRewardText,
                        TransferRewardText,
                        DurationInDays);
                case MAchievementClass.MEGA_ACHIEVEMENT_MOBILE_INSTALL:
                    return string.Format(ResourceService.UiResources.GetString("UI_InstallMobileAppInformation"),
                        StorageRewardText,
                        TransferRewardText,
                        DurationInDays);
                default:
                    return null;
            }
        }

        #endregion

        #region Commands

        public ICommand ActionCommand { get; }

        #endregion

        #region Properties

        public MAchievementClass? AchievementClass { get; }

        public bool IsBaseAward { get; }

        public string DisplayNameStorage => IsBaseAward
            ? ResourceService.UiResources.GetString("UI_BaseStorage")
            : GetAwardTitle();

        public string DisplayNameTransfer => IsBaseAward
            ? ResourceService.UiResources.GetString("UI_BaseTransferQuota")
            : GetAwardTitle();

        public string Title => GetAwardTitle();

        public Uri ImageUri => GetAwardImageUri();

        public bool HasImage => ImageUri != null;

        private bool _isGranted;
        public bool IsGranted
        {
            get { return _isGranted; }
            set { SetField(ref _isGranted, value); }
        }

        private DateTime? _expireDate;
        public DateTime? ExpireDate
        {
            get { return _expireDate; }
            set { SetField(ref _expireDate, value); }
        }

        public bool IsExpired => ExpireDate.HasValue && ExpireDate <= DateTime.Now;

        public int ExpiresIn => ExpireDate?.Subtract(DateTime.Today).Days ?? -1;

        private DateTime _achievedOnDate;
        public DateTime AchievedOnDate
        {
            get { return _achievedOnDate; }
            set { SetField(ref _achievedOnDate, value); }
        }

        public string AchievedOnText => AchievedOnDate.ToString("dd MMM yyyy");

        private long _durationInDays;
        public long DurationInDays
        {
            get { return _durationInDays; }
            set { SetField(ref _durationInDays, value); }
        }

        private long _storageReward;
        public long StorageReward
        {
            get { return _storageReward; }
            set { SetField(ref _storageReward, value); }
        }

        private long _transferReward;
        public long TransferReward
        {
            get { return _transferReward; }
            set { SetField(ref _transferReward, value); }
        }

        public string StorageRewardText => StorageReward > 0
            ? ((ulong)StorageReward).ToStringAndSuffix()
            : "- GB";

        public string TransferRewardText => TransferReward > 0
            ? ((ulong)TransferReward).ToStringAndSuffix() : "- GB";

        public ContactsListViewModel Contacts { get; }

        public string Description => GetAwardDescription();

        public string Information => GetAwardInformation();

        public bool IsTransferAmountVisible { get; set; } = true;

        public string ExpiresInText => GetDays(ExpiresIn);

        #endregion
    }
}
