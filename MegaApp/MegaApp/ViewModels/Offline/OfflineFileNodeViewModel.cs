using System;
using System.Collections.ObjectModel;
using System.IO;
using mega;
using MegaApp.Database;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.Offline
{
    public class OfflineFileNodeViewModel : OfflineNodeViewModel
    {
        public OfflineFileNodeViewModel(FileInfo fileInfo, OfflineFolderViewModel parent,
            ObservableCollection<IBaseNode> parentCollection = null, ObservableCollection<IBaseNode> childCollection = null)
            : base(parent, parentCollection, childCollection)
        {
            Update(fileInfo);
            
            this.IsDefaultImage = true;
            this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(this.Name);
        }

        #region Methods

        public override async void Open()
        {
            if (!await FileService.OpenFile(this.NodePath))
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_OpenFileFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_OpenFileFailed"));
                });
            }
        }

        public void Update(FileInfo fileInfo)
        {
            try
            {
                this.Base64Handle = "0";
                var existingNode = SavedForOfflineDB.SelectNodeByLocalPath(fileInfo.FullName);
                if (existingNode != null)
                    this.Base64Handle = existingNode.Base64Handle;

                this.Name = fileInfo.Name;
                this.NodePath = fileInfo.FullName;
                this.Size = Convert.ToUInt64(fileInfo.Length);
                this.IsFolder = false;
                this.CreationTime = fileInfo.CreationTime.DateToString();
                this.ModificationTime = fileInfo.LastWriteTime.DateToString();

                SetDefaultValues();
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error updating offline node info", e);
            }
        }

        #endregion
    }
}
