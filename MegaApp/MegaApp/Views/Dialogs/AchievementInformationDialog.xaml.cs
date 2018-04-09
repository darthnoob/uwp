using System;
using Windows.UI.Xaml.Controls;
using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseAchievementInformationDialog : ContentDialogEx<AchievementInformationDialogViewModel> { }

    public sealed partial class AchievementInformationDialog : BaseAchievementInformationDialog
    {
        public AchievementInformationDialog(AwardClassViewModel award)
        {
            this.InitializeComponent();
            this.ViewModel.Award = award;
        }


        private void DialogOnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.ViewModel.Closed += OnClosed;
            this.PrimaryButtonText = !this.ViewModel.Award.IsGranted 
                ? this.ViewModel.InstallText 
                : string.Empty;
        }

        private void DialogOnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            this.ViewModel.Closed -= OnClosed;
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            this.Hide();
        }
       
    }
}
