using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class AwaitEmailConfirmationDialogViewModel : BaseContentDialogViewModel
    {
        public AwaitEmailConfirmationDialogViewModel() : base()
        {
            this.TitleText = ResourceService.AppMessages.GetString("AM_AwaitingEmailConfirmation");
            this.MessageText = ResourceService.AppMessages.GetString("AM_AwaitingEmailConfirmationDescription");
    }

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

        public string NewEmailTitleText => ResourceService.UiResources.GetString("UI_NewEmail");
        
        #endregion
    }
}
