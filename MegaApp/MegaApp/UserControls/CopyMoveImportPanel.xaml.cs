using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels.UserControls;

namespace MegaApp.UserControls
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseCopyMoveImportPanel : UserControlEx<CopyMoveImportPanelViewModel> { }

    public sealed partial class CopyMoveImportPanel : BaseCopyMoveImportPanel
    {
        public CopyMoveImportPanel()
        {
            this.InitializeComponent();
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PivotControl.SelectedItem.Equals(CloudDrivePivot))
                this.ViewModel.ActiveFolderView = this.ViewModel.CloudDrive;

            if (PivotControl.SelectedItem.Equals(IncomingSharesPivot))
                this.ViewModel.ActiveFolderView = this.ViewModel.IncomingShares;
        }
    }
}
