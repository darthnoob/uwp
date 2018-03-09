using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using MegaApp.UserControls;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Views.MyAccount
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseAchievementInvitationsView : UserControlEx<AchievementInvitationsViewModel> { }

    public sealed partial class AchievementInvitationsView : BaseAchievementInvitationsView
    {
        public AchievementInvitationsView()
        {
            this.InitializeComponent();
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
            throw new System.NotImplementedException();
        }
    }
}
