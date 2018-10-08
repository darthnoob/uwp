using System;
using System.Linq;
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml.Controls;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.ViewModels;
using MegaApp.ViewModels.UserControls;
using Windows.UI.Xaml;

namespace MegaApp.UserControls
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseShareToPanel : UserControlEx<ShareToPanelViewModel> { }

    public sealed partial class ShareToPanel : BaseShareToPanel
    {
        public ShareToPanel()
        {
            this.InitializeComponent();

            this.ViewModel.MegaContacts.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
            this.ViewModel.MegaContacts.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
            this.ViewModel.MegaContacts.ItemCollection.AllSelected += OnAllSelected;
        }

        #region Methods

        /// <summary>
        /// Gets or sets the Node.
        /// </summary>
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
                typeof(ShareToPanel),
                new PropertyMetadata(null, NodeChangedCallback));

        private static void NodeChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as ShareToPanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnNodeChanged((NodeViewModel)dpc.NewValue);
            }
        }

        private void OnNodeChanged(NodeViewModel node)
        {
            this.ViewModel.Node = node;
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            // Needed to avoid strange behaviors during the view update
            DisableViewsBehaviors();

            // First save the current selected items to restore them after enable the multi select
            var selectedItems = this.ViewModel.MegaContacts.ItemCollection.SelectedItems.ToList();

            this.ListViewContacts.SelectionMode = ListViewSelectionMode.Multiple;

            // Update the selected items
            foreach (var item in selectedItems)
                this.ListViewContacts.SelectedItems.Add(item);

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        private void OnMultiSelectDisabled(object sender, EventArgs e)
        {
            // Needed to avoid strange behaviors during the view update
            DisableViewsBehaviors();

            // If there is only one selected item save it to restore it after disable the multi select mode
            IMegaContact selectedItem = null;
            if (this.ViewModel.MegaContacts.ItemCollection.OnlyOneSelectedItem)
                selectedItem = this.ViewModel.MegaContacts.ItemCollection.SelectedItems.First();

            this.ListViewContacts.SelectionMode = 
                DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop ?
                ListViewSelectionMode.Extended : ListViewSelectionMode.Single;

            // Restore the selected item
            this.ListViewContacts.SelectedItem = this.ViewModel.MegaContacts.ItemCollection.FocusedItem = selectedItem;

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        /// <summary>
        /// Enable the behaviors of the active view
        /// </summary>
        private void EnableViewsBehaviors()
        {
            Interaction.GetBehaviors(this.ListViewContacts).Attach(this.ListViewContacts);
        }

        /// <summary>
        /// Disable the behaviors of the current active view
        /// </summary>
        private void DisableViewsBehaviors()
        {
            Interaction.GetBehaviors(this.ListViewContacts).Detach();
        }

        private void OnAllSelected(object sender, bool value)
        {
            if (!value)
            {
                this.ListViewContacts?.SelectedItems.Clear();
                return;
            }

            if (this.ListViewContacts?.SelectionMode == ListViewSelectionMode.Extended ||
                this.ListViewContacts?.SelectionMode == ListViewSelectionMode.Multiple)
            {
                this.ListViewContacts?.SelectAll();
            }
        }

        #endregion
    }
}
