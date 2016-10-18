using System.Collections.ObjectModel;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;

namespace MegaApp.ViewModels
{
    class FileNodeViewModel: NodeViewModel
    {
        public FileNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, ContainerType parentContainerType,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parentContainerType, parentCollection, childCollection)
        {
            
        }
    }
}
