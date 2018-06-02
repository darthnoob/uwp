using System;
using Windows.System;

namespace MegaApp.ViewModels.Settings
{
    public class LinkSettingViewModel : SettingViewModel<string>
    {
        public LinkSettingViewModel(string title, string text, string link)
            : base(title, null, null)
        {
            this._text = text;
            this._link = link;
        }

        protected override async void DoAction()
        {
            await Launcher.LaunchUriAsync(new Uri(this._link,
                UriKind.RelativeOrAbsolute));
        }

        public override string GetValue(string defaultValue)
        {
            return this._text;
        }

        #region Properties

        private string _text;
        private string _link;

        #endregion
    }
}
