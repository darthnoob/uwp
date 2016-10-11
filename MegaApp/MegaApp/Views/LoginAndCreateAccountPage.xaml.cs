using Windows.UI.Xaml;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseLoginAndCreateAccountPage : PageEx<LoginAndCreateAccountViewModel> {}
   
    public sealed partial class LoginAndCreateAccountPage : BaseLoginAndCreateAccountPage
    {
        public LoginAndCreateAccountPage()
        {
            InitializeComponent();            
        }

        private async void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            if (! await NetworkService.IsNetworkAvailable(true)) return;

            // To not allow cancel a request to login or 
            // create account once that is started
            //SetApplicationBar(false);

            //if (_loginAndCreateAccountViewModelContainer == null)
            //    _loginAndCreateAccountViewModelContainer = new LoginAndCreateAccountViewModelContainer(this);

            if (Pivot_LoginAndCreateAccount.SelectedItem == PivotItem_Login)
                this.ViewModel.LoginViewModel.DoLogin();
            else if (Pivot_LoginAndCreateAccount.SelectedItem == PivotItem_CreateAccount)
                this.ViewModel.CreateAccountViewModel.CreateAccount();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
