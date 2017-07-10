using Windows.UI.Xaml.Controls;

namespace MegaApp.Views.MyAccount
{
    public sealed partial class GeneralView : UserControl
    {
        public GeneralView()
        {
            this.InitializeComponent();
        }

        public StackPanel MainStackPanel => this.PART_MainStackPanel;
    }
}
