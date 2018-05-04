using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Views.MyAccount
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseGeneralView : UserControlEx<GeneralViewModel> { }

    public sealed partial class GeneralView : BaseGeneralView
    {
        public GeneralView()
        {
            this.InitializeComponent();
        }

        private void ToolTipOnClosed(object sender, RoutedEventArgs e)
        {
            ((ToolTip)sender).Content = null;
        }

        private void ToolTipOnOpened(object sender, RoutedEventArgs e)
        {
            var tooltip = sender as ToolTip;
            if (tooltip == null) return;

            if (!this.ViewModel.AccountAchievements.IsAchievementsEnabled)
            {
                tooltip.IsOpen = false;
                return;
            }

            tooltip.Content = new ListView
            {
                Width = 280,
                ItemContainerStyle = Application.Current.Resources["StretchedListviewItemStyle"] as Style,
                ItemTemplate = (string) tooltip.Tag == "Storage"
                    ? Application.Current.Resources["StorageToolTipItemTemplate"] as DataTemplate
                    : Application.Current.Resources["TransferToolTipItemTemplate"] as DataTemplate,
                SelectionMode = ListViewSelectionMode.None,
                ItemContainerTransitions = null,
                ItemsSource = ViewModel.AccountAchievements.AwardedClasses
                    .Where(a => !a.IsExpired || a.IsBaseAward).ToList()
            };
        }

        private async void ShowToolTipOnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (!this.ViewModel.AccountAchievements.IsAchievementsEnabled) return;
           
            // Show the tooltip on mobile devices on tap for 3 seconds
            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop) return;

            var toolTip = (ToolTip) ToolTipService.GetToolTip(sender as TextBlock);
            toolTip.IsOpen = true;
            await Task.Delay(3000);
            toolTip.IsOpen = false;
        }
    }
}
