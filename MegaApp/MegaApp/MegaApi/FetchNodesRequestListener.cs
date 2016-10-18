using System;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using mega;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    class FetchNodesRequestListener : BaseRequestListener
    {
        private readonly CloudDriveViewModel _cloudDriveViewModel;

        // Timer for ignore the received API_EAGAIN (-3) during login
        private DispatcherTimer timerAPI_EAGAIN;
        private bool isFirstAPI_EAGAIN;

        public FetchNodesRequestListener(CloudDriveViewModel cloudDriveViewModel) : base()
        {
            _cloudDriveViewModel = cloudDriveViewModel;

            createTimer();
        }

        private void createTimer()
        {
            timerAPI_EAGAIN = new DispatcherTimer();
            timerAPI_EAGAIN.Tick += timerTickAPI_EAGAIN;
            timerAPI_EAGAIN.Interval = new TimeSpan(0, 0, 10);
        }

        // Method which is call when the timer event is triggered
        private async void timerTickAPI_EAGAIN(object sender, object e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                timerAPI_EAGAIN.Stop();
                //ProgressService.SetProgressIndicator(true, ProgressMessages.PM_ServersTooBusy);
            });
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ResourceService.ProgressMessages.GetString("PM_FetchingNodes"); }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return ResourceService.AppMessages.GetString("AM_FetchingNodesFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return ResourceService.AppMessages.GetString("AM_FetchingNodesFailed_Title"); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
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

        protected override NavigationParameter NavigationParameter
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            // Enable transfer resumption for the current MegaSDK instance which is
            // doing the fetch nodes request (app, folder link, etc.)
            //api.enableTransferResumption();

            //if (_mainPageViewModel != null)
                FetchNodesCloudDrivePage(api, request);
            //else if (_cameraUploadsPageViewModel != null)
            //    FetchNodesCameraUploadsPage(api, request);
            //else if (_folderLinkViewModel != null)
            //    FetchNodesFolderLink(api, request);
        }

        private async void FetchNodesCloudDrivePage(MegaSDK api, MRequest request)
        {
            App.AppInformation.HasFetchedNodes = true;

            // If the user is trying to open a shortcut
            //if (App.ShortCutBase64Handle != null)
            //{
            //    bool shortCutError = false;

            //    MNode shortCutMegaNode = api.getNodeByBase64Handle(App.ShortCutBase64Handle);
            //    App.ShortCutBase64Handle = null;

            //    if (_mainPageViewModel != null && shortCutMegaNode != null)
            //    {
            //        // Looking for the absolute parent of the shortcut node to see the type
            //        MNode parentNode;
            //        MNode absoluteParentNode = shortCutMegaNode;
            //        while ((parentNode = api.getParentNode(absoluteParentNode)) != null)
            //            absoluteParentNode = parentNode;

            //        if (absoluteParentNode.getType() == MNodeType.TYPE_ROOT)
            //        {
            //            var newRootNode = NodeService.CreateNew(api, _mainPageViewModel.AppInformation, shortCutMegaNode, ContainerType.CloudDrive);
            //            var autoResetEvent = new AutoResetEvent(false);
            //            Deployment.Current.Dispatcher.BeginInvoke(() =>
            //            {
            //                _mainPageViewModel.ActiveFolderView.FolderRootNode = newRootNode;
            //                autoResetEvent.Set();
            //            });
            //            autoResetEvent.WaitOne();
            //        }
            //        else shortCutError = true;
            //    }
            //    else shortCutError = true;

            //    if (shortCutError)
            //    {
            //        Deployment.Current.Dispatcher.BeginInvoke(() =>
            //        {
            //            new CustomMessageDialog(AppMessages.ShortCutFailed_Title,
            //                AppMessages.ShortCutFailed, App.AppInformation,
            //                MessageDialogButtons.Ok).ShowDialog();
            //        });
            //    }
            //}
            //else
            //{
                var cloudDriveRootNode = _cloudDriveViewModel.CloudDrive.FolderRootNode ??
                    NodeService.CreateNew(api, App.AppInformation, api.getRootNode(), ContainerType.CloudDrive);
                var rubbishBinRootNode = _cloudDriveViewModel.RubbishBin.FolderRootNode ??
                    NodeService.CreateNew(api, App.AppInformation, api.getRubbishNode(), ContainerType.RubbishBin);

                var autoResetEvent = new AutoResetEvent(false);
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _cloudDriveViewModel.CloudDrive.FolderRootNode = cloudDriveRootNode;
                    _cloudDriveViewModel.RubbishBin.FolderRootNode = rubbishBinRootNode;
                    autoResetEvent.Set();
                });
                autoResetEvent.WaitOne();
            //}

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _cloudDriveViewModel.LoadFolders();
                //_mainPageViewModel.GetAccountDetails();

                // Enable MainPage appbar buttons
                //_mainPageViewModel.SetCommandStatus(true);

                //if (_mainPageViewModel.SpecialNavigation()) return;
            });

            // KEEP ALWAYS AT THE END OF THE METHOD, AFTER THE "LoadForlders" call
            //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
            //    // If is a newly activated account, navigates to the upgrade account page
            //    if (App.AppInformation.IsNewlyActivatedAccount)
            //        NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal, new Dictionary<string, string> { { "Pivot", "1" } });
            //    // If is the first login, navigates to the camera upload service config page
            //    else if (SettingsService.LoadSetting<bool>(SettingsResources.CameraUploadsFirstInit, true))
            //        NavigateService.NavigateTo(typeof(InitCameraUploadsPage), NavigationParameter.Normal);
            //    else if (App.AppInformation.IsStartedAsAutoUpload)
            //        NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.AutoCameraUpload);
            //});
        }

        public async override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
                timerAPI_EAGAIN.Stop());

            // If is a folder link fetch nodes
            //if (_folderLinkViewModel != null)
            //    onRequestFinishFolderLink(api, request, e);
            //else
                base.onRequestFinish(api, request, e);
        }

        public async override void onRequestStart(MegaSDK api, MRequest request)
        {
            this.isFirstAPI_EAGAIN = true;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Disable MainPage appbar buttons
                //if (_mainPageViewModel != null) _mainPageViewModel.SetCommandStatus(false);

                //ProgressService.SetProgressIndicator(true,
                //   String.Format(ProgressMessages.FetchingNodes, request.getTransferredBytes().ToStringAndSuffix()));
            });
        }

        public async override void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Starts the timer when receives the first API_EAGAIN (-3)
            if (e.getErrorCode() == MErrorType.API_EAGAIN && this.isFirstAPI_EAGAIN)
            {
                this.isFirstAPI_EAGAIN = false;
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
                    timerAPI_EAGAIN.Start());
            }

            base.onRequestTemporaryError(api, request, e);
        }

        public async override void onRequestUpdate(MegaSDK api, MRequest request)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                //ProgressService.SetProgressIndicator(true, String.Format(ProgressMessages.FetchingNodes,
                //    request.getTransferredBytes().ToStringAndSuffix()));
            });

            //if (AppMemoryController.IsThresholdExceeded(75UL.FromMBToBytes()))
            //{
            //    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //    {
            //        new CustomMessageDialog(
            //            App.ResourceLoaders.AppMessages.GetString("AM_MemoryLimitError_Title"),
            //            App.ResourceLoaders.AppMessages.GetString("AM_MemoryLimitError"),
            //            App.AppInformation,
            //            MessageDialogButtons.Ok).ShowDialogAsync();

            //        Application.Current.Exit();
            //    });

            //}
        }

        #endregion
    }
}
