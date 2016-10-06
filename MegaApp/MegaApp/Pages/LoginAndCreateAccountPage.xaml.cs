using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MegaApp.Containers;
using MegaApp.Services;

namespace MegaApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginAndCreateAccountPage : Page
    {
        private LoginAndCreateAccountViewModelContainer _loginAndCreateAccountViewModelContainer;

        public LoginAndCreateAccountPage()
        {
            _loginAndCreateAccountViewModelContainer = new LoginAndCreateAccountViewModelContainer(this);

            this.DataContext = _loginAndCreateAccountViewModelContainer;

            InitializeComponent();

            //SetApplicationBar(true);
        }

        public void SetStatusTxtEmailCreateAccount(bool isReadOnly)
        {
            this.TxtEmail_CreateAccount.IsReadOnly = isReadOnly;
        }

        private async void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            if (! await NetworkService.IsNetworkAvailable(true)) return;

            // To not allow cancel a request to login or 
            // create account once that is started
            //SetApplicationBar(false);

            if (_loginAndCreateAccountViewModelContainer == null)
                _loginAndCreateAccountViewModelContainer = new LoginAndCreateAccountViewModelContainer(this);

            if (Pivot_LoginAndCreateAccount.SelectedItem == PivotItem_Login)
                _loginAndCreateAccountViewModelContainer.LoginViewModel.DoLogin();
            else if (Pivot_LoginAndCreateAccount.SelectedItem == PivotItem_CreateAccount)
                _loginAndCreateAccountViewModelContainer.CreateAccountViewModel.CreateAccount();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
