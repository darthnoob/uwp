using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseChangeEmailDialog : ContentDialogEx<ChangeEmailDialogViewModel> { }

    public sealed partial class ChangeEmailDialog : BaseChangeEmailDialog
    {
        public ChangeEmailDialog()
        {
            this.InitializeComponent();
        }
    }
}
