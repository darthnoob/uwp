using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;
using MegaApp.Enums;

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

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            // If selected the pivotitem for the downloads
            if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
                ViewModel.CancelDownloads();
            else // If selected the pivotitem for the uploads
                ViewModel.CancelUploads();
        }

        private void OnCleanUpTransfersClick(object sender, RoutedEventArgs e)
        {
            TransferService.UpdateMegaTransfersList(ViewModel.MegaTransfers);
        }

        private void SetGUI()
        {
            bool arePaused;

            // If selected the pivotitem for the downloads
            if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
            {
                arePaused = ViewModel.AreDownloadsPaused;
                this.CancelButton.Label = ViewModel.CancelDownloadsText;
            }                
            else // If selected the pivotitem for the uploads
            {
                arePaused = ViewModel.AreUploadsPaused;
                this.CancelButton.Label = ViewModel.CancelUploadsText;
            }

            this.PauseResumeButton.Icon = arePaused ? new SymbolIcon(Symbol.Play) : new SymbolIcon(Symbol.Pause);
            this.PauseResumeButton.Label = arePaused ? ViewModel.ResumeText : ViewModel.PauseText;
        }

        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ListView listView = (ListView)sender;
            e.Handled = OpenFlyoutMenu(listView, (FrameworkElement)e.OriginalSource, e.GetPosition(listView));
        }

        private void OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            ListView listView = (ListView)sender;
            e.Handled = OpenFlyoutMenu(listView, (FrameworkElement)e.OriginalSource, e.GetPosition(listView));
        }

        private bool OpenFlyoutMenu(ListView listView, FrameworkElement listViewItem, Point position)
        {
            SdkService.MegaSdk.retryPendingConnections();

            try
            {
                if (ViewModel != null)
                {
                    // We don't want to open the menu if the focused element is not a list view item.
                    // If the list view is empty listViewItem will be null.
                    if (!(listViewItem?.DataContext is TransferObjectModel))
                        return true;

                    // We don't want to open the menu if the focused element has already canceled,
                    // downloaded, uploaded, etc.
                    switch((listViewItem?.DataContext as TransferObjectModel).Status)
                    {
                        case TransferStatus.Error:
                        case TransferStatus.Canceled:
                        case TransferStatus.Canceling:
                        case TransferStatus.Downloaded:
                        case TransferStatus.Uploaded:
                            return true;
                    }

                    MenuFlyout menuFlyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(listView);
                    menuFlyout.ShowAt(listView, position);

                    ViewModel.FocusedTransfer = listViewItem.DataContext as TransferObjectModel;
                }
                else
                {
                    return true;
                }

                return false;
            }
            catch (Exception) { return true; }
        }
    }
}
