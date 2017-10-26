using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseTransferOverquotaWarningDialog : ContentDialogEx<TransferOverquotaWarningDialogViewModel> { }

    public sealed partial class TransferOverquotaWarningDialog : BaseTransferOverquotaWarningDialog
    {
        public TransferOverquotaWarningDialog()
        {
            this.InitializeComponent();
        }
    }
}
