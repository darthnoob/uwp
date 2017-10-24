using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels.UserControls
{
    public class SharedFolderInformationPanelViewModel : BaseViewModel
    {
        public SharedFolderInformationPanelViewModel()
        {
            this.CopyLinkCommand = new RelayCommand(CopyLink);
            this.DecryptionKeyCommand = new RelayCommand(GetDecryptiontKey);
            this.LinkWithKeyCommand = new RelayCommand(GetLinkWithKey);
            this.LinkWithoutKeyCommand = new RelayCommand(GetLinkWithoutKey);
            this.ShareLinkCommand = new RelayCommand(ShareLink);
        }

        #region Commands

        public ICommand CopyLinkCommand { get; }
        public ICommand DecryptionKeyCommand { get; }
        public ICommand LinkWithKeyCommand { get; }
        public ICommand LinkWithoutKeyCommand { get; }
        public ICommand ShareLinkCommand { get; }

        #endregion

        #region Methods

        public void EnableLink(bool isOn)
        {
            if (isOn && !this.SharedFolder.OriginalMNode.isExported())
                this.SharedFolder.GetLinkAsync(false);
            else if (!isOn && this.SharedFolder.OriginalMNode.isExported())
                this.SharedFolder.RemoveLink();
        }

        private void GetDecryptiontKey()
        {
            this.ExportLinkBorderTitle = this.DecryptionKeyLabelText;
            this.SharedFolder.ExportLink = this.SharedFolder.OriginalMNode.getBase64Key();
        }

        private void GetLinkWithKey()
        {
            this.ExportLinkBorderTitle = this.ExportLinkText;
            this.SharedFolder.ExportLink = this.SharedFolder.OriginalMNode.getPublicLink(true);
        }

        private void GetLinkWithoutKey()
        {
            this.ExportLinkBorderTitle = this.ExportLinkText;
            this.SharedFolder.ExportLink = this.SharedFolder.OriginalMNode.getPublicLink(false);
        }

        private void CopyLink()
        {
            ShareService.CopyLinkToClipboard(this.SharedFolder.ExportLink);
        }

        private void ShareLink()
        {
            ShareService.ShareLink(this.SharedFolder.ExportLink);
        }

        public void EnableSharedFolder(bool isOn)
        {
            if (!isOn)
            {
                if (this.SharedFolder?.RemoveSharedAccessCommand?.CanExecute(null) == true)
                    this.SharedFolder.RemoveSharedAccessCommand.Execute(null);
            }

            OnPropertyChanged(nameof(this.IsOutShare));
        }

        #endregion

        #region Properties

        private SharedFolderNodeViewModel _sharedFolder;
        public SharedFolderNodeViewModel SharedFolder
        {
            get { return _sharedFolder; }
            set
            {
                SetField(ref _sharedFolder, value);

                OnPropertyChanged(nameof(this.IsInShare),
                    nameof(this.IsOutShare),
                    nameof(this.DateCreatedLabelText));

                this.GetLinkWithKey();
            }
        }

        public bool IsInShare => (this.SharedFolder?.OriginalMNode != null) ?
            this.SharedFolder.OriginalMNode.isInShare() : false;

        public bool IsOutShare => (this.SharedFolder?.OriginalMNode != null) ?
            this.SharedFolder.OriginalMNode.isOutShare() : false;

        private string _exportLinkBorderTitle;
        public string ExportLinkBorderTitle
        {
            get { return _exportLinkBorderTitle; }
            set { SetField(ref _exportLinkBorderTitle, value); }
        }

        public bool IsLinkWithExpirationTime => 
            (this.SharedFolder?.LinkExpirationTime > 0) ? true : false;

        public AccountDetailsViewModel AccountDetails => AccountService.AccountDetails;

        #endregion

        #region UiResources

        // Common
        public string InformationText => ResourceService.UiResources.GetString("UI_Information");
        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string LeaveShareText => ResourceService.UiResources.GetString("UI_LeaveShare");
        public string RemoveSharedAccessText => ResourceService.UiResources.GetString("UI_RemoveSharedAccess");

        // Details pivot
        public string ContentsLabelText => ResourceService.UiResources.GetString("UI_Contents");
        public string DateModifiedLabelText => ResourceService.UiResources.GetString("UI_DateModified");
        public string DetailsText => ResourceService.UiResources.GetString("UI_Details");
        public string FolderLocationLabelText => ResourceService.UiResources.GetString("UI_FolderLocation");        
        public string OwnerLabelText => ResourceService.UiResources.GetString("UI_Owner");
        public string PermissionsLabelText => ResourceService.UiResources.GetString("UI_Permissions");
        public string SharedOnLabelText => ResourceService.UiResources.GetString("UI_SharedOn");
        public string SharedToLabelText => ResourceService.UiResources.GetString("UI_SharedTo");
        public string SizeLabelText => ResourceService.UiResources.GetString("UI_Size");

        public string DateCreatedLabelText => 
            (this.SharedFolder?.OriginalMNode != null && this.SharedFolder.OriginalMNode.isInShare()) ?
            ResourceService.UiResources.GetString("UI_SharedOn") :
            ResourceService.UiResources.GetString("UI_DateCreated");

        // Link pivot
        public string CopyText => ResourceService.UiResources.GetString("UI_Copy");
        public string DecryptionKeyLabelText => ResourceService.UiResources.GetString("UI_DecryptionKey");
        public string EnableLinkText => ResourceService.UiResources.GetString("UI_EnableLink");
        public string ExportLinkText => ResourceService.UiResources.GetString("UI_ExportLink");
        public string LinkText => ResourceService.UiResources.GetString("UI_Link");
        public string LinkWithKeyLabelText => ResourceService.UiResources.GetString("UI_LinkWithKey");
        public string LinkWithoutKeyLabelText => ResourceService.UiResources.GetString("UI_LinkWithoutKey");
        public string ShareText => ResourceService.UiResources.GetString("UI_Share");

        public string SetLinkExpirationDateText => string.Format("{0} {1}",
            ResourceService.UiResources.GetString("UI_SetExpirationDate"),
            ResourceService.UiResources.GetString("UI_ProOnly"));

        // Share pivot
        public string AddContactText => ResourceService.UiResources.GetString("UI_AddContact");
        public string EnableSharedFolderText => ResourceService.UiResources.GetString("UI_EnableSharedFolder");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string SharedToText => ResourceService.UiResources.GetString("UI_SharedTo");

        #endregion

        #region VisualResources

        public string AddContactPathData => ResourceService.VisualResources.GetString("VR_AddContactPathData");
        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string LeaveSharePathData => ResourceService.VisualResources.GetString("VR_LeaveSharePathData");
        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");

        #endregion
    }
}
