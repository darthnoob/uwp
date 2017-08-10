using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseAwaitEmailConfirmationDialog : ContentDialogEx<AwaitEmailConfirmationDialogViewModel> { }

    public sealed partial class AwaitEmailConfirmationDialog : BaseAwaitEmailConfirmationDialog
    {
        public AwaitEmailConfirmationDialog(string email)
        {
            this.InitializeComponent();

            this.ViewModel.Email = email;
        }
    }
}
