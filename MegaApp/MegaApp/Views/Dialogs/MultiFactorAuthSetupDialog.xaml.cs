using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseMultiFactorAuthSetupDialog : ContentDialogEx<MultiFactorAuthSetupDialogViewModel> { }

    public sealed partial class MultiFactorAuthSetupDialog : BaseMultiFactorAuthSetupDialog
    {
        public MultiFactorAuthSetupDialog()
        {
            this.InitializeComponent();
        }
    }
}
