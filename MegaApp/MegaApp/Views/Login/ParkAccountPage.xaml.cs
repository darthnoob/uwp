using Windows.System;
using Windows.UI.Xaml.Input;
using MegaApp.UserControls;
using MegaApp.ViewModels.Login;

namespace MegaApp.Views.Login
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseParkAccountPage : SimplePage<ParkAccountViewModel> {}
   
    public sealed partial class ParkAccountPage : BaseParkAccountPage
    {
        public ParkAccountPage()
        {
            InitializeComponent();
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;

            this.ViewModel?.ParkAccountCommand.Execute(null);
        }
    }
}
