using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Views.MyAccount
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseAchievementInvitationsPage : PageEx<AchievementInvitationsViewModel> { }

    public sealed partial class AchievementInvitationsPage : BaseAchievementInvitationsPage
    {
        public AchievementInvitationsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.AwardClass.Contacts.Initialize();
            // If the referral bonus award was clicked, go the second tab item
            if (this.ViewModel.AwardClass.IsGranted)
            {
                MainPivot.SelectedIndex = 1;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.AwardClass.Contacts.Deinitialize();
            base.OnNavigatedFrom(e);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // for nice looking size on desktop
            var element = sender as FrameworkElement;
            if (element == null) return;
            MainStackPanel.Width = element.ActualWidth >= MainStackPanel.MaxWidth 
                ? MainStackPanel.MaxWidth
                : element.Width;
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;

            this.ViewModel?.InviteCommand.Execute(null);
        }

        private void OnSortClick(object sender, RoutedEventArgs e)
        {
            var sortButton = sender as Button;
            if (sortButton == null) return;

            MenuFlyout menuFlyout = DialogService.CreateInviteContactsSortMenu(this.ViewModel.AwardClass.Contacts);

            if (menuFlyout == null) return;
            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(sortButton);
        }
    }
}
