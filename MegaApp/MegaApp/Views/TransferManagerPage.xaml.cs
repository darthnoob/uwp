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
            SetGUI();
        }

        private async void OnPauseResumeClick(object sender, RoutedEventArgs e)
        {
            // If selected the pivotitem for the downloads
            if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
                await ViewModel.PauseDownloads(!ViewModel.AreDownloadsPaused);
            else // If selected the pivotitem for the uploads
                await ViewModel.PauseUploads(!ViewModel.AreUploadsPaused);

            SetGUI();
        }

        private void SetGUI()
        {
            bool arePaused;

            // If selected the pivotitem for the downloads
            if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
                arePaused = ViewModel.AreDownloadsPaused;
            else // If selected the pivotitem for the uploads
                arePaused = ViewModel.AreUploadsPaused;

            this.PauseResumeButton.Icon = arePaused ? new SymbolIcon(Symbol.Play) : new SymbolIcon(Symbol.Pause);
            this.PauseResumeButton.Label = arePaused ?
                ResourceService.UiResources.GetString("UI_Resume") : ResourceService.UiResources.GetString("UI_Pause");
        }
    }
}
