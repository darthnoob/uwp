using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseMultiFactorAuthDisabledDialog : ContentDialogEx<MultiFactorAuthDisabledDialogViewModel> { }

    public sealed partial class MultiFactorAuthDisabledDialog : BaseMultiFactorAuthDisabledDialog
    {
        public MultiFactorAuthDisabledDialog()
        {
            this.InitializeComponent();
        }
    }
}
