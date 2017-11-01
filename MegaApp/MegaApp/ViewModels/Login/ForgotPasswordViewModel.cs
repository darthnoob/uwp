using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.Views.Login;

namespace MegaApp.ViewModels.Login
{
    public class ForgotPasswordViewModel: BasePageViewModel
    {
        public ForgotPasswordViewModel()
        {
            this.ConfirmCommand = new RelayCommand(Confirm);
            this.DenyCommand = new RelayCommand(Deny);
        }

        #region Methods

        private void Confirm()
        {
            NavigateService.Instance.Navigate(typeof(RecoveryPage), true);
        }

        private void Deny()
        {
            NavigateService.Instance.Navigate(typeof(ParkAccountPage), true);
        }

        #endregion

        #region Commands

        public ICommand ConfirmCommand { get; }
        public ICommand DenyCommand { get; }

        #endregion

        #region UiResources

        public string RecoveryConfirmText => ResourceService.UiResources.GetString("UI_Yes");
        public string RecoveryDenyText => ResourceService.UiResources.GetString("UI_No");
        public string ForgotMyPasswordHeaderText => ResourceService.UiResources.GetString("UI_ForgotMyPassword");

        public string ForgotMyPasswordDescriptionText => UiService.ConcatStringsToParagraph(
            new[]
            {
                ResourceService.UiResources.GetString("UI_ForgotMyPasswordDescription_Part_1"),
                ResourceService.UiResources.GetString("UI_ForgotMyPasswordDescription_Part_2"),
                ResourceService.UiResources.GetString("UI_ForgotMyPasswordDescription_Part_3")
            });
        public string RecoveryKeyQuestionText => ResourceService.UiResources.GetString("UI_RecoveryKeyQuestion");

        #endregion
    }
}
