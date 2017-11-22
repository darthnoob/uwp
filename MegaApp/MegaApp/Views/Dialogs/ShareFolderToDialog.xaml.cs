using Windows.UI.Xaml.Controls;
using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseShareFolderToDialog : ContentDialogEx<ShareFolderToDialogViewModel> { }

    public sealed partial class ShareFolderToDialog : BaseShareFolderToDialog
    {
        public ShareFolderToDialog(string folderName)
        {
            this.InitializeComponent();

            this.ViewModel.FolderName = folderName;
        }        

        #region Private Methods

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!this.ViewModel.CanClose)
                args.Cancel = true;
        }

        #endregion
    }
}
