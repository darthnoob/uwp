using System.Collections.Generic;
using System.Threading.Tasks;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class CameraUploadsSelectionSettingViewModel: SelectionSettingViewModel
    {
        public CameraUploadsSelectionSettingViewModel(string title, string description, string key, IList<SelectionOption> options) 
                : base(title, description, key, options)
        {}

        public override Task<bool> StoreValue(string key, int value)
        {
            SettingsService.SaveSettingToFile(key, value);
            return base.StoreValue(key, value);
        }
    }
}
