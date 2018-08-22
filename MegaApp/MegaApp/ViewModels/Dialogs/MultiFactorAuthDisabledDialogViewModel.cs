using System;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class MultiFactorAuthDisabledDialogViewModel : BaseContentDialogViewModel
    {
        public MultiFactorAuthDisabledDialogViewModel() : base()
        {
            this.TitleText = ResourceService.AppMessages.GetString("AM_2FA_DisabledDialogTitle");
        }

        #region Properties

        /// <summary>
        /// Uri image to display in the dialog
        /// </summary>
        public Uri MultiFactorAuthImageUri =>
            new Uri("ms-appx:///Assets/MultiFactorAuth/multiFactorAuth.png");

        #endregion
    }
}
