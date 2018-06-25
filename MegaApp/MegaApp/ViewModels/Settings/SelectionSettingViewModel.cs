using System.Collections.Generic;
using System.Linq;

namespace MegaApp.ViewModels.Settings
{
    public class SelectionSettingViewModel: SettingViewModel<int>
    {
        public SelectionSettingViewModel(string title, string description, string key, IList<SelectionOption> options) 
                : base(title, description, key)
        {
            this.Options = options;
        }

        protected override void DoAction()
        {
            var newValue = this.Options.First(o => o.IsSelected).Value;
            if(this.Value == newValue) return;
            this.Value = newValue;
            base.DoAction();
        }

        public override void Initialize()
        {
            base.Initialize();
            SetSelected(this.Value);
        }

        private void SetSelected(int value)
        {
            var option = this.Options.FirstOrDefault(o => o.Value == value);
            if (option == null) return;
            option.IsSelected = true;
        }

        public IList<SelectionOption> Options { get; }

        public class SelectionOption
        {
            public string Description { get; set; }

            public int Value { get; set; }
          
            public bool IsSelected { get; set; }

        }
    }
}
