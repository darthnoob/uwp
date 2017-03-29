using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels;

namespace MegaApp.UserControls
{
    public class NodeDetailsPanel : ContentControl
    {
        /// <summary>
        /// Gets or sets the Node.
        /// </summary>
        /// <value>The header text</value>
        public NodeViewModel Node
        {
            get { return (NodeViewModel)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="Node" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty NodeProperty =
            DependencyProperty.Register(
                nameof(Node),
                typeof(NodeViewModel),
                typeof(NodeDetailsPanel),
                new PropertyMetadata(null));

        //private ToggleSwitch _enableOfflineView;
        private ToggleSwitch _enableLink;
        private Button _copyLink;
        private Button _shareLink;

        public NodeDetailsPanel()
        {
            this.DefaultStyleKey = typeof(NodeDetailsPanel);
            this.Opacity = 1;
            this.Visibility = Visibility.Visible;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //this._enableOfflineView = (ToggleSwitch)this.GetTemplateChild("PART_EnableOfflineViewSwitch");
            this._enableLink = (ToggleSwitch)this.GetTemplateChild("PART_EnableLinkSwitch");
            this._copyLink = (Button)this.GetTemplateChild("PART_CopyLinkButton");
            this._shareLink = (Button)this.GetTemplateChild("PART_ShareLinkButton");

            //this._enableOfflineView.Toggled += (sender, args) => OnEnableOfflineViewSwitchToggled();
            this._enableLink.Toggled += (sender, args) => OnEnableLinkSwitchToggled();
            this._copyLink.Tapped += (sender, args) => OnCopyLinkButtonTapped();
            this._shareLink.Tapped += (sender, args) => OnShareLinkButtonTapped();
        }

        //private void OnEnableOfflineViewSwitchToggled()
        //{
        //    if(this._enableOfflineView.IsOn)
        //    {
                
        //    }
        //    else
        //    {
                
        //    }
        //}

        private void OnEnableLinkSwitchToggled()
        {
            if (this._enableLink.IsOn && !this.Node.OriginalMNode.isExported())
                this.Node.GetLinkAsync(false);
            else if (!this._enableLink.IsOn && this.Node.OriginalMNode.isExported())
                this.Node.RemoveLink();
        }

        private void OnCopyLinkButtonTapped()
        {
            Services.ShareService.CopyLinkToClipboard(this.Node.ExportLink);
        }

        private void OnShareLinkButtonTapped()
        {
            Services.ShareService.ShareLink(this.Node.ExportLink);
        }
    }
}
