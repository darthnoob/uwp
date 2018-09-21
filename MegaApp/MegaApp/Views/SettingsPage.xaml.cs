using System;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseSettingsPage : PageEx<SettingsViewModel> { }

    public sealed partial class SettingsPage : BaseSettingsPage
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SettingsService.ReloadSettingsRequested -= OnReloadSettingsRequested;

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Initialize();

            SettingsService.ReloadSettingsRequested += OnReloadSettingsRequested;

            var navObj = NavigateService.GetNavigationObject(e.Parameter) as NavigationObject;
            var navActionType = navObj?.Action ?? NavigationActionType.Default;
            if (navActionType == NavigationActionType.SecuritySettings)
                this.MainPivot.SelectedItem = this.SecurityPivot;
        }

        private void OnReloadSettingsRequested(object sender, EventArgs e)
        {
            this.ViewModel.ReloadSettings();
        }

        private void OnSdkVersionPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SdkService.ChangeApiUrlActionStarted();
        }

        private void OnSdkVersionPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            SdkService.ChangeApiUrlActionFinished();
        }

        private void OnSdkVersionTapped(object sender, TappedRoutedEventArgs e)
        {
            DebugService.ChangeStatusAction();
        }
    }
}
