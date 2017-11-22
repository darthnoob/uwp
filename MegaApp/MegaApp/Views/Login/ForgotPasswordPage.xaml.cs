using MegaApp.UserControls;
using MegaApp.ViewModels.Login;

namespace MegaApp.Views.Login
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseForgotPasswordPage : SimplePage<ForgotPasswordViewModel> {}
   
    public sealed partial class ForgotPasswordPage : BaseForgotPasswordPage
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }
    }
}
