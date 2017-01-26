using System.Threading.Tasks;

namespace MegaApp.ViewModels.Settings
{
    public class DescriptionSettingViewModel : SettingViewModel<object>
    {
        public DescriptionSettingViewModel(string title, string description)
            : base(title, description, null)
        {
            
        }

        public override object GetValue(object defaultValue)
        {
            return null;
        }

        public override Task<bool> StoreValue(string key, object value)
        {
            // Do nothing
            return Task.FromResult(true);
        }
    }
}
