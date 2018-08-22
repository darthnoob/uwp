using System.Linq;
using Windows.UI.Xaml.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels.MultiFactorAuth;

namespace MegaApp.Views.MultiFactorAuth
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseMultiFactorAuthAppSetupPage : PageEx<MultiFactorAuthAppSetupViewModel> { }

    public sealed partial class MultiFactorAuthAppSetupPage : BaseMultiFactorAuthAppSetupPage
    {
        public MultiFactorAuthAppSetupPage()
        {
            this.InitializeComponent();
        }

        public override bool CanGoBack => true;

        public override void GoBack()
        {
            NavigateService.Instance.Navigate(typeof(SettingsPage), false,
                NavigationObject.Create(typeof(MultiFactorAuthAppSetupViewModel),
                NavigationActionType.SecuritySettings));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Remove this page from the BackStack
            if (NavigateService.MainFrame.BackStack.Any())
                NavigateService.MainFrame.BackStack.Remove(NavigateService.MainFrame.BackStack.Last());

            base.OnNavigatedFrom(e);
        }
    }
}
