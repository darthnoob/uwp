using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Services;

namespace MegaApp.ViewModels.MyAccount
{
    public class AwardViewModel: BaseViewModel
    {

        public AwardViewModel(MAchievementClass? achievementClass, bool isBaseAward = false)
        {
            this.AchievementClass = achievementClass;
            this.IsBaseAward = isBaseAward;
            this.ActionCommand = new RelayCommand(DoAction);
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
                    ShowDialog();
                    break;
                }
                case MAchievementClass.MEGA_ACHIEVEMENT_INVITE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ShowDialog()
        {
            DialogService.ShowAchievementInformationDialog(this);
        }


        public string GetAwardTitle()
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

        private static string GetDaysRemaining(long days)
        {
            if (days < 1) return ResourceService.UiResources.GetString("UI_Expired");
            return days == 1
                ? string.Format("1 {0}", ResourceService.UiResources.GetString("UI_RemainingDay"))
                : string.Format(ResourceService.UiResources.GetString("UI_RemainingDays"), days);

        }

        private static string GetDays(long days)
        {
            if (days < 1) return "-";
            return days == 1
                ? string.Format("1 {0}", ResourceService.UiResources.GetString("UI_Day"))
                : string.Format("{0} {1}", days, ResourceService.UiResources.GetString("UI_Days"));

        }

        public string GetAwardDescription()
        {
            if (!AchievementClass.HasValue) return null;
            if (IsGranted)
            {
                return AchievementClass == MAchievementClass.MEGA_ACHIEVEMENT_INVITE 
                    ? ResourceService.UiResources.GetString("UI_ViewStatus") 
                    : GetDaysRemaining(ExpiresIn);
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

        public string GetAwardInformation()
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

        public Brush GetBackgroundColor()
        {
            if (IsBaseAward) return Application.Current.Resources["MegaAppForegroundBrush"] as SolidColorBrush;
            if (!AchievementClass.HasValue) return Application.Current.Resources["MegaAppForegroundBrush"] as SolidColorBrush;

            switch (AchievementClass)
            {
                case MAchievementClass.MEGA_ACHIEVEMENT_WELCOME:
                    return Application.Current.Resources["MegaAppForegroundBrush"] as SolidColorBrush;
                case MAchievementClass.MEGA_ACHIEVEMENT_INVITE:
                    return Application.Current.Resources["MegaAppForegroundBrush"] as SolidColorBrush;
                case MAchievementClass.MEGA_ACHIEVEMENT_DESKTOP_INSTALL:
                    return Application.Current.Resources["MegaAppForegroundBrush"] as SolidColorBrush; ;
                case MAchievementClass.MEGA_ACHIEVEMENT_MOBILE_INSTALL:
                    return Application.Current.Resources["MegaAppForegroundBrush"] as SolidColorBrush; ;
                default:
                    return null;
            }
        }

        public Brush GetForegroundColor()
        {
            if (IsBaseAward) return Application.Current.Resources["MegaAppBackgroundBrush"] as SolidColorBrush;
            if (!AchievementClass.HasValue) return Application.Current.Resources["MegaAppBackgroundBrush"] as SolidColorBrush;

            switch (AchievementClass)
            {
                case MAchievementClass.MEGA_ACHIEVEMENT_WELCOME:
                    return Application.Current.Resources["MegaAppBackgroundBrush"] as SolidColorBrush;
                case MAchievementClass.MEGA_ACHIEVEMENT_INVITE:
                    return Application.Current.Resources["MegaAppBackgroundBrush"] as SolidColorBrush;
                case MAchievementClass.MEGA_ACHIEVEMENT_DESKTOP_INSTALL:
                    return Application.Current.Resources["MegaAppBackgroundBrush"] as SolidColorBrush; ;
                case MAchievementClass.MEGA_ACHIEVEMENT_MOBILE_INSTALL:
                    return Application.Current.Resources["MegaAppBackgroundBrush"] as SolidColorBrush; ;
                default:
                    return null;
            }
        }

        public ICommand ActionCommand { get; }

        #region Properties

        public MAchievementClass? AchievementClass { get; }

        public bool IsBaseAward { get; }

       
        public Brush BackgroundColor => GetBackgroundColor();

        public Brush ForegroundColor => GetForegroundColor();

        public string DisplayNameStorage => IsBaseAward
            ? ResourceService.UiResources.GetString("UI_BaseStorage")
            : GetAwardTitle();

        public string DisplayNameTransfer => IsBaseAward
            ? ResourceService.UiResources.GetString("UI_BaseTransferQuota")
            : GetAwardTitle();

        public string Title => GetAwardTitle();

        public string Description => GetAwardDescription();

        public string Information => GetAwardInformation();

        private bool _isGranted;
        public bool IsGranted
        {
            get { return _isGranted; }
            set { SetField(ref _isGranted, value); }
        }

        private DateTime _expireDate;
        public DateTime ExpireDate
        {
            get { return _expireDate; }
            set { SetField(ref _expireDate, value); }
        }

        public int ExpiresIn => ExpireDate.Subtract(DateTime.Today).Days;

        public string ExpiresInText => GetDays(ExpiresIn);

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
            ? ((ulong) StorageReward).ToStringAndSuffix()
            : "- GB";

        public string TransferRewardText => TransferReward > 0
            ? ((ulong) TransferReward).ToStringAndSuffix()
            : "- GB";

        public string AwardLetter => IsBaseAward
            ? "M"
            : null;

        public bool IsTransferAmountVisible { get; set; } = true;

        #endregion
    }
}
