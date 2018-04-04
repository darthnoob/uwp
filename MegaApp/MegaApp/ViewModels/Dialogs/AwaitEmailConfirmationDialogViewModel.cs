using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class AwaitEmailConfirmationDialogViewModel : BaseContentDialogViewModel
    {
        public AwaitEmailConfirmationDialogViewModel() : base() { }

        #region Properties

        private string _email;
        /// <summary>
        /// Email for which is waiting confirmation
        /// </summary>
        public string Email
        {
            get { return _email; }
            set { SetField(ref _email, value); }
        }

        #endregion

        #region UiResources

        public string TitleText => ResourceService.AppMessages.GetString("AM_AwaitingEmailConfirmation");
        public string DescriptionText => ResourceService.AppMessages.GetString("AM_AwaitingEmailConfirmationDescription");
        public string NewEmailTitleText => ResourceService.UiResources.GetString("UI_NewEmail");
        
        #endregion
    }
}
