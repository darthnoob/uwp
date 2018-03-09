using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MegaApp.UserControls;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Views.MyAccount
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseAchievementsView : UserControlEx<AchievementsViewModel> { }

    public sealed partial class AchievementsView : BaseAchievementsView
    {
        public AchievementsView()
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

        private void GridViewOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || !e.AddedItems.Any()) return;
            var award = e.AddedItems[0] as AwardViewModel;
            award?.ActionCommand.Execute(null);
            ((GridView) sender).SelectedItem = null;
        }
    }
}
