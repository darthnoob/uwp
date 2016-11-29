using System.Collections.ObjectModel;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    class FileNodeViewModel: NodeViewModel
    {
        public FileNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, ContainerType parentContainerType,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parentContainerType, parentCollection, childCollection)
        {
            Information = Size.ToStringAndSuffix();
            Transfer = new TransferObjectModel(this, TransferType.Download, LocalDownloadPath);
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
    }
}
