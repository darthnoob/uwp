using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace MegaApp.UserControls
{
    /// <summary>
    /// Extended HamburgerMenu control. Derived from the UWP Toolkit HamburgerMenu
    /// Exposes for example extra properties for binding purposes and the internal listviews
    /// </summary>
    public class HamburgerMenuEx: HamburgerMenu
    {
        /// <summary>
        /// Identifies the <see cref="SelectedMenuItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedMenuItemProperty = 
            DependencyProperty.Register(nameof(SelectedMenuItem), typeof(object), typeof(HamburgerMenuEx),
                new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="SelectedOptionItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedOptionItemProperty =
            DependencyProperty.Register(nameof(SelectedOptionItem), typeof(object), typeof(HamburgerMenuEx),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value that specifies the selected menu item.
        /// </summary>
        public object SelectedMenuItem
        {
            get { return GetValue(SelectedMenuItemProperty); }
            set { SetValue(SelectedMenuItemProperty, value); }
        }

        /// <summary>
        ///  Gets or sets a value that specifies the selected option menu item.
        /// </summary>
        public object SelectedOptionItem
        {
            get { return GetValue(SelectedOptionItemProperty); }
            set { SetValue(SelectedOptionItemProperty, value); }
        }
        
        /// <summary>
        /// Control that contains the menu items
        /// </summary>
        public ListViewBase ItemsListView { get; private set; }

        /// <summary>
        /// Control that contains the option items
        /// </summary>
        public ListViewBase OptionsListView { get; private set; }

        protected override void OnApplyTemplate()
        {
            // Get the template controls and expose them as public property for future use
            this.ItemsListView = (ListViewBase)GetTemplateChild("ButtonsListView");
            this.OptionsListView = (ListViewBase)GetTemplateChild("OptionsListView");
            
            // Bind the listview selected item to the dependency properties of the control
            // Now we can bind it to a viewmodel object
            var itemBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath("SelectedMenuItem"),
                Mode = BindingMode.TwoWay
            };
            this.ItemsListView.SetBinding(Selector.SelectedItemProperty, itemBinding);
            var optionBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath("SelectedOptionItem"),
                Mode = BindingMode.TwoWay
            };
            this.OptionsListView.SetBinding(Selector.SelectedItemProperty, optionBinding);

            base.OnApplyTemplate();
        }
    }
}
