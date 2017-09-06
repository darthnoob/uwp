using mega;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class OutgoingSharedFolderNodeViewModel : FolderNodeViewModel
    {
        public OutgoingSharedFolderNodeViewModel(MNode megaNode, FolderViewModel parent)
            : base(SdkService.MegaSdk, App.AppInformation, megaNode, parent)
        {
            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_OutgoingSharedFolderPathData");
        }
    }
}
