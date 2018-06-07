using MegaApp.UserControls;
using MegaApp.ViewModels.MultiFactorAuth;

namespace MegaApp.Views.MultiFactorAuth
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseMultiFactorAuthAppSetupPage : PageEx<MultiFactorAuthAppSetupViewModel> { }

    public sealed partial class MultiFactorAuthAppSetupPage : BaseMultiFactorAuthAppSetupPage
    {
        public MultiFactorAuthAppSetupPage()
        {
            this.InitializeComponent();
        }
    }
}
