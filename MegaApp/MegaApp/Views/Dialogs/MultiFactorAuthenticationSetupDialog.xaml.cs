using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseMultiFactorAuthenticationSetupDialog : ContentDialogEx<MultiFactorAuthenticationSetupDialogViewModel> { }

    public sealed partial class MultiFactorAuthenticationSetupDialog : BaseMultiFactorAuthenticationSetupDialog
    {
        public MultiFactorAuthenticationSetupDialog()
        {
            this.InitializeComponent();
        }
    }
}
