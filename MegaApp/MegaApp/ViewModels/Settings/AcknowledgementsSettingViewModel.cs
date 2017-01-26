using System;
using System.Collections.Generic;

namespace MegaApp.ViewModels.Settings
{
    public class AcknowledgementsSettingViewModel : SettingViewModel<Dictionary<string, Uri>>
    {
        public AcknowledgementsSettingViewModel()
            : base("Acknowledgements", null, null)
        {
            
        }

        public override Dictionary<string, Uri> GetValue(Dictionary<string, Uri> defaultValue)
        {
            return new Dictionary<string, Uri>
            {
                { "GoedWare Developer", new Uri("https://www.goedware.com")},
            };
        }
    }
}
