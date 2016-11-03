using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using mega;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    class GetThumbnailRequestListener: BaseRequestListener
    {
        private readonly NodeViewModel _node;

        public GetThumbnailRequestListener(NodeViewModel node)
        {
            this._node = node;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowProgressMessage
        {
            get { return false; }
        }

        protected override string ErrorMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_GetThumbnailFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return ResourceService.AppMessages.GetString("AM_GetThumbnailFailed_Title"); }
        }

        protected override bool ShowErrorMessage
        {
            get { return false; }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
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

        protected async override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _node.IsDefaultImage = false;
                _node.ThumbnailImageUri = new Uri(request.getFile());
            });
            
        }

        #endregion
    }
}
