using System.Collections.Generic;
using Windows.UI.Xaml;
using MegaApp.Interfaces;
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

            this.ViewModel.CloudDrive.LoadChildNodes();
        }

        /// <summary>
        /// Gets or sets the selected nodes to copy or move
        /// </summary>
        public List<IMegaNode> SelectedNodes
        {
            get { return (List<IMegaNode>)GetValue(SelectedNodesProperty); }
            set { SetValue(SelectedNodesProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="SelectedNodes" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedNodesProperty =
            DependencyProperty.Register(
                nameof(SelectedNodes),
                typeof(List<IMegaNode>),
                typeof(CopyOrMovePanel),
                new PropertyMetadata(null, SelectedNodesChangedCallback));

        private static void SelectedNodesChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as CopyOrMovePanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnSelectedNodesChanged((List<IMegaNode>)dpc.NewValue);
            }
        }

        private void OnSelectedNodesChanged(List<IMegaNode> selectedNodes)
        {
            this.ViewModel.SelectedNodes = selectedNodes;
        }
    }
}
