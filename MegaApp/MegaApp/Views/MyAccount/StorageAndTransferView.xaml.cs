using Windows.UI.Xaml.Controls;

namespace MegaApp.Views.MyAccount
{
    public sealed partial class StorageAndTransferView : UserControl
    {
        public StorageAndTransferView()
        {
            this.InitializeComponent();
        }

        public StackPanel MainStackPanel => this.PART_MainStackPanel;
    }
}
