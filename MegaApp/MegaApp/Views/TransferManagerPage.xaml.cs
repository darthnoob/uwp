using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MegaApp.UserControls;
using MegaApp.ViewModels;
using Windows.UI.Xaml.Navigation;
using MegaApp.Services;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseTransferManagerPage : PageEx<TransferManagerViewModel> { }

    public sealed partial class TransferManagerPage : BaseTransferManagerPage
    {
        public TransferManagerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TransferService.UpdateMegaTransfersList(ViewModel.MegaTransfers);
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnPauseAllClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
