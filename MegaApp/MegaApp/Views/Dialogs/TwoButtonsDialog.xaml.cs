using Windows.UI.Xaml;
using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseTwoButtonsDialog : ContentDialogEx<TwoButtonsDialogViewModel> { }

    public sealed partial class TwoButtonsDialog : BaseTwoButtonsDialog
    {
        /// <summary>
        /// Creates a dialog with a message, a warning and two buttons.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Message of the dialog.</param>
        /// <param name="warning">Optinal warning message of the dialog (optional).</param>
        /// <param name="dialogType">A <see cref="TwoButtonsDialogType"/> value that indicates the type (buttons to display).</param>
        /// <param name="primaryButton">Label of the primary button of the dialog. Default value "Ok".</param>
        /// <param name="secondaryButton">Label of the secondary button of the dialog. Default value "Cancel".</param>
        /// <param name="hasCloseButton">Indicate if the dialog should show a top right corner "close" button.</param>
        /// <param name="closeButton">Label of the top right corner "close" button. Default value "Close".</param>
        /// <param name="dialogStyle">Style of the dialog.</param>
        public TwoButtonsDialog(string title, string message, string warning = null,
            TwoButtonsDialogType dialogType = TwoButtonsDialogType.OkCancel,
            string primaryButton = null, string secondaryButton = null,
            bool hasCloseButton = false, string closeButton = null,
            MegaDialogStyle dialogStyle = MegaDialogStyle.ContentDialog)
        {
            this.InitializeComponent();

            this.ViewModel.TitleText = title;
            this.ViewModel.MessageText = message;
            this.ViewModel.WarningText = warning ?? string.Empty;

            switch (dialogType)
            {
                case TwoButtonsDialogType.OkCancel:
                default:
                    this.ViewModel.PrimaryButtonLabel = this.ViewModel.OkText;
                    this.ViewModel.SecondaryButtonLabel = this.ViewModel.CancelText;
                    break;

                case TwoButtonsDialogType.YesNo:
                    this.ViewModel.PrimaryButtonLabel = this.ViewModel.YesText;
                    this.ViewModel.SecondaryButtonLabel = this.ViewModel.NoText;
                    break;

                case TwoButtonsDialogType.Custom:
                    this.ViewModel.PrimaryButtonLabel = primaryButton ?? this.ViewModel.OkText;
                    this.ViewModel.SecondaryButtonLabel = secondaryButton ?? this.ViewModel.CancelText;
                    break;
            }

            this.ViewModel.CloseButtonVisibility = hasCloseButton ? Visibility.Visible : Visibility.Collapsed;
            this.ViewModel.CloseButtonLabel = closeButton ?? this.ViewModel.CloseText;

            switch (dialogStyle)
            {
                case MegaDialogStyle.AlertDialog:
                    this.ViewModel.DialogStyle = (Style)Application.Current.Resources["MegaAlertDialogStyle"];
                    break;

                case MegaDialogStyle.ContentDialog:
                    this.ViewModel.DialogStyle = (Style)Application.Current.Resources["MegaContentDialogStyle"];
                    break;
            }
        }
    }
}
