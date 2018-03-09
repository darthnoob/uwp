using System.Collections.ObjectModel;
using System.IO;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class FileNodeViewModel: NodeViewModel
    {
        public FileNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, FolderViewModel parent,
            ObservableCollection<IBaseNode> parentCollection = null, ObservableCollection<IBaseNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parent, parentCollection, childCollection)
        {
            this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(Name);
        }

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
