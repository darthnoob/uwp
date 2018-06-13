using Windows.UI.Xaml;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Initialize();

            var navObj = NavigateService.GetNavigationObject(e.Parameter) as NavigationObject;
            var navActionType = navObj?.Action ?? NavigationActionType.Default;
            if (navActionType == NavigationActionType.SecuritySettings)
                this.MainPivot.SelectedItem = this.SecurityPivot;
        }

        private void OnSdkVersionTapped(object sender, TappedRoutedEventArgs e)
        {
            DebugService.ChangeStatusAction();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // For nice looking size on desktop
            var element = sender as FrameworkElement;
            if (element == null) return;

            this.ViewModel.LegalAndPoliciesSetting.UpdateWidthOfGUI(element.ActualWidth);
        }
    }
}
