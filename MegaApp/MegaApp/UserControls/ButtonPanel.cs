using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.UI.Animations;
using MegaApp.Enums;
using System.Windows.Input;

namespace MegaApp.UserControls
{
    public class ButtonPanel : ContentControl
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
                typeof(ButtonPanel),
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
                typeof(ButtonPanel),
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
                typeof(ButtonPanel),
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
                typeof(ButtonPanel),
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
                typeof(ButtonPanel),
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
                typeof(ButtonPanel),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the sub header foreground color.
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
                typeof(ButtonPanel),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the min width of the button.
        /// </summary>
        /// <value>Size of the button control</value>
        public double ButtonMinWidth
        {
            get { return (double)GetValue(ButtonMinWidthProperty); }
            set { SetValue(ButtonMinWidthProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ButtonMinWidth" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ButtonMinWidthProperty =
            DependencyProperty.Register(
                nameof(ButtonMinWidth),
                typeof(double),
                typeof(ButtonPanel),
                new PropertyMetadata(48));

        /// <summary>
        /// Gets or sets the alignment of the button control relative to the texts.
        /// </summary>
        /// <value>Alignment of the button control</value>
        public RelativeAlignment ButtonAlignment
        {
            get { return (RelativeAlignment)GetValue(ButtonAlignmentProperty); }
            set { SetValue(ButtonAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ButtonAlignment" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ButtonAlignmentProperty =
            DependencyProperty.Register(
                nameof(ButtonAlignment),
                typeof(RelativeAlignment),
                typeof(ButtonPanel),
                new PropertyMetadata(RelativeAlignment.Below, ButtonAlignmentChangedCallback));

        private static void ButtonAlignmentChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as ButtonPanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnAlignmentChanged((RelativeAlignment)dpc.NewValue);
            }
        }

        /// <summary>
        /// Gets or sets the button text.
        /// </summary>
        /// <value>The button text</value>
        public ICommand ButtonText
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ButtonCommand" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register(
                nameof(ButtonCommand),
                typeof(ICommand),
                typeof(ButtonPanel),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the button command.
        /// </summary>
        /// <value>The button text</value>
        public string ButtonCommand
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ButtonText" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(
                nameof(ButtonText),
                typeof(string),
                typeof(ButtonPanel),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Visibility of the button panel.
        /// </summary>
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="IsVisible" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register(
                nameof(IsVisible),
                typeof(bool),
                typeof(ButtonPanel),
                new PropertyMetadata(false, IsVisibleChangedCallback));

        private static void IsVisibleChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as ButtonPanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnIsVisibleChanged((bool)dpc.NewValue);
            }
        }

        private Button _button;
        private Grid _root;

        public ButtonPanel()
        {
            this.DefaultStyleKey = typeof(ButtonPanel);
            this.Opacity = 0.0;
            this.Visibility = Visibility.Collapsed;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _root = (Grid)this.GetTemplateChild("PART_RootGrid");
            _button = (Button)this.GetTemplateChild("PART_Button");
            if (_button != null)
            {
                _button.Tapped -= OnButtonTapped;
                _button.Tapped += OnButtonTapped;
            }
            OnAlignmentChanged(this.ButtonAlignment);
            OnIsVisibleChanged(this.IsVisible);
        }

        /// <summary>
        /// Event triggered when button is tapped.
        /// </summary>
        public EventHandler ButtonTapped;

        /// <summary>
        /// Event invocator method called when the button is tapped.
        /// </summary>
        protected virtual void OnButtonTapped(object sender, RoutedEventArgs e)
        {
            this.ButtonTapped?.Invoke(this, EventArgs.Empty);
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
                this.Fade(1.0f, 250.0).Start();
                return;
            }
            await this.Fade(0.0f, 250.0).StartAsync();
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
