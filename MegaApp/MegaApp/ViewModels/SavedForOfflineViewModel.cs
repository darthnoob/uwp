using System.IO;
using MegaApp.Services;
using MegaApp.ViewModels.Offline;

namespace MegaApp.ViewModels
{
    public class SavedForOfflineViewModel : BaseSdkViewModel
    {
        public SavedForOfflineViewModel() : base(SdkService.MegaSdk)
        {
            this.SavedForOffline = new OfflineFolderViewModel();

            if (this.SavedForOffline.FolderRootNode == null)
            {
                this.SavedForOffline.FolderRootNode =
                    new OfflineFolderNodeViewModel(new DirectoryInfo(AppService.GetOfflineDirectoryPath()));
            }

            this.SavedForOffline.LoadChildNodes();
        }

        #region Properties

        private OfflineFolderViewModel _savedForOffline;
        public OfflineFolderViewModel SavedForOffline
        {
            get { return _savedForOffline; }
            set { SetField(ref _savedForOffline, value); }
        }

        #endregion

        #region UiResources

        public string SavedForOfflineText => ResourceService.UiResources.GetString("UI_SavedForOffline");

        #endregion
    }
}
