using System;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class MultiFactorAuthEnabledDialogViewModel : BaseContentDialogViewModel
    {
        #region Properties

        /// <summary>
        /// Uri image to display in the dialog
        /// </summary>
        public Uri MultiFactorAuthImageUri =>
            new Uri("ms-appx:///Assets/MultiFactorAuth/multiFactorAuth.png");

        #endregion

        #region AppMessageResources

        public string TitleText => ResourceService.AppMessages.GetString("AM_2FA_EnabledDialogTitle");
        public string DescriptionText => ResourceService.AppMessages.GetString("AM_2FA_EnabledDialogDescription");

        #endregion
    }
}
