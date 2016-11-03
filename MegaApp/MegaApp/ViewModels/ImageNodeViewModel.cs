using System.Collections.ObjectModel;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    class ImageNodeViewModel: FileNodeViewModel
    {
        public ImageNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, ContainerType parentContainerType,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parentContainerType, parentCollection, childCollection)
        {
            DefaultImagePathData = ImageService.GetDefaultFileTypePathData(Name);
        }
    }
}
