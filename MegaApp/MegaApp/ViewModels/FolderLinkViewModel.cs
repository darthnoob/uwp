using mega;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.Login;

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
            this._link = link;

            if (string.IsNullOrWhiteSpace(_link)) return;

            if (_loginToFolder == null)
                _loginToFolder = new LoginToFolderRequestListenerAsync();
            _loginToFolder.ServerBusy += OnServerBusy;

            this.ControlState = false;
            this.IsBusy = true;

            this.ProgressHeaderText = ResourceService.ProgressMessages.GetString("PM_LoginToFolderHeader");
            this.ProgressText = ResourceService.ProgressMessages.GetString("PM_LoginToFolderSubHeader");

            var result = await _loginToFolder.ExecuteAsync(() =>
                this.MegaSdk.loginToFolder(_link, _loginToFolder));

            switch(result)
            {
                case LoginToFolderResult.Success:
                    if (!await this.FetchNodes()) return;
                    this.LoadFolder();
                    break;

                case LoginToFolderResult.InvalidHandleOrDecryptionKey:
                    LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Login to folder failed. Invalid handle or decryption key.");
                    this.ShowFolderLinkNoValidAlert();
                    break;

                case LoginToFolderResult.InvalidDecryptionKey:
                    this.ShowDecryptionKeyNotValidAlert();
                    break;

                case LoginToFolderResult.NoDecryptionKey:
                    this.ShowDecryptionAlert();
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

        private async void ShowDecryptionAlert()
        {
            this._loginToFolder.DecryptionAlert = true;

            var decryptionKey = await DialogService.ShowInputDialogAsync(
                ResourceService.AppMessages.GetString("AM_DecryptionKeyAlertTitle"),
                ResourceService.AppMessages.GetString("AM_DecryptionKeyAlertMessage"));

            if (string.IsNullOrWhiteSpace(decryptionKey)) return;            
            this.OpenLink(decryptionKey);
        }

        private async void ShowDecryptionKeyNotValidAlert()
        {
            var decryptionKey = await DialogService.ShowInputDialogAsync(
                ResourceService.AppMessages.GetString("AM_DecryptionKeyNotValid"),
                ResourceService.AppMessages.GetString("AM_DecryptionKeyAlertMessage"));

            if (string.IsNullOrWhiteSpace(decryptionKey)) return;
            this.OpenLink(decryptionKey);
        }

        private async void ShowFolderLinkNoValidAlert()
        {
            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_OpenLinkFailed_Title"),
                ResourceService.AppMessages.GetString("AM_InvalidLink"));
        }

        /// <summary>
        /// Open a MEGA file link providing its decryption key.        
        /// </summary>        
        /// <param name="decryptionKey">Decryption key of the link.</param>
        private void OpenLink(string decryptionKey)
        {
            string[] splittedLink = SplitLink(_link);

            // If the decryption key already includes the "!" character, delete it.
            if (decryptionKey.StartsWith("!"))
                decryptionKey = decryptionKey.Substring(1);

            string link = string.Format("{0}!{1}!{2}", splittedLink[0],
                splittedLink[1], decryptionKey);

            // If is a folder link
            if (splittedLink[0].EndsWith("#F"))
                this.LoginToFolder(link);
            //else
                //api.getPublicNode(link, this);
        }

        /// <summary>
        /// Split the MEGA link in its three parts, separated by the "!" chartacter.
        /// <para>1. MEGA Url address.</para>
        /// <para>2. Node handle.</para>
        /// <para>3. Decryption key.</para>
        /// </summary>        
        /// <param name="link">Link to split.</param>
        /// <returns>Char array with the parts of the link.</returns>
        private string[] SplitLink(string link)
        {
            string delimStr = "!";
            return link.Split(delimStr.ToCharArray(), 3);
        }

        #endregion

        #region Properties

        private string _link;

        private LoginToFolderRequestListenerAsync _loginToFolder;

        private FolderViewModel _folderLink;
        public FolderViewModel FolderLink
        {
            get { return _folderLink; }
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
