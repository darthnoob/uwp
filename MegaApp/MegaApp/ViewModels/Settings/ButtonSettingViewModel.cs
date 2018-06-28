using System;

namespace MegaApp.ViewModels.Settings
{
    public class ButtonSettingViewModel : SettingViewModel<string>
    {
        public ButtonSettingViewModel(string title, string description, 
            string buttonLabel, Action buttonAction = null) : base(title, description, null)
        {
            this.ButtonLabel = buttonLabel;
            this.ButtonAction = buttonAction;
        }

        protected override void DoAction() => this.ButtonAction?.Invoke();

        public override string GetValue(string defaultValue) => defaultValue;

        #region Properties

        private string _buttonLabel;
        /// <summary>
        /// Label of the button
        /// </summary>
        public string ButtonLabel
        {
            get { return _buttonLabel; }
            set { SetField(ref _buttonLabel, value); }
        }

        protected Action ButtonAction;

        #endregion
    }
}
