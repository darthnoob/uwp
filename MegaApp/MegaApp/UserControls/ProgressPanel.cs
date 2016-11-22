using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using MegaApp.Enums;
using Microsoft.Toolkit.Uwp.UI.Animations;

namespace MegaApp.UserControls
{
    public class ProgressPanel: ContentControl
    {
        /// <summary>
        /// Gets or sets the header text.
        /// </summary>
        /// <value>The icon to show as home button</value>
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
                typeof(ProgressPanel),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the sub header text.
        /// </summary>
        /// <value>The icon to show as home button</value>
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
                typeof(ProgressPanel),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the header font size.
        /// </summary>
        /// <value>The icon to show as home button</value>
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
                typeof(ProgressPanel),
                new PropertyMetadata(24));

        /// <summary>
        /// Gets or sets the sub header font size.
        /// </summary>
        /// <value>The icon to show as home button</value>
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
                typeof(ProgressPanel),
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
                typeof(ProgressPanel),
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
                typeof(ProgressPanel),
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
                typeof(ProgressPanel),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the size of the progress ring.
        /// </summary>
        /// <value>The icon to show as home button</value>
        public double ProgressSize
        {
            get { return (double)GetValue(ProgressSizeProperty); }
            set { SetValue(ProgressSizeProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ProgressSize" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProgressSizeProperty =
            DependencyProperty.Register(
                nameof(ProgressSize),
                typeof(double),
                typeof(ProgressPanel),
                new PropertyMetadata(48));

        /// <summary>
        /// Gets or sets the size of the progress ring.
        /// </summary>
        /// <value>The icon to show as home button</value>
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
                typeof(ProgressPanel),
                new PropertyMetadata(false, IsVisibleChangedCallback));

        private static void IsVisibleChangedCallback(DependencyObject d, 
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as ProgressPanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnIsVisibleChanged((bool)dpc.NewValue);
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the progress ring relative to the texts.
        /// </summary>
        /// <value>The icon to show as home button</value>
        public RelativeAlignment ProgressAlignment
        {
            get { return (RelativeAlignment)GetValue(ProgressAlignmentProperty); }
            set { SetValue(ProgressAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ProgressAlignment" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProgressAlignmentProperty =
            DependencyProperty.Register(
                nameof(ProgressAlignment),
                typeof(RelativeAlignment),
                typeof(ProgressPanel),
                new PropertyMetadata(RelativeAlignment.Below, ProgressAlignmentChangedCallback));

        private static void ProgressAlignmentChangedCallback(DependencyObject d, 
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as ProgressPanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnAlignmentChanged((RelativeAlignment) dpc.NewValue);
            }
        }

        private ProgressRing _progress;
        private Grid _root;

        public ProgressPanel()
        {
            this.DefaultStyleKey = typeof(ProgressPanel);
            this.Opacity = 0.0;
            this.Visibility = Visibility.Collapsed;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _root = (Grid)this.GetTemplateChild("PART_RootGrid");
            _progress = (ProgressRing) this.GetTemplateChild("PART_Progress");
            OnAlignmentChanged(this.ProgressAlignment);
            OnIsVisibleChanged(this.IsVisible);
        }

        protected async void OnIsVisibleChanged(bool isVisible)
        {
            if (_progress != null) _progress.IsActive = isVisible;

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
            if (_progress == null || _root == null) return;
            
            // Reset grid
           
            _root.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Auto);
            _root.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            _root.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Auto);
            _root.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
            _root.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Auto);
            switch (newAlignment)
            {
                case RelativeAlignment.Left:
                    {
                        Grid.SetRow(_progress, 1);
                        Grid.SetColumn(_progress, 0);
                        _root.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                        break;
                    }
                case RelativeAlignment.Above:
                    {
                        Grid.SetRow(_progress, 0);
                        Grid.SetColumn(_progress, 0);
                        Grid.SetColumnSpan(_progress, 3);
                        _root.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                        _root.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Auto);
                        break;
                    }
                case RelativeAlignment.Right:
                    {
                        Grid.SetRow(_progress, 1);
                        Grid.SetColumn(_progress, 2);
                        _root.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                        break;
                    }
                case RelativeAlignment.Below:
                    {
                        Grid.SetRow(_progress, 2);
                        Grid.SetColumn(_progress, 0);
                        Grid.SetColumnSpan(_progress, 3);
                        _root.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Auto);
                        _root.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
