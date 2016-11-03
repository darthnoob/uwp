using System;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class CleanRubbishBinRequestListener : BaseRequestListener
    {

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ResourceService.ProgressMessages.GetString("PM_CleanRubbishBin"); }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_CleanRubbishBinFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return ResourceService.AppMessages.GetString("AM_CleanRubbishBin_Title").ToUpper(); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_CleanRubbishBinSuccess"); }
        }

        protected override string SuccessMessageTitle
        {
            get { return ResourceService.AppMessages.GetString("AM_CleanRubbishBin_Title").ToUpper(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return false; }
        }

        protected override Type NavigateToPage
        {
            get { throw new NotImplementedException(); }
        }

        protected override NavigationObject NavigationObject
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
