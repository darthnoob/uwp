using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.UI.Animations;
using MegaApp.Enums;

namespace MegaApp.UserControls
{
    public class EmptyState: ContentControl
    {
        /// <summary>
        /// Gets or sets the header text.
        /// </summary>
        /// <value>The header text</value>
        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="HeaderText" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register(
                nameof(HeaderText), 
                typeof(string), 
                typeof(EmptyState),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the sub header text.
        /// </summary>
        /// <value>The sub header text</value>
        public string SubHeaderText
        {
            get { return (string)GetValue(SubHeaderTextProperty); }
            set { SetValue(SubHeaderTextProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="SubHeaderText" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SubHeaderTextProperty =
            DependencyProperty.Register(
                nameof(SubHeaderText),
                typeof(string), 
                typeof(EmptyState),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the header font size.
        /// </summary>
        /// <value>The header text font size</value>
        public double HeaderFontSize
        {
            get { return (double)GetValue(HeaderFontSizeProperty); }
            set { SetValue(HeaderFontSizeProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="HeaderFontSize" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderFontSizeProperty =
            DependencyProperty.Register(
                nameof(HeaderFontSize),
                typeof(double), 
                typeof(EmptyState),
                new PropertyMetadata(24));

        /// <summary>
        /// Gets or sets the sub header font size.
        /// </summary>
        /// <value>The sub header text font size</value>
        public double SubHeaderFontSize
        {
            get { return (double)GetValue(SubHeaderFontSizeProperty); }
            set { SetValue(SubHeaderFontSizeProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="SubHeaderFontSize" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SubHeaderFontSizeProperty =
            DependencyProperty.Register(
                nameof(SubHeaderFontSize),
                typeof(double), 
                typeof(EmptyState),
                new PropertyMetadata(16));

        /// <summary>
        /// Gets or sets the header foreground color.
        /// </summary>
        /// <value>The icon to show as home button</value>
        public Brush HeaderForeground
        {
            get { return (Brush)GetValue(HeaderForegroundProperty); }
            set { SetValue(HeaderForegroundProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="HeaderForeground" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderForegroundProperty =
            DependencyProperty.Register(
                nameof(HeaderForeground),
                typeof(Brush), 
                typeof(EmptyState),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the sub header foreground color.
        /// </summary>
        /// <value>The icon to show as home button</value>
        public Brush SubHeaderForeground
        {
            get { return (Brush)GetValue(SubHeaderForegroundProperty); }
            set { SetValue(SubHeaderForegroundProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="SubHeaderForeground" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SubHeaderForegroundProperty =
            DependencyProperty.Register(
                nameof(SubHeaderForeground),
                typeof(Brush), 
                typeof(EmptyState),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the sub header margin.
        /// </summary>
        /// <value>The icon to show as home button</value>
        public Thickness SubHeaderMargin
        {
            get { return (Thickness)GetValue(SubHeaderMarginProperty); }
            set { SetValue(SubHeaderMarginProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="SubHeaderMargin" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SubHeaderMarginProperty =
            DependencyProperty.Register(
                nameof(SubHeaderMargin),
                typeof(Thickness),
                typeof(EmptyState),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Visibility of the empty state.
        /// </summary>        
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="IsVisible"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register(
                nameof(IsVisible),
                typeof(bool),
                typeof(EmptyState),
                new PropertyMetadata(false, IsVisibleChangedCallback));

        private static void IsVisibleChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as EmptyState;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnIsVisibleChanged((bool)dpc.NewValue);
            }
        }

        private Grid _root;

        public EmptyState()
        {
            this.DefaultStyleKey = typeof(EmptyState);
            this.Opacity = 0.0;
            this.Visibility = Visibility.Collapsed;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _root = (Grid)this.GetTemplateChild("PART_RootGrid");
        }

        protected async void OnIsVisibleChanged(bool isVisible)
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                this.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            if (isVisible)
            {
                this.Visibility = Visibility.Visible;
                this.Fade(1.0f, 250.0f).Start();
                return;
            }
            await this.Fade(0.0f, 250.0f).StartAsync();
            this.Visibility = Visibility.Collapsed;
        }

        protected void OnAlignmentChanged(RelativeAlignment newAlignment)
        {
            if (_root == null) return;

            // Reset grid

            _root.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Auto);
            _root.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            _root.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Auto);
            _root.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
            _root.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Auto);
            switch (newAlignment)
            {
                case RelativeAlignment.Left:
                    _root.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    break;

                case RelativeAlignment.Above:
                    _root.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                    _root.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Auto);
                    break;

                case RelativeAlignment.Right:
                    _root.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                    break;

                case RelativeAlignment.Below:
                    _root.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Auto);
                    _root.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
