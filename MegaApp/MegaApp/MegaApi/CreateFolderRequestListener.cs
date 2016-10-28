using System;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class CreateFolderRequestListener: BaseRequestListener
    {
        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ResourceService.ProgressMessages.GetString("PM_CreateFolder"); }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_CreateFolderFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return ResourceService.AppMessages.GetString("AM_CreateFolderFailed_Title"); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_CreateFolderSuccess"); }
        }

        protected override string SuccessMessageTitle
        {
            get { return ResourceService.AppMessages.GetString("AM_CreateFolderSuccess_Title"); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return true; }
        }

        protected override bool NavigateOnSucces
        {
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
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
