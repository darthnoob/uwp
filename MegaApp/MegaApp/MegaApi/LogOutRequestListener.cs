using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class LogOutRequestListener: BaseRequestListener
    {
        private readonly bool _navigateOnSucces;

        /// <summary>
        /// LogOutRequestListener constructor
        /// </summary>
        /// <param name="navigateOnSucces">
        /// Boolean value to allow the developer decide if the app should go to the
        /// "InitTourPage" after logout or no. The default value is TRUE.
        /// </param>        
        public LogOutRequestListener(bool navigateOnSucces = true)
        {
            _navigateOnSucces = navigateOnSucces;
        }

        protected override string ProgressMessage
        {
            get { return App.ResourceLoaders.ProgressMessages.GetString("PM_Logout"); }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return App.ResourceLoaders.AppMessages.GetString("AM_LogoutFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return App.ResourceLoaders.AppMessages.GetString("AM_LogoutFailed_Title"); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return App.ResourceLoaders.AppMessages.GetString("AM_LoggedOut"); }
        }

        protected override string SuccessMessageTitle
        {
            get { return App.ResourceLoaders.AppMessages.GetString("AM_LoggedOut_Title"); }
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

        protected override NavigationParameter NavigationParameter
        {
            get { return NavigationParameter.Normal; }
        }

        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            AppService.LogoutActions();
        }

        #endregion
    }
}
