using Windows.UI.Xaml.Controls;

namespace MegaApp.UserControls
{
    /// <summary>
    /// Frame extension 
    ///  </summary>
    public class FrameEx: Frame
    {
        /// <summary>
        /// Content of Frame cast as PageEx type
        /// </summary>
        public PageEx ContentPage => this.Content as PageEx;
    }
}
