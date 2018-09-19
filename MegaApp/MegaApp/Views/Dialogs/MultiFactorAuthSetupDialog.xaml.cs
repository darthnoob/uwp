using Windows.UI.Xaml;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseMultiFactorAuthSetupDialog : ContentDialogEx<MultiFactorAuthSetupDialogViewModel> { }

    public sealed partial class MultiFactorAuthSetupDialog : BaseMultiFactorAuthSetupDialog
    {
        public MultiFactorAuthSetupDialog()
        {
            this.InitializeComponent();

            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
            {
                this.MaxWidth = 420;
                this.MainStackPanel.Margin = new Thickness(24, 0, 24, 0);
            }
        }
    }
}
