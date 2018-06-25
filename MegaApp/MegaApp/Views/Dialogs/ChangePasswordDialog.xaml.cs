using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseChangePasswordDialog : ContentDialogEx<ChangePasswordDialogViewModel> { }

    public sealed partial class ChangePasswordDialog : BaseChangePasswordDialog
    {
        public ChangePasswordDialog()
        {
            this.InitializeComponent();
        }
    }
}
