using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseChangeToStagingServerDialog : ContentDialogEx<ChangeToStagingServerDialogViewModel> { }

    public sealed partial class ChangeToStagingServerDialog : BaseChangeToStagingServerDialog
    {
        public ChangeToStagingServerDialog()
        {
            this.InitializeComponent();
        }
    }
}
