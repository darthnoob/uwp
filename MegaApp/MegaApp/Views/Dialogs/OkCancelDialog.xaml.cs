using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseOkCancelDialog : ContentDialogEx<OkCancelDialogViewModel> { }

    public sealed partial class OkCancelDialog : BaseOkCancelDialog
    {
        public OkCancelDialog(string title, string message)
        {
            this.InitializeComponent();

            this.ViewModel.Title = title;
            this.ViewModel.Message = message;
            this.ViewModel.PrimaryButtonLabel = ResourceService.UiResources.GetString("UI_Ok");
            this.ViewModel.SecondaryButtonLabel = ResourceService.UiResources.GetString("UI_Cancel");
        }

        public OkCancelDialog(string title, string message, string primaryButton, string secondaryButton)
        {
            this.InitializeComponent();

            this.ViewModel.Title = title;
            this.ViewModel.Message = message;
            this.ViewModel.PrimaryButtonLabel = primaryButton;
            this.ViewModel.SecondaryButtonLabel = secondaryButton;
        }
    }
}
