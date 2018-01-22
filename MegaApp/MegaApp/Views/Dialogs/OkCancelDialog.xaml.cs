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
        /// <param name="dialogButtons">A <see cref="OkCancelDialogButtons"/> value that indicates the buttons to display.</param>
        /// <param name="primaryButton">Label of the primary button of the dialog. Default value "Ok".</param>
        /// <param name="secondaryButton">Label of the secondary button of the dialog. Default value "Cancel".</param>
        public OkCancelDialog(string title, string message, string warning = null,
            OkCancelDialogButtons dialogButtons = OkCancelDialogButtons.OkCancel,
            string primaryButton = null, string secondaryButton = null)
        {
            this.InitializeComponent();

            this.ViewModel.TitleText = title;
            this.ViewModel.MessageText = message;
            this.ViewModel.WarningText = warning ?? string.Empty;

            switch (dialogButtons)
            {
                case OkCancelDialogButtons.OkCancel:
                default:
                    this.ViewModel.PrimaryButtonLabel = this.ViewModel.OkText;
                    this.ViewModel.SecondaryButtonLabel = this.ViewModel.CancelText;
                    break;

                case OkCancelDialogButtons.YesNo:
                    this.ViewModel.PrimaryButtonLabel = this.ViewModel.YesText;
                    this.ViewModel.SecondaryButtonLabel = this.ViewModel.NoText;
                    break;

                case OkCancelDialogButtons.Custom:
                    this.ViewModel.PrimaryButtonLabel = primaryButton ?? this.ViewModel.OkText;
                    this.ViewModel.SecondaryButtonLabel = secondaryButton ?? this.ViewModel.CancelText;
                    break;
            }
        }
    }
}
