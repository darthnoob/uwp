using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
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
        }
    }
}
