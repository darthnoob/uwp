using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;
using MegaApp.ViewModels.UserControls;

namespace MegaApp.UserControls
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseSharedFolderContentPanel : UserControlEx<SharedFolderContentPanelViewModel> { }

    public sealed partial class SharedFolderContentPanel : BaseSharedFolderContentPanel
    {
        public SharedFolderContentPanel()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the Shared Folder node.
        /// </summary>
        public SharedFolderNodeViewModel SharedFolderNode
        {
            get { return (SharedFolderNodeViewModel)GetValue(SharedFolderProperty); }
            set { SetValue(SharedFolderProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="SharedFolder" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SharedFolderProperty =
            DependencyProperty.Register(
                nameof(SharedFolderNode),
                typeof(SharedFolderNodeViewModel),
                typeof(SharedFolderContentPanel),
                new PropertyMetadata(null, SharedFolderChangedCallback));

        private static void SharedFolderChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as SharedFolderContentPanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnSharedFolderChanged((SharedFolderNodeViewModel)dpc.NewValue);
            }
        }

        private void OnSharedFolderChanged(SharedFolderNodeViewModel sharedFolderNode)
        {
            this.ViewModel.SharedFolderNode = sharedFolderNode;
        }

        private void OnSortClick(object sender, RoutedEventArgs e)
        {
            var sortButton = sender as Button;
            if (sortButton == null) return;

            MenuFlyout menuFlyout = DialogService.CreateSortMenu(this.ViewModel.SharedFolder);
            if (menuFlyout == null) return;

            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(sortButton);
        }
    }
}
