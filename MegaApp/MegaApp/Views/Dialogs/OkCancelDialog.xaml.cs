using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseOkCancelDialog : ContentDialogEx<OkCancelDialogViewModel> { }

    public sealed partial class OkCancelDialog : BaseOkCancelDialog
    {
        /// <summary>
        /// Creates a dialog with a message, a warning and two buttons.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Message of the dialog.</param>
        /// <param name="warning">Optinal warning message of the dialog (optional).</param>
        /// <param name="primaryButton">Label of the primary button of the dialog. Default value "Ok".</param>
        /// <param name="secondaryButton">Label of the secondary button of the dialog. Default value "Cancel".</param>
        public OkCancelDialog(string title, string message, string warning = null,
            string primaryButton = null, string secondaryButton = null)
        {
            this.InitializeComponent();

            this.ViewModel.TitleText = title;
            this.ViewModel.MessageText = message;
            this.ViewModel.WarningText = warning ?? string.Empty;
            this.ViewModel.PrimaryButtonLabel = primaryButton ?? this.ViewModel.OkText;
            this.ViewModel.SecondaryButtonLabel = secondaryButton ?? this.ViewModel.CancelText;
        }
    }
}
