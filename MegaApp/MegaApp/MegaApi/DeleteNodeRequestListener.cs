using System;
using System.Threading;
using mega;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    class DeleteNodeRequestListener: BaseRequestListener
    {
        private NodeViewModel _nodeViewModel;
        private bool _isMultiSelect;
        private AutoResetEvent _waitEventRequest;

        public DeleteNodeRequestListener(NodeViewModel nodeViewModel, bool isMultiSelect = false, AutoResetEvent waitEventRequest = null)
        {
            this._nodeViewModel = nodeViewModel;
            this._isMultiSelect = isMultiSelect;
            
            this._waitEventRequest = waitEventRequest;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ResourceService.ProgressMessages.GetString("PM_Delete"); }                
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_RemoveNodeFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return ResourceService.AppMessages.GetString("AM_RemoveNodeFailed_Title"); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {            
            get { return ResourceService.AppMessages.GetString("AM_RemoveNodeSuccess"); }
        }

        protected override string SuccessMessageTitle
        {
            get { return ResourceService.AppMessages.GetString("AM_RemoveNodeSuccess_Title"); }
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

        #region Override Methods

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            //Deployment.Current.Dispatcher.BeginInvoke(() =>
            //{
            //    ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
            //    ProgressService.SetProgressIndicator(false);
            //});

            this._waitEventRequest?.Set();

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                if (ShowSuccesMessage && !_isMultiSelect)
                {
                    new CustomMessageDialog(
                        SuccessMessageTitle,
                        SuccessMessage,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                }

                if (ActionOnSucces)
                    OnSuccesAction(api, request);                
            }
            else if (e.getErrorCode() != MErrorType.API_EINCOMPLETE)
            {
                if (ShowErrorMessage)
                {
                    new CustomMessageDialog(
                        ErrorMessageTitle,
                        String.Format(ErrorMessage, e.getErrorString()),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                }
            }
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            UiService.OnUiThread(() =>
            {
                try 
                {
                    _nodeViewModel?.ParentCollection?.Remove(_nodeViewModel);
                    _nodeViewModel = null;
                }
                catch (Exception) { }
            });
        }

        #endregion
    }
}
