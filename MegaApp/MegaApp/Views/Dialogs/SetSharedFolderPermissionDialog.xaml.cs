using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseSetSharedFolderPermissionDialog : ContentDialogEx<SetSharedFolderPermissionDialogViewModel> { }

    public sealed partial class SetSharedFolderPermissionDialog : BaseSetSharedFolderPermissionDialog
    {
        public SetSharedFolderPermissionDialog()
        {
            this.InitializeComponent();
        }
    }
}
