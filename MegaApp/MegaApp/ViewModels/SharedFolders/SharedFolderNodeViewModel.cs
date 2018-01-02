using System.ComponentModel;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.SharedFolders
{
    public abstract class SharedFolderNodeViewModel : FolderNodeViewModel, IMegaSharedFolderNode
    {
        protected SharedFolderNodeViewModel(MNode megaNode, SharedFoldersListViewModel parent)
            : base(SdkService.MegaSdk, App.AppInformation, megaNode, parent)
        {
            this.Parent = parent;

            this.DownloadCommand = new RelayCommand(Download);
            this.OpenContentPanelCommand = new RelayCommand(OpenContentPanel);
            this.OpenInformationPanelCommand = new RelayCommand(OpenInformationPanel);
        }

        #region Commands

        public ICommand LeaveShareCommand { get; set; }

        public ICommand OpenContentPanelCommand { get; }

        #endregion

        #region Methods

        private void OnParentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.Parent.ItemCollection.OnlyOneSelectedItem):
                    OnPropertyChanged(nameof(this.OnlyOneSelectedItem));
                    break;

                case nameof(this.Parent.IsPanelOpen):
                    OnPropertyChanged(nameof(this.IsPanelOpen));
                    break;
            }
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
            if (!this.Parent.ItemCollection.OnlyOneSelectedItem) return;
            if (this.Parent.OpenInformationPanelCommand.CanExecute(null))
                this.Parent.OpenInformationPanelCommand.Execute(null);
        }

        private void OpenContentPanel()
        {
            if (!this.Parent.ItemCollection.OnlyOneSelectedItem) return;
            if (this.Parent.OpenContentPanelCommand.CanExecute(null))
                this.Parent.OpenContentPanelCommand.Execute(null);
        }

        #endregion

        #region Properties

        private SharedFoldersListViewModel _parent;
        protected new SharedFoldersListViewModel Parent
        {
            get { return _parent; }
            set
            {
                if(_parent != null)
                {
                    _parent.PropertyChanged -= OnParentPropertyChanged;
                    if (_parent.ItemCollection != null)
                        _parent.ItemCollection.PropertyChanged -= OnParentPropertyChanged;
                }

                SetField(ref _parent, value);

                if (_parent == null) return;
                _parent.PropertyChanged += OnParentPropertyChanged;
                if (_parent.ItemCollection != null)
                    _parent.ItemCollection.PropertyChanged += OnParentPropertyChanged;
            }
        }

        public bool OnlyOneSelectedItem => this.Parent.ItemCollection.OnlyOneSelectedItem;
        public bool IsPanelOpen => this.Parent.IsPanelOpen;

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

        public virtual string ContactsText { get; set; }

        #endregion

        #region UiResources

        public string OpenText => ResourceService.UiResources.GetString("UI_Open");

        #endregion
    }
}
