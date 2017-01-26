using System;
using System.Threading;
using mega;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.Views;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    class LogOutRequestListener: BaseRequestListener
    {
        private readonly bool _navigateOnSucces;
        private readonly AutoResetEvent _waitEventRequest;

        /// <summary>
        /// LogOutRequestListener constructor
        /// </summary>
        /// <param name="navigateOnSucces">
        /// Boolean value to allow the developer decide if the app should go to the
        /// "InitTourPage" after logout or no. The default value is TRUE.
        /// </param>
        /// <param name="waitEventRequest">
        /// Event to notify to the caller method that the request has finished
        /// </param>
        public LogOutRequestListener(bool navigateOnSucces = true, AutoResetEvent waitEventRequest = null)
        {
            _navigateOnSucces = navigateOnSucces;
            _waitEventRequest = waitEventRequest;
        }

        protected override string ProgressMessage
        {
            get { return ResourceService.ProgressMessages.GetString("PM_Logout"); }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_LogoutFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return ResourceService.AppMessages.GetString("AM_LogoutFailed_Title"); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_LoggedOut"); }
        }

        protected override string SuccessMessageTitle
        {
            get { return ResourceService.AppMessages.GetString("AM_LoggedOut_Title"); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return _navigateOnSucces; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { return typeof(LoginAndCreateAccountPage); }
        }

        protected override NavigationObject NavigationObject
        {
            get { return NavigationObject.Create(typeof(CloudDriveViewModel)); }
        }

        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            _waitEventRequest?.Set();
            AppService.LogoutActions();
        }

        #endregion
    }
}
