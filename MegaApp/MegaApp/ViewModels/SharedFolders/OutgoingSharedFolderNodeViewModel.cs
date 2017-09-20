using mega;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class OutgoingSharedFolderNodeViewModel : FolderNodeViewModel
    {
        public OutgoingSharedFolderNodeViewModel(MNode megaNode, SharedFoldersListViewModel parent)
            : base(SdkService.MegaSdk, App.AppInformation, megaNode, null)
        {
            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_OutgoingSharedFolderPathData");
        }
    }
}
