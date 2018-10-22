using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.Login;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    public class FolderLinkViewModel : LoginViewModel
    {
        public FolderLinkViewModel() : base(SdkService.MegaSdkFolderLinks)
        {
            InitializeModel();
        }

        #region Methods

        private void InitializeModel()
        {
            this.FolderLink = new FolderViewModel(this.MegaSdk, ContainerType.FolderLink);
        }

        public async void LoginToFolder(string link)
        {
            if (string.IsNullOrWhiteSpace(link)) return;

            PublicLinkService.Link = link;

            if (_loginToFolder == null)
            {
                _loginToFolder = new LoginToFolderRequestListenerAsync();
                _loginToFolder.IsWaiting += OnIsWaiting;
            }

            this.ControlState = false;
            this.IsBusy = true;

            this.ProgressHeaderText = ResourceService.ProgressMessages.GetString("PM_LoginToFolderHeader");
            this.ProgressText = ResourceService.ProgressMessages.GetString("PM_LoginToFolderSubHeader");

            var result = await _loginToFolder.ExecuteAsync(() =>
                this.MegaSdk.loginToFolder(PublicLinkService.Link, _loginToFolder));

            bool navigateBack = true;
            switch(result)
            {
                case LoginToFolderResult.Success:
                    if (!await this.FetchNodesFromFolder()) break;
                    this.LoadFolder();
                    navigateBack = false;
                    break;

                case LoginToFolderResult.InvalidHandleOrDecryptionKey:
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Login to folder failed. Invalid handle or decryption key.");
                    PublicLinkService.ShowLinkNoValidAlert();
                    break;

                case LoginToFolderResult.InvalidDecryptionKey:
                    PublicLinkService.Link = await PublicLinkService.ShowDecryptionKeyNotValidAlertAsync();
                    if (PublicLinkService.Link != null)
                    {
                        this.LoginToFolder(PublicLinkService.Link);
                        return;
                    }
                    break;

                case LoginToFolderResult.NoDecryptionKey:
                    PublicLinkService.Link = await PublicLinkService.ShowDecryptionAlertAsync();
                    this._loginToFolder.DecryptionAlert = true;
                    if (PublicLinkService.Link != null)
                    {
                        this.LoginToFolder(PublicLinkService.Link);
                        return;
                    }
                    break;

                case LoginToFolderResult.Unknown:
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Login to folder failed.");
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_LoginToFolderFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_LoginToFolderFailed"));
                    break;
            }

            this.ControlState = true;
            this.IsBusy = false;

            if (!navigateBack) return;

            OnUiThread(() =>
            {
                // Navigate to the Cloud Drive page
                NavigateService.Instance.Navigate(typeof(CloudDrivePage), false,
                    NavigationObject.Create(this.GetType()));
            });
        }

        /// <summary>
        /// Log out from the folder
        /// </summary>
        public async void LogoutFromFolder()
        {
            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Logging out from folder");

            var logoutFromFolder = new LogOutRequestListenerAsync();
            var result = await logoutFromFolder.ExecuteAsync(() =>
                this.MegaSdk.logout(logoutFromFolder));

            if (!result)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error logging out from folder");
                return;
            }
                
            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Logged out from folder");
        }

        private async Task<bool> FetchNodesFromFolder()
        {
            var fetchNodesResult = await this.FetchNodes();
            switch(fetchNodesResult)
            {
                case FetchNodesResult.Success:
                    // Save the handle of the last public node accessed (Task #10801)
                    SettingsService.SaveLastPublicNodeHandle(this.FolderLink.FolderRootNode.Handle);
                    return true;

                case FetchNodesResult.InvalidHandleOrDecryptionKey:
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING,
                        "Fetch nodes from folder link failed. Invalid handle or decryption key.");
                    PublicLinkService.ShowLinkNoValidAlert();
                    break;

                case FetchNodesResult.InvalidDecryptionKey:
                    PublicLinkService.Link = await PublicLinkService.ShowDecryptionKeyNotValidAlertAsync();
                    if (PublicLinkService.Link != null)
                        this.LoginToFolder(PublicLinkService.Link);
                    break;

                case FetchNodesResult.NoDecryptionKey:
                    PublicLinkService.Link = await PublicLinkService.ShowDecryptionAlertAsync();
                    this._loginToFolder.DecryptionAlert = true;
                    if (PublicLinkService.Link != null)
                        this.LoginToFolder(PublicLinkService.Link);
                    break;

                case FetchNodesResult.UnavailableLink:
                    this.ShowUnavailableFolderLinkAlert();
                    break;

                case FetchNodesResult.AssociatedUserAccountTerminated:
                    PublicLinkService.ShowAssociatedUserAccountTerminatedAlert();
                    break;

                case FetchNodesResult.Unknown:
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Fetch nodes failed.");
                    this.ShowFetchNodesFailedAlertDialog();
                    break;
            }

            return false;
        }

        private void LoadFolder()
        {
            if (this.FolderLink.FolderRootNode == null)
            {
                this.FolderLink.FolderRootNode = NodeService.CreateNew(
                    this.MegaSdk, App.AppInformation,
                    this.MegaSdk.getRootNode(), this.FolderLink);
            }

            // Store the absolute root node of the folder link
            if (this.FolderLinkRootNode == null)
                this.FolderLinkRootNode = this.FolderLink.FolderRootNode as FolderNodeViewModel;

            this.FolderLink.LoadChildNodes();
        }

        private async void ShowUnavailableFolderLinkAlert()
        {
            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_LinkUnavailableTitle"),
                ResourceService.AppMessages.GetString("AM_FolderLinkUnavailable"));
        }

        #endregion

        #region Properties

        private LoginToFolderRequestListenerAsync _loginToFolder;

        private FolderViewModel _folderLink;
        public FolderViewModel FolderLink
        {
            get
            {
                if (_folderLink == null)
                    _folderLink = new FolderViewModel(this.MegaSdk, ContainerType.FolderLink);

                if (_folderLink.FolderRootNode == null)
                {
                    _folderLink.FolderRootNode = NodeService.CreateNew(this.MegaSdk,
                        App.AppInformation,this.MegaSdk.getRootNode(), _folderLink);
                }

                return _folderLink;
            }

            private set { SetField(ref _folderLink, value); }
        }

        /// <summary>
        /// Property to store the absolute root node of the folder link.
        /// <para>Used for example to download/import all the folder link.</para>
        /// </summary>        
        private FolderNodeViewModel _folderLinkRootNode;
        public FolderNodeViewModel FolderLinkRootNode
        {
            get { return _folderLinkRootNode; }
            set { SetField(ref _folderLinkRootNode, value); }
        }

        #endregion

        #region UiResources

        public string SectionNameText => ResourceService.UiResources.GetString("UI_FolderLink");

        #endregion
    }
}
