using Windows.System;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels.CreateAccount;
using MegaApp.ViewModels.Login;

namespace MegaApp.Views.Login
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseConfirmEmailPage : SimplePage<ConfirmEmailViewModel> {}
   
    public sealed partial class ConfirmEmailPage : BaseConfirmEmailPage
    {
        public ConfirmEmailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var navObject = NavigateService.GetNavigationObject(e.Parameter);
            if (navObject == null) return;
            this.ViewModel.Email = navObject.Parameters[NavigationParamType.Email] as string;
            this.ViewModel.Password = navObject.Parameters[NavigationParamType.Password] as string;
            this.ViewModel.FirstName = navObject.Parameters[NavigationParamType.FirstName] as string;
            this.ViewModel.LastName = navObject.Parameters[NavigationParamType.LastName] as string;
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;
            
            this.ViewModel?.ResendCommand.Execute(null);
        }
    }
}
