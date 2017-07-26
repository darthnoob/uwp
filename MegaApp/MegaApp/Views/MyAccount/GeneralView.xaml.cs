using Windows.UI.Xaml.Controls;
using MegaApp.UserControls;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Views.MyAccount
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseGeneralView : UserControlEx<GeneralViewModel> { }

    public sealed partial class GeneralView : BaseGeneralView
    {
        public GeneralView()
        {
            this.InitializeComponent();
        }
    }
}
