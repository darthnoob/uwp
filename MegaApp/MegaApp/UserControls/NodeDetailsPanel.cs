using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MegaApp.Services;
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
                new PropertyMetadata(null, NodeChangedCallback));

        private static void NodeChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as NodeDetailsPanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnNodeChanged((NodeViewModel)dpc.NewValue);
            }
        }

        private Image _previewImage;
        //private ToggleSwitch _enableOfflineView;
        private ToggleSwitch _enableLink;
        private RadioButton _linkWithoutKey;
        private RadioButton _decryptionKey;
        private RadioButton _linkWithKey;
        private TextBlock _exportLinkBorderTitle;
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

            this._previewImage = (Image)this.GetTemplateChild("PART_PreviewImage");
            //this._enableOfflineView = (ToggleSwitch)this.GetTemplateChild("PART_EnableOfflineViewSwitch");
            this._enableLink = (ToggleSwitch)this.GetTemplateChild("PART_EnableLinkSwitch");
            this._linkWithoutKey = (RadioButton)this.GetTemplateChild("PART_LinkWithoutKeyRadioButton");
            this._decryptionKey = (RadioButton)this.GetTemplateChild("PART_DecryptionKeyRadioButton");
            this._linkWithKey = (RadioButton)this.GetTemplateChild("PART_LinkWithKeyRadioButton");
            this._exportLinkBorderTitle = (TextBlock)this.GetTemplateChild("PART_ExportLinkBorderTitle");
            this._copyLink = (Button)this.GetTemplateChild("PART_CopyLinkButton");
            this._shareLink = (Button)this.GetTemplateChild("PART_ShareLinkButton");
            
            this._previewImage.Tapped += (sender, args) => OnPreviewImageTapped();
            //this._enableOfflineView.Toggled += (sender, args) => OnEnableOfflineViewSwitchToggled();
            this._enableLink.Toggled += (sender, args) => OnEnableLinkSwitchToggled();
            this._linkWithoutKey.Content = this.LinkWithoutKeyLabelText;
            this._linkWithoutKey.Checked += (sender, args) => OnLinkWithoutKeyRadioButtonChecked();
            this._decryptionKey.Content = this.DecryptionKeyLabelText;
            this._decryptionKey.Checked += (sender, args) => OnDecryptionKeyRadioButtonChecked();
            this._linkWithKey.Content = this.LinkWithKeyLabelText;
            this._linkWithKey.Checked += (sender, args) => OnLinkWithKeyRadioButtonChecked();
            this._copyLink.Tapped += (sender, args) => OnCopyLinkButtonTapped();
            this._shareLink.Tapped += (sender, args) => OnShareLinkButtonTapped();
        }

        protected void OnNodeChanged(NodeViewModel node)
        {
            // Default value is laways link with key
            this._linkWithKey.IsChecked = true;
            OnLinkWithKeyRadioButtonChecked();
        }

        private void OnPreviewImageTapped()
        {
            this.Node.Parent.ProcessFileNode(this.Node);
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

        private void OnLinkWithoutKeyRadioButtonChecked()
        {
            this._exportLinkBorderTitle.Text = this.ExportLinkText;
            this.Node.ExportLink = this.Node.OriginalMNode.getPublicLink(false);
        }

        private void OnDecryptionKeyRadioButtonChecked()
        {
            this._exportLinkBorderTitle.Text = this.DecryptionKeyLabelText;
            this.Node.ExportLink = this.Node.OriginalMNode.getBase64Key();
        }

        private void OnLinkWithKeyRadioButtonChecked()
        {
            this._exportLinkBorderTitle.Text = this.ExportLinkText;
            this.Node.ExportLink = this.Node.OriginalMNode.getPublicLink(true);
        }

        private void OnCopyLinkButtonTapped()
        {
            Services.ShareService.CopyLinkToClipboard(this.Node.ExportLink);
        }

        private void OnShareLinkButtonTapped()
        {
            Services.ShareService.ShareLink(this.Node.ExportLink);
        }

        #region UiResources

        private string DecryptionKeyLabelText => ResourceService.UiResources.GetString("UI_DecryptionKey");
        private string ExportLinkText => ResourceService.UiResources.GetString("UI_ExportLink");
        private string LinkWithKeyLabelText => ResourceService.UiResources.GetString("UI_LinkWithKey");
        private string LinkWithoutKeyLabelText => ResourceService.UiResources.GetString("UI_LinkWithoutKey");

        #endregion
    }
}
