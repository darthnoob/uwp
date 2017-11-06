using Windows.System;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels.CreateAccount;

namespace MegaApp.Views.CreateAccount
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseConfirmAccountPage : SimplePage<ConfirmAccountViewModel> {}
   
    public sealed partial class ConfirmAccountPage : BaseConfirmAccountPage
    {
        public ConfirmAccountPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var navObject = NavigateService.GetNavigationObject(e.Parameter);
            if (navObject == null) return;
            this.ViewModel.ConfirmLink = navObject.Parameters[NavigationParamType.Data] as string;
            this.ViewModel.Email = navObject.Parameters[NavigationParamType.Email] as string;
          
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;

            this.ViewModel?.ConfirmAccountCommand.Execute(null);
        }
    }
}
