using Windows.UI.Xaml;
using MegaApp.ViewModels.SharedFolders;
using MegaApp.ViewModels.UserControls;

namespace MegaApp.UserControls
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseSharedFolderInformationPanel : UserControlEx<SharedFolderInformationPanelViewModel> { }

    public sealed partial class SharedFolderInformationPanel : BaseSharedFolderInformationPanel
    {
        public SharedFolderInformationPanel()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the Shared Folder.
        /// </summary>
        public SharedFolderNodeViewModel SharedFolder
        {
            get { return (SharedFolderNodeViewModel)GetValue(SharedFolderProperty); }
            set { SetValue(SharedFolderProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="SharedFolder" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SharedFolderProperty =
            DependencyProperty.Register(
                nameof(SharedFolder),
                typeof(SharedFolderNodeViewModel),
                typeof(SharedFolderInformationPanel),
                new PropertyMetadata(null, SharedFolderChangedCallback));

        private static void SharedFolderChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as SharedFolderInformationPanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnSharedFolderChanged((SharedFolderNodeViewModel)dpc.NewValue);
            }
        }

        private void OnSharedFolderChanged(SharedFolderNodeViewModel sharedFolder)
        {
            this.ViewModel.SharedFolder = sharedFolder;

            if (this.ViewModel.IsInShare)
            {
                this.PivotControl.Items.Remove(this.LinkPivot);
                return;
            }

            if (!this.PivotControl.Items.Contains(this.LinkPivot))
                this.PivotControl.Items.Add(this.LinkPivot);
        }
    }
}
