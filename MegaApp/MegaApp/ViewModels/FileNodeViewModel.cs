using System.Collections.ObjectModel;
using System.IO;
using mega;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class FileNodeViewModel: NodeViewModel
    {
        public FileNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, FolderViewModel parent,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parent, parentCollection, childCollection)
        {
            SizeText = Size.ToStringAndSuffix(1);
            Transfer = new TransferObjectModel(this, MTransferType.TYPE_DOWNLOAD, LocalDownloadPath);
        }

        #region Override Methods

        public override async void Open()
        {
            await FileService.OpenFile(LocalDownloadPath);
        }

        #endregion

        #region Public Methods

        public void SetFile()
        {
            if (!FileService.FileExists(LocalDownloadPath))
                Transfer.StartTransfer();
            else
                DefaultImagePathData = ImageService.GetDefaultFileTypePathData(Name);
        }

        #endregion

        #region Properties

        public virtual string FileType => Path.GetExtension(Name).ToUpper().Replace(".", string.Empty);

        #endregion
    }
}
