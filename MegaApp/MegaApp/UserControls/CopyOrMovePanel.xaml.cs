using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels.UserControls;

namespace MegaApp.UserControls
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseCopyOrMovePanel : UserControlEx<CopyOrMovePanelViewModel> { }

    public sealed partial class CopyOrMovePanel : BaseCopyOrMovePanel
    {
        public CopyOrMovePanel()
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
