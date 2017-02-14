using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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

            var parameters = NavigateService.GetNavigationObject(e.Parameter).Parameters;
            _parentFolder = parameters[NavigationParamType.Data] as FolderViewModel;

            if (_parentFolder == null)
            {
                PageCommandBar.IsEnabled = false;

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
            var numItems = ViewModel.PreviewItems.Count;
            var itemIndex = ViewModel.PreviewItems.IndexOf(ViewModel.SelectedPreview);

            this.PreviousButton.IsEnabled = (itemIndex > 0) ? true : false;
            this.NextButton.IsEnabled = (itemIndex < numItems - 1) ? true : false;

            // Reset scale and position of the old selected image
            foreach (var previousImage in e.RemovedItems)
            {
                if (previousImage != null)
                {
                    var container = this.FlipView.ContainerFromItem(previousImage);
                    if(container != null)
                    {
                        var image = container.FindDescendant<Image>();
                        if (image != null)
                        {
                            (image.RenderTransform as CompositeTransform).ScaleX = 1;
                            (image.RenderTransform as CompositeTransform).ScaleY = 1;
                        }
                    }
                }
            }
        }

        private void OnImageDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var imagePreview = sender as Image;
            var imagePreviewTransform = imagePreview.RenderTransform as CompositeTransform;

            if (imagePreviewTransform.ScaleX > 1 || imagePreviewTransform.ScaleY > 1)
            {
                imagePreviewTransform.ScaleX = imagePreviewTransform.ScaleY = 1;
            }
            else
            {
                imagePreviewTransform.ScaleX = imagePreviewTransform.ScaleY = 2;
                imagePreviewTransform.CenterX = e.GetPosition(imagePreview).X;
                imagePreviewTransform.CenterY = e.GetPosition(imagePreview).Y;
            }
        }

        private void OnImagePointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            // If is a Desktop device and the Control key is pressed, zoom in/out the image
            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
            {
                var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
                if ((state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                {
                    var imagePreview = sender as Image;
                    var imagePreviewTransform = imagePreview.RenderTransform as CompositeTransform;

                    double deltaScroll = e.GetCurrentPoint(imagePreview).Properties.MouseWheelDelta;
                    deltaScroll = (deltaScroll > 0) ? 1.2 : 0.8;

                    double new_ScaleX = imagePreviewTransform.ScaleX * deltaScroll;
                    double new_ScaleY = imagePreviewTransform.ScaleY * deltaScroll;

                    imagePreviewTransform.CenterX = e.GetCurrentPoint(imagePreview).Position.X;
                    imagePreviewTransform.CenterY = e.GetCurrentPoint(imagePreview).Position.Y;

                    imagePreviewTransform.ScaleX = (new_ScaleX > 1) ? new_ScaleX : 1;
                    imagePreviewTransform.ScaleY = (new_ScaleY > 1) ? new_ScaleY : 1;

                    e.Handled = true;
                }
            }
        }

        private void OnImageManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            (sender as Image).Opacity = 0.4;
        }

        private void OnImageManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var imagePreview = sender as Image;
            var imagePreviewTransform = imagePreview.RenderTransform as CompositeTransform;

            imagePreviewTransform.TranslateX += e.Delta.Translation.X;
            imagePreviewTransform.TranslateY += e.Delta.Translation.Y;

            e.Handled = true;
        }

        private void OnImageManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            (sender as Image).Opacity = 1;
        }
    }
}
