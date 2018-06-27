using System.Collections.Generic;
using MegaApp.Interfaces;

namespace MegaApp.ViewModels.Settings
{
    public class SettingSectionViewModel: BaseUiViewModel
    {
        public SettingSectionViewModel()
        {
            this.Items = new List<ISetting>();
        }

        public void Initialize()
        {
            foreach (var setting in this.Items)
            {
                setting.Initialize();
            }
        }

        #region Properties

        public string Header { get; set; }

        public string Description { get; set; }

        public IList<ISetting> Items { get; }

        #endregion
    }
}
