using Windows.UI.Xaml.Controls;
using MegaApp.UserControls;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Views.MyAccount
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseStorageAndTransferView : UserControlEx<StorageAndTransferViewModel> { }

    public sealed partial class StorageAndTransferView : BaseStorageAndTransferView
    {
        public StorageAndTransferView()
        {
            this.InitializeComponent();
        }

        public StackPanel ViewArea => this.MainStackPanel;
    }
}
