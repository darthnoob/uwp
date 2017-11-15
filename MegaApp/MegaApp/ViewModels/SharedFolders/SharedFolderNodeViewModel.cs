using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.Contacts;

namespace MegaApp.ViewModels.SharedFolders
{
    public abstract class SharedFolderNodeViewModel : FolderNodeViewModel, IMegaSharedFolderNode
    {
        public SharedFolderNodeViewModel(MNode megaNode, SharedFoldersListViewModel parent)
            : base(SdkService.MegaSdk, App.AppInformation, megaNode, null)
        {
            this.Parent = parent;

            this.DownloadCommand = new RelayCommand(Download);
            this.OpenContentPanelCommand = new RelayCommand(OpenContentPanel);
            this.OpenInformationPanelCommand = new RelayCommand(OpenInformationPanel);

            this.Update(megaNode);
        }

        #region Commands

        public ICommand LeaveShareCommand { get; set; }
        public ICommand RemoveSharedAccessCommand { get; set; }

        public ICommand OpenContentPanelCommand { get; }
        public ICommand OpenInformationPanelCommand { get; }

        #endregion

        #region Methods

        private void ParentItemCollectionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Parent.ItemCollection.OnlyOneSelectedItem))
                OnPropertyChanged(nameof(this.OnlyOneSelectedItem));
        }

        private void Download()
        {
            if (this.Parent.ItemCollection.IsMultiSelectActive)
            {
                if (this.Parent.DownloadCommand.CanExecute(null))
                    this.Parent.DownloadCommand.Execute(null);
                return;
            }

            base.Download(TransferService.MegaTransfers);
        }

        private void OpenInformationPanel()
        {
            if (this.Parent.ItemCollection.OnlyOneSelectedItem)
            {
                if (this.Parent.OpenInformationPanelCommand.CanExecute(null))
                    this.Parent.OpenInformationPanelCommand.Execute(null);
            }
        }

        private void OpenContentPanel()
        {
            if (this.Parent.ItemCollection.OnlyOneSelectedItem)
            {
                if (this.Parent.OpenContentPanelCommand.CanExecute(null))
                    this.Parent.OpenContentPanelCommand.Execute(null);
            }
        }

        public async Task<bool> RemoveSharedAccessAsync()
        {
            var removeSharedAccess = new ShareRequestListenerAsync();
            var outShares = SdkService.MegaSdk.getOutShares(this.OriginalMNode);
            var outSharesSize = outShares.size();
            bool result = true;
            for (int i = 0; i < outSharesSize; i++)
            {
                result = result & await removeSharedAccess.ExecuteAsync(() =>
                {
                    this.MegaSdk.shareByEmail(this.OriginalMNode, outShares.get(i).getUser(),
                        (int)MShareType.ACCESS_UNKNOWN, removeSharedAccess);
                });
            }

            return result;
        }

        #endregion

        #region Properties

        private SharedFoldersListViewModel _parent;
        protected new SharedFoldersListViewModel Parent
        {
            get { return _parent; }
            set
            {
                if (_parent?.ItemCollection != null)
                    _parent.ItemCollection.PropertyChanged -= ParentItemCollectionOnPropertyChanged;

                SetField(ref _parent, value);

                if (_parent?.ItemCollection != null)
                    _parent.ItemCollection.PropertyChanged += ParentItemCollectionOnPropertyChanged;
            }
        }

        public bool OnlyOneSelectedItem => this.Parent.ItemCollection.OnlyOneSelectedItem;

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Owner of the shared folder
        /// </summary>
        public virtual string Owner { get; set; }

        /// <summary>
        /// Folder location of the shared folder
        /// </summary>
        public virtual string FolderLocation { get; set; }

        /// <summary>
        /// List of contacts with the folder is shared
        /// </summary>
        public virtual ContactsListOutgoingSharedFolderViewModel ContactsList { get; set; }

        public virtual string ContactsText { get; set; }

        #endregion

        #region UiResources

        public string InformationText => ResourceService.UiResources.GetString("UI_Information");
        public string OpenText => ResourceService.UiResources.GetString("UI_Open");

        #endregion
    }
}
