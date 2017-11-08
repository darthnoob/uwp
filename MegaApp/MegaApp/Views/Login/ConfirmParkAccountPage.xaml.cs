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
    public class BaseConfirmParkAccountPage : SimplePage<ConfirmParkAccountViewModel> {}
   
    public sealed partial class ConfirmParkAccountPage : BaseConfirmParkAccountPage
    {
        public ConfirmParkAccountPage()
        {
            InitializeComponent();
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;

            this.ViewModel?.StartNewAccountCommand.Execute(null);
        }
    }
}
