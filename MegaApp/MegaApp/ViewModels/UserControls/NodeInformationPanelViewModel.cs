using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels.UserControls
{
    public class NodeInformationPanelViewModel : BaseViewModel
    {
        public NodeInformationPanelViewModel()
        {
            this.AddContactToFolderCommand = new RelayCommand(AddContactToFolder);
            this.CopyLinkCommand = new RelayCommand(CopyLink);
            this.DecryptionKeyCommand = new RelayCommand(GetDecryptiontKey);
            this.LinkWithKeyCommand = new RelayCommand(GetLinkWithKey);
            this.LinkWithoutKeyCommand = new RelayCommand(GetLinkWithoutKey);
            this.ShareLinkCommand = new RelayCommand(ShareLink);
        }

        #region Commands

        public ICommand AddContactToFolderCommand { get; }
        public ICommand CopyLinkCommand { get; }
        public ICommand DecryptionKeyCommand { get; }
        public ICommand LinkWithKeyCommand { get; }
        public ICommand LinkWithoutKeyCommand { get; }
        public ICommand ShareLinkCommand { get; }

        #endregion

        #region Events

        public event EventHandler OpenShareToPanelEvent;

        #endregion

        #region Methods

        public void EnableLink(bool isOn)
        {
            if (isOn && !this.Node.OriginalMNode.isExported())
                this.Node.GetLinkAsync(false);
            else if (!isOn && this.Node.OriginalMNode.isExported())
                this.Node.RemoveLink();
        }

        private void GetDecryptiontKey()
        {
            this.ExportLinkBorderTitle = this.DecryptionKeyLabelText;
            this.Node.ExportLink = this.Node.OriginalMNode.getBase64Key();
        }

        private void GetLinkWithKey()
        {
            this.ExportLinkBorderTitle = this.ExportLinkText;
            this.Node.ExportLink = this.Node.OriginalMNode.getPublicLink(true);
        }

        private void GetLinkWithoutKey()
        {
            this.ExportLinkBorderTitle = this.ExportLinkText;
            this.Node.ExportLink = this.Node.OriginalMNode.getPublicLink(false);
        }

        private void CopyLink()
        {
            ShareService.CopyLinkToClipboard(this.Node.ExportLink);
        }

        private void ShareLink()
        {
            ShareService.ShareLink(this.Node.ExportLink);
        }

        public void EnableSharedFolder(bool isOn)
        {
            if (this.Node is FolderNodeViewModel == false) return;

            var folderNode = (FolderNodeViewModel)this.Node;

            if (!isOn && folderNode.IsOutShare)
            {
                if (folderNode?.RemoveSharedAccessCommand?.CanExecute(null) == true)
                    folderNode.RemoveSharedAccessCommand.Execute(null);
            }
            else if (isOn && !folderNode.IsOutShare)
            {
                this.AddContactToFolderAction(folderNode);
            }
        }

        private void AddContactToFolder()
        {
            if (this.Node is FolderNodeViewModel == false) return;

            var folderNode = (FolderNodeViewModel)this.Node;
            if (!folderNode.IsOutShare) return;

            this.AddContactToFolderAction(folderNode);
        }

        private void AddContactToFolderAction(FolderNodeViewModel folderNode)
        {
            if (ContactsService.MegaContacts.ItemCollection.HasItems)
            {
                this.OpenShareToPanelEvent?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (folderNode?.ContactsList == null)
                folderNode.ContactsList = new Contacts.ContactsListOutgoingSharedFolderViewModel(this.Node.OriginalMNode);

            if (folderNode?.ContactsList?.AddContactToFolderCommand?.CanExecute(null) == true)
                folderNode.ContactsList.AddContactToFolderCommand.Execute(null);
        }

        public void SaveForOffline(bool isOn)
        {
            if (isOn && !this.Node.IsSavedForOffline)
                this.Node.SaveForOffline();
            else if (!isOn && this.Node.IsSavedForOffline)
                this.Node.RemoveFromOffline();
        }

        #endregion

        #region Properties

        private NodeViewModel _node;
        public NodeViewModel Node
        {
            get { return _node; }
            set
            {
                SetField(ref _node, value);
                if (_node == null) return;

                OnPropertyChanged(nameof(this.IsFolder), 
                    nameof(this.FolderNode), nameof(this.SharedFolderNode), 
                    nameof(this.IsInShare), nameof(this.IsOutShare),
                    nameof(this.IsFolderLinkChild), nameof(this.ContentsOrTypeLabelText),
                    nameof(this.ContentsOrTypeText), nameof(this.DateCreatedLabelText));

                this.GetLinkWithKey();
            }
        }

        public bool IsFolder => this.Node is FolderNodeViewModel;
        public bool IsInShare => this.Node is IncomingSharedFolderNodeViewModel;
        public bool IsOutShare => this.Node is OutgoingSharedFolderNodeViewModel;
        public bool IsFolderLinkChild => this.Node?.Parent?.Type == ContainerType.FolderLink;

        public FolderNodeViewModel FolderNode => this.Node as FolderNodeViewModel;
        public SharedFolderNodeViewModel SharedFolderNode => this.Node as SharedFolderNodeViewModel;

        private string _exportLinkBorderTitle;
        public string ExportLinkBorderTitle
        {
            get { return _exportLinkBorderTitle; }
            set { SetField(ref _exportLinkBorderTitle, value); }
        }

        public bool IsLinkWithExpirationTime => this.Node?.LinkExpirationTime > 0;

        public AccountDetailsViewModel AccountDetails => AccountService.AccountDetails;

        #endregion

        #region UiResources

        // Common
        public string InformationText => ResourceService.UiResources.GetString("UI_Information");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string CopyOrMoveText => CopyText + "/" + MoveText;
        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string LeaveShareText => ResourceService.UiResources.GetString("UI_LeaveShare");
        public string ImportText => ResourceService.UiResources.GetString("UI_Import");
        public string MoveText => ResourceService.UiResources.GetString("UI_Move");
        public string PreviewText => ResourceService.UiResources.GetString("UI_Preview");
        public string RemoveText => ResourceService.UiResources.GetString("UI_Remove");
        public string RenameText => ResourceService.UiResources.GetString("UI_Rename");
        public string RemoveSharedAccessText => ResourceService.UiResources.GetString("UI_RemoveSharedAccess");

        // Details pivot
        public string ContentsLabelText => ResourceService.UiResources.GetString("UI_Contents");
        public string DateModifiedLabelText => ResourceService.UiResources.GetString("UI_DateModified");
        public string DetailsText => ResourceService.UiResources.GetString("UI_Details");
        public string FolderLocationLabelText => ResourceService.UiResources.GetString("UI_FolderLocation");        
        public string OwnerLabelText => ResourceService.UiResources.GetString("UI_Owner");
        public string PermissionsLabelText => ResourceService.UiResources.GetString("UI_Permissions");
        public string SaveForOfflineText => ResourceService.UiResources.GetString("UI_SaveForOffline");
        public string SharedOnLabelText => ResourceService.UiResources.GetString("UI_SharedOn");
        public string SharedToLabelText => ResourceService.UiResources.GetString("UI_SharedTo");
        public string SizeLabelText => ResourceService.UiResources.GetString("UI_Size");
        public string TypeLabelText => ResourceService.UiResources.GetString("UI_Type");

        public string ContentsOrTypeLabelText => this.IsFolder ? this.ContentsLabelText : this.TypeLabelText;
        public string ContentsOrTypeText => this.IsFolder ?
            (this.FolderNode != null ? this.FolderNode.Contents : string.Empty) :
            (this.Node != null ? this.Node.TypeText : string.Empty);

        public string DateCreatedLabelText => 
            (this.Node?.OriginalMNode != null && this.Node.OriginalMNode.isInShare()) ?
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
        public string AddContactToFolderText => ResourceService.UiResources.GetString("UI_AddContactToFolder");
        public string EnableSharedFolderText => ResourceService.UiResources.GetString("UI_EnableSharedFolder");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string RemoveFromFolderText => ResourceService.UiResources.GetString("UI_RemoveFromFolder");
        public string SetFolderPermissionText => ResourceService.UiResources.GetString("UI_SetFolderPermission");
        public string SharedToText => ResourceService.UiResources.GetString("UI_SharedTo");

        #endregion

        #region VisualResources

        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string CopyOrMovePathData => ResourceService.VisualResources.GetString("VR_CopyOrMovePathData");
        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string LeaveSharePathData => ResourceService.VisualResources.GetString("VR_LeaveSharePathData");
        public string ImportPathData => ResourceService.VisualResources.GetString("VR_ImportPathData");
        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");
        public string PreviewImagePathData => ResourceService.VisualResources.GetString("VR_PreviewImagePathData");
        public string RenamePathData => ResourceService.VisualResources.GetString("VR_RenamePathData");
        public string RubbishBinPathData => ResourceService.VisualResources.GetString("VR_RubbishBinPathData");
        public string ShareIconPathData => ResourceService.VisualResources.GetString("VR_ShareIconPathData");

        #endregion
    }
}
