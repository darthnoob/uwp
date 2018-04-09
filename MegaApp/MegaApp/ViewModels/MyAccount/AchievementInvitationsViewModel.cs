using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.MyAccount
{
    public class AchievementInvitationsViewModel : BasePageViewModel
    {
        public static AwardClassViewModel InvitationAward { get; set; }

        public AchievementInvitationsViewModel()
        {
            this.InviteCommand = new RelayCommand(Invite);
        }

        #region Methods

        private async void Invite()
        {
            SetWarning(false, string.Empty);
            this.EmailInputState = InputState.Normal;

            if (!await NetworkService.IsNetworkAvailableAsync(true)) return;

            if (!CheckInputParameters()) return;

            this.IsBusy = true;
            this.ControlState = false;
            this.InviteButtonState = false;
           
            var invite = new InviteContactRequestListenerAsync();
            var result = await invite.ExecuteAsync(() =>
            {
                SdkService.MegaSdk.inviteContact(
                    this.Email, string.Empty , MContactRequestInviteActionType.INVITE_ACTION_ADD, invite);
            });

            this.ControlState = true;
            this.InviteButtonState = true;
            this.IsBusy = false;

            switch (result)
            {
                case InviteContactResult.Success:
                    await DialogService.ShowAlertAsync(InviteFriendsTitle,
                        string.Format(ResourceService.AppMessages.GetString("AM_InviteContactSuccessfully"),
                            this.Email));
                    this.Email = null;
                    break;

                case InviteContactResult.AlreadyExists:
                    await DialogService.ShowAlertAsync(InviteFriendsTitle,
                        ResourceService.AppMessages.GetString("AM_ContactAlreadyExists"));
                    break;

                case InviteContactResult.Unknown:
                    await DialogService.ShowAlertAsync(InviteFriendsTitle,
                        ResourceService.AppMessages.GetString("AM_InviteContactFailed"));
                    break;
            }
        }

        private bool CheckInputParameters()
        {
            if (string.IsNullOrWhiteSpace(this.Email))
            {
                SetWarning(true, ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
                EmailInputState = InputState.Warning;
                return false;
            }

            if (ValidationService.IsValidEmail(this.Email)) return true;

            SetWarning(true, ResourceService.AppMessages.GetString("AM_IncorrectEmailFormat"));
            EmailInputState = InputState.Warning;
            return false;
        }

        private void SetWarning(bool isVisible, string warningText)
        {
            if (isVisible)
            {
                // First text and then display
                this.WarningText = warningText;
                this.IsWarningVisible = true;
            }
            else
            {
                // First remove and than clean text
                this.IsWarningVisible = false;
                this.WarningText = warningText;
            }
        }

        #endregion

        #region Commands

        public ICommand InviteCommand { get; }

        #endregion

        #region Properties

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                SetField(ref _email, value);
                this.InviteButtonState = !string.IsNullOrWhiteSpace(_email);
            }
        }

        private InputState _emailInputState;
        public InputState EmailInputState
        {
            get { return _emailInputState; }
            set { SetField(ref _emailInputState, value); }
        }

        private bool _inviteButtonState;
        public bool InviteButtonState
        {
            get { return _inviteButtonState; }
            set { SetField(ref _inviteButtonState, value); }
        }

        private string _warningText;
        public string WarningText
        {
            get { return _warningText; }
            set { SetField(ref _warningText, value); }
        }

        private bool _isWarningVisible;

        public bool IsWarningVisible
        {
            get { return _isWarningVisible; }
            set { SetField(ref _isWarningVisible, value); }
        }

        public AwardClassViewModel AwardClass => InvitationAward;

        #endregion

        #region UiResources

        // Invite friends
        public string InviteFriendsTitle => ResourceService.UiResources.GetString("UI_InviteFriends");
        public string InviteFriendsText => string.Format(ResourceService.UiResources.GetString("UI_InviteFriendsText"),
            AwardClass.StorageRewardText, AwardClass.TransferRewardText);
        public string EmailWatermarkText => ResourceService.UiResources.GetString("UI_EmailWatermark");
        public string InviteText => ResourceService.UiResources.GetString("UI_Invite");
        public string HowItWorksTitle => ResourceService.UiResources.GetString("UI_HowItWorks");
        public string HowItWorksText => ResourceService.UiResources.GetString("UI_HowItWorksText");
        public string WhyNotWorkingTitle => ResourceService.UiResources.GetString("UI_WhyNotWorking");
        public string WhyNotWorkingTextPart1 => ResourceService.UiResources.GetString("UI_WhyNotWorkingText_Part_1");
        public string WhyNotWorkingTextPart2 => ResourceService.UiResources.GetString("UI_WhyNotWorkingText_Part_2");

        // Referral bonuses
        public string ReferralBonusTitle => ResourceService.UiResources.GetString("UI_ReferralBonus");
        public string AchievementsText => ResourceService.UiResources.GetString("UI_AchievementsText");

        public string AvailableText => ResourceService.UiResources.GetString("UI_Available");
        public string CompletedText => ResourceService.UiResources.GetString("UI_Completed");

        // Section
        public string SectionNameText => ResourceService.UiResources.GetString("UI_Achievements");

        #endregion

        #region Visual Resources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");

        #endregion
    }
}
