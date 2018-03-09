using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseAlertDialog : ContentDialogEx<AlertDialogViewModel> { }

    public sealed partial class AlertDialog : BaseAlertDialog
    {
        /// <summary>
        /// Creates an standard alert dialog.
        /// </summary>
        /// <param name="title">Title of the alert dialog.</param>
        /// <param name="message">Message of the alert dialog.</param>
        /// <param name="button">Label of the button of the alert dialog. Default value "Ok".</param>
        public AlertDialog(string title, string message, string button = null)
        {
            this.InitializeComponent();

            this.ViewModel.TitleText = title;
            this.ViewModel.MessageText = message;
            this.ViewModel.ButtonLabel = button ?? this.ViewModel.OkText;
        }
    }
}
