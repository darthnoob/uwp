using Windows.System;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels.Login;

namespace MegaApp.Views.Login
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseConfirmRecoveryPage : SimplePage<ConfirmRecoveryViewModel> {}
   
    public sealed partial class ConfirmRecoveryPage : BaseConfirmRecoveryPage
    {
        public ConfirmRecoveryPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.RecoveryKey =
                NavigateService.GetNavigationObject(e.Parameter).Parameters[NavigationParamType.Data] as string;
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;

            this.ViewModel?.ValidatePasswordCommand.Execute(null);
        }
    }
}
