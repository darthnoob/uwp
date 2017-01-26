using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class CameraUploadsSettingViewModel: SettingViewModel<bool>
    {
        public const string ImageDateSetting = "ImageLastUploadDate";

        public CameraUploadsSettingViewModel()
            : base(ResourceService.UiResources.GetString("UI_CameraUploadsTitle"), null, "CameraUploadsSettingsKey")
        {
            this.ValueChanged += async (sender, args) =>
            {
                await StoreValue(this.Key, this.Value);
            };
        }

        public override bool GetValue(bool defaultValue)
        {
            return TaskService.IsBackGroundTaskActive(
                TaskService.CameraUploadTaskEntryPoint,
                TaskService.CameraUploadTaskName);
        }

        public override async Task<bool> StoreValue(string key, bool value)
        {
            // Store true/false also in the local settings
            // On app start up we can check if task should be available and active by reading this value 
            await base.StoreValue(key, value);

            // Activate or deactivate the background task
            if(value)
            {
                if (!await TaskService.RequestBackgroundAccessAsync()) return false;

                TaskService.UnregisterBackgroundTask(
                    TaskService.CameraUploadTaskEntryPoint,
                    TaskService.CameraUploadTaskName);
                TaskService.RegisterBackgroundTask(
                    TaskService.CameraUploadTaskEntryPoint,
                    TaskService.CameraUploadTaskName,
                    new TimeTrigger(TaskService.CameraUploadTaskTimeTrigger, false),
                    null);

                return true;
            }

            TaskService.UnregisterBackgroundTask(
                TaskService.CameraUploadTaskEntryPoint,
                TaskService.CameraUploadTaskName);

            // Reset the date
            SettingsService.SaveSettingToFile(ImageDateSetting, DateTime.MinValue);

            return true;
        }
    }
}
