using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;
using MegaApp.Enums;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseTransferManagerPage : PageEx<TransferManagerViewModel> { }

    public sealed partial class TransferManagerPage : BaseTransferManagerPage
    {
        public TransferManagerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Update();
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(TransfersPivot.SelectedIndex)
            {
                case 0:
                    this.ViewModel.ActiveViewModel = this.ViewModel.Uploads;
                    break;
                case 1:
                    this.ViewModel.ActiveViewModel = this.ViewModel.Downloads;
                    break;
            }
        }
    }
}
