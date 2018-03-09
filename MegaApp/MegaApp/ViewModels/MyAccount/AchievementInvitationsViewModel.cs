using System.Windows.Input;
using MegaApp.Enums;
using MegaApp.Services;

namespace MegaApp.ViewModels.MyAccount
{
    public class AchievementInvitationsViewModel : MyAccountBaseViewModel
    {

        #region Commands

        public ICommand InviteCommand { get; }

        #endregion

        #region Properties

        public AwardViewModel InvitationAward { get; set; }

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                SetField(ref _email, value);
                //SetState();
            }
        }

        private InputState _emailInputState;
        public InputState EmailInputState
        {
            get { return _emailInputState; }
            set { SetField(ref _emailInputState, value); }
        }

        #endregion

        #region UiResources

        // Invite friends
        public string InviteFriendsTitle => ResourceService.UiResources.GetString("UI_InviteFriends");
        public string InviteFriendsText => ResourceService.UiResources.GetString("UI_InviteFriendsText");
        public string EmailWatermarkText => ResourceService.UiResources.GetString("UI_EmailWatermark");
        public string InviteText => ResourceService.UiResources.GetString("UI_Invite");
        public string HowItWorksTitle => ResourceService.UiResources.GetString("UI_HowItWorks");
        public string HowItWorksText => ResourceService.UiResources.GetString("UI_HowItWorksText");
        public string WhyNotWorkingTitle => ResourceService.UiResources.GetString("UI_WhyNotWorking");
        public string WhyNotWorkingTextPart1 => ResourceService.UiResources.GetString("UI_WhyNotWorkingText_Part_1");
        public string WhyNotWorkingTextPart2 => ResourceService.UiResources.GetString("UI_WhyNotWorkingText2_Part_2");

        // Referral bonuses
        public string AchievementsTitle => ResourceService.UiResources.GetString("UI_Achievements");
        public string AchievementsText => ResourceService.UiResources.GetString("UI_AchievementsText");

        public string AvailableText => ResourceService.UiResources.GetString("UI_Available");
        public string CompletedText => ResourceService.UiResources.GetString("UI_Completed");

        #endregion
    }
}
