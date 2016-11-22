using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);            

            // Do not allow user to go back to any previous page
            NavigateService.CoreFrame.BackStack.Clear();
        }

        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            if (PivotLoginAndCreateAccount?.SelectedItem == PivotItemLogin)
            {
                this.ViewModel?.LoginViewModel?.LoginAsync();
                return;
            }
            // Else it is always create account
           this.ViewModel?.CreateAccountViewModel?.CreateAccount();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PivotLoginAndCreateAccount?.SelectedItem == PivotItemLogin)
            {
                this.ViewModel.ActiveViewModel = this.ViewModel.LoginViewModel;
                return;
            }
            // Else it is always create account
            this.ViewModel.ActiveViewModel = this.ViewModel.CreateAccountViewModel;
        }
    }
}
