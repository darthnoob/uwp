using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using mega;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class CameraUploadsSettingViewModel: SettingViewModel<bool>
    {
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
                CameraUploadService.TaskEntryPoint,
                CameraUploadService.TaskName);
        }

        public override async Task<bool> StoreValue(string key, bool value)
        {
            // Store true/false also in the local settings
            // On app start up we can check if task should be available and active by reading this value 
            var result = await base.StoreValue(key, value);

            // Activate or deactivate the background task
            return result & await CameraUploadService.SetBackgroundTaskAsync(value);
        }
    }
}
