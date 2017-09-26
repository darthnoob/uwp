using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseAlertDialog : ContentDialogEx<AlertDialogViewModel> { }

    public sealed partial class AlertDialog : BaseAlertDialog
    {
        public AlertDialog(string title, string message, string button)
        {
            this.InitializeComponent();

            this.ViewModel.Title = title;
            this.ViewModel.Message = message;
            this.ViewModel.ButtonLabel = button;
        }
    }
}
