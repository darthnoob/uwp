using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseMultiFactorAuthEnabledDialog : ContentDialogEx<MultiFactorAuthEnabledDialogViewModel> { }

    public sealed partial class MultiFactorAuthEnabledDialog : BaseMultiFactorAuthEnabledDialog
    {
        public MultiFactorAuthEnabledDialog()
        {
            this.InitializeComponent();
        }
    }
}
