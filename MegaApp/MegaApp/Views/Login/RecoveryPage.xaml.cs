using Windows.System;
using Windows.UI.Xaml.Input;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels.Login;

namespace MegaApp.Views.Login
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseRecoveryPage : SimplePage<RecoveryViewModel> {}
   
    public sealed partial class RecoveryPage : BaseRecoveryPage
    {
        public RecoveryPage()
        {
            InitializeComponent();
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;
            
            this.ViewModel?.SendCommand.Execute(null);
        }
    }
}
