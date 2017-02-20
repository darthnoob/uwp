using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BasePreviewImagePage : PageEx<PreviewImageViewModel> { }

    public sealed partial class PreviewImagePage : BasePreviewImagePage
    {
        private FolderViewModel _parentFolder;

        public PreviewImagePage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            AppService.SetAppViewBackButtonVisibility(true);
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            UiService.HideStatusBar();

            var parameters = NavigateService.GetNavigationObject(e.Parameter).Parameters;
            _parentFolder = parameters[NavigationParamType.Data] as FolderViewModel;

            if (_parentFolder == null)
            {
                // If something went wrong, disables the command bars
                this.TopCommandBar.IsEnabled = false;
                this.BottomCommandBar.IsEnabled = false;

                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_PreviewImageFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_PreviewImageInitFailed"));
                return;
            }

            this.ViewModel.Initialize(_parentFolder);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            UiService.ShowStatusBar();
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
            base.OnNavigatedFrom(e);
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            if (args.Handled) return;

            NavigateService.Instance.Navigate(typeof(MainPage), true);

            args.Handled = true;
        }

        private void OnFlipViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var itemsCount = this.FlipView.Items.Count;
            var itemIndex = this.FlipView.SelectedIndex;

            // Set the gallery direction and reset scale of the old selected image
            if (e.RemovedItems?.Count > 0 && e.RemovedItems[0] != null)
            {
                // Reset the scale of the old selected image
                var container = this.FlipView.ContainerFromItem(e.RemovedItems[0]);
                if (container != null)
                {
                    var scrollViewer = container.FindDescendant<ScrollViewer>();
                    if (scrollViewer != null)
                        scrollViewer.ChangeView(null, null, 1);
                }

                // Set the gallery direction taking into account the old and new image
                if (e.AddedItems?.Count > 0 && e.AddedItems[0] != null)
                {
                    var currentIndex = ViewModel.PreviewItems.IndexOf(e.AddedItems[0] as ImageNodeViewModel);
                    var lastIndex = ViewModel.PreviewItems.IndexOf(e.RemovedItems[0] as ImageNodeViewModel);

                    ViewModel.GalleryDirection = currentIndex > lastIndex ?
                        GalleryDirection.Next : GalleryDirection.Previous;
                }
            }
            else
            {
                // Set the default gallery direction
                ViewModel.GalleryDirection = GalleryDirection.Next;
            }
                        
            // Set the move buttons status
            this.PreviousButton.IsEnabled = (itemIndex > 0) ? true : false;
            this.NextButton.IsEnabled = (itemIndex < itemsCount - 1) ? true : false;

            // Check if the selected item is the first or the last item of the gallery.
            // If it is, invert the gallery direction to the only possible direction.
            if (itemIndex == 0)
                ViewModel.GalleryDirection = GalleryDirection.Next;
            else if (itemIndex == itemsCount - 1)
                ViewModel.GalleryDirection = GalleryDirection.Previous;
        }

        private void OnImageDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var image = sender as Image;
            if(image != null)
            {
                // Get the offset (exact point of double tap)
                var hOffset = e.GetPosition(image).X;
                var vOffset = e.GetPosition(image).Y;

                // Get the container of the image and set the new zoom
                var scrollViewer = image.FindAscendant<ScrollViewer>();
                if(scrollViewer != null)
                {
                    if (scrollViewer.ZoomFactor > 1)
                        scrollViewer.ChangeView(hOffset, vOffset, 1);
                    else
                        scrollViewer.ChangeView(hOffset, vOffset, 4);
                }
            }
        }

        private void OnImageManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                var hOffset = scrollViewer.HorizontalOffset - (scrollViewer.ZoomFactor * e.Delta.Translation.X);
                var vOffset = scrollViewer.VerticalOffset - (scrollViewer.ZoomFactor * e.Delta.Translation.Y);

                scrollViewer.ChangeView(hOffset, vOffset, scrollViewer.ZoomFactor);
            }
        }
    }
}
