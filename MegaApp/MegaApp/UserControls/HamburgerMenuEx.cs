using System;
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
        /// Gets or sets the minimal window width at which the hamburger frame will use a narrow style.
        /// </summary>
        /// <value>The minimal window width at which the hamburger frame will use a narrow style.</value>
        public double VisualStateNarrowMinWidth
        {
            get { return (double)GetValue(VisualStateNarrowMinWidthProperty); }
            set { SetValue(VisualStateNarrowMinWidthProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="VisualStateNarrowMinWidth" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty VisualStateNarrowMinWidthProperty =
            DependencyProperty.Register(nameof(VisualStateNarrowMinWidth), typeof(double), typeof(HamburgerMenuEx), 
                new PropertyMetadata(0.0));

        /// <summary>
        /// Gets or sets the minimal window width at which the hamburger frame will use a normal style.
        /// </summary>
        /// <value>The minimal window width at which the hamburger frame will use a normal style.</value>
        public double VisualStateNormalMinWidth
        {
            get { return (double)GetValue(VisualStateNormalMinWidthProperty); }
            set { SetValue(VisualStateNormalMinWidthProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="VisualStateNormalMinWidth" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty VisualStateNormalMinWidthProperty =
            DependencyProperty.Register(nameof(VisualStateNormalMinWidth), typeof(double), typeof(HamburgerMenuEx), 
                new PropertyMetadata(0.0));

        /// <summary>
        /// Gets or sets the minimal window width at which the hamburger frame will use a wide style.
        /// </summary>
        /// <value>The minimal window width at which the hamburger frame will use a wide style.</value>
        public double VisualStateWideMinWidth
        {
            get { return (double)GetValue(VisualStateWideMinWidthProperty); }
            set { SetValue(VisualStateWideMinWidthProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="VisualStateWideMinWidth" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty VisualStateWideMinWidthProperty =
            DependencyProperty.Register(nameof(VisualStateWideMinWidth), typeof(double), typeof(HamburgerMenuEx), 
                new PropertyMetadata(0.0));

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

            if (this.ItemsListView != null)
            {
                // Bind the listview selected item to the dependency properties of the control
                // Now we can bind it to a viewmodel object
                this.ItemsListView.ItemContainerStyle = Application.Current.Resources["MenuItemContainerStyle"] as Style;
                var itemBinding = new Binding
                {
                    Source = this,
                    Path = new PropertyPath("SelectedMenuItem"),
                    Mode = BindingMode.TwoWay
                };
                this.ItemsListView.SetBinding(Selector.SelectedItemProperty, itemBinding);}

            if (this.OptionsListView != null)
            {
                // Bind the listview selected item to the dependency properties of the control
                // Now we can bind it to a viewmodel object
                this.OptionsListView.ItemContainerStyle = Application.Current.Resources["MenuItemContainerStyle"] as Style;
                var optionBinding = new Binding
                {
                    Source = this,
                    Path = new PropertyPath("SelectedOptionItem"),
                    Mode = BindingMode.TwoWay
                };
                this.OptionsListView.SetBinding(Selector.SelectedItemProperty, optionBinding);
            }

            if (this.Parent != null)
            {
                // Bind to parent resizing to change the display mode
                ((FrameworkElement) this.Parent).SizeChanged += OnParentSizeChanged;
            }

            base.OnApplyTemplate();
        }

        private void OnParentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width >= this.VisualStateWideMinWidth)
            {
                this.DisplayMode = SplitViewDisplayMode.CompactInline;
                if(e.PreviousSize.Width < this.VisualStateWideMinWidth)
                    this.IsPaneOpen = true;
                else
                    this.IsPaneOpen = this.IsPaneOpen & true;
                return;
            }
            if (e.NewSize.Width >= this.VisualStateNormalMinWidth)
            {
                this.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                this.IsPaneOpen = false;
                return;
            }
            this.DisplayMode = SplitViewDisplayMode.Overlay;
            this.IsPaneOpen = false;
        }
    }
}
