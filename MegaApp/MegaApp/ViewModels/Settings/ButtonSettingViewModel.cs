using System;

namespace MegaApp.ViewModels.Settings
{
    public class ButtonSettingViewModel : SettingViewModel<string>
    {
        public ButtonSettingViewModel(string title, string description, 
            string buttonLabel, Action buttonAction) : base(title, description, null)
        {
            this._buttonLabel = buttonLabel;
            this._buttonAction = buttonAction;
        }

        protected override void DoAction()
        {
            this._buttonAction?.Invoke();
        }

        public override string GetValue(string defaultValue)
        {
            return this._buttonLabel;
        }

        #region Properties

        private string _buttonLabel;
        private Action _buttonAction;

        #endregion
    }
}
