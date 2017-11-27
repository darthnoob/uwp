using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.UI.Animations;

namespace MegaApp.UserControls
{
    public class ActivityIndicator : ContentControl
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
                typeof(ActivityIndicator),
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
                typeof(ActivityIndicator),
                new PropertyMetadata(null, SubHeaderChanged));

        /// <summary>
        /// Gets or sets the progress text.
        /// </summary>
        /// <value>The progress text</value>
        public string ProgressText
        {
            get { return (string)GetValue(ProgressTextProperty); }
            set { SetValue(ProgressTextProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ProgressText" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register(
                nameof(ProgressText),
                typeof(string),
                typeof(ActivityIndicator),
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
                typeof(ActivityIndicator),
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
                typeof(ActivityIndicator),
                new PropertyMetadata(16));

        /// <summary>
        /// Gets or sets the progress text font size.
        /// </summary>
        /// <value>The progress text font size</value>
        public double ProgressTextFontSize
        {
            get { return (double)GetValue(ProgressTextFontSizeProperty); }
            set { SetValue(ProgressTextFontSizeProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ProgressTextFontSize" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProgressTextFontSizeProperty =
            DependencyProperty.Register(
                nameof(ProgressTextFontSize),
                typeof(double),
                typeof(ActivityIndicator),
                new PropertyMetadata(16));

        /// <summary>
        /// Gets or sets the header foreground color.
        /// </summary>
        /// <value>the header color</value>
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
                typeof(ActivityIndicator),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the sub header foreground color.
        /// </summary>
        /// <value>the sub header color</value>
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
                typeof(ActivityIndicator),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the progress text foreground color.
        /// </summary>
        /// <value>the progress text color</value>
        public Brush ProgressTextForeground
        {
            get { return (Brush)GetValue(ProgressTextForegroundProperty); }
            set { SetValue(ProgressTextForegroundProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ProgressTextForeground" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProgressTextForegroundProperty =
            DependencyProperty.Register(
                nameof(ProgressTextForeground),
                typeof(Brush),
                typeof(ActivityIndicator),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the margin of the sub header.
        /// </summary>
        /// <value>Margin sub header</value>
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
                typeof(ActivityIndicator),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the margin of the progress text.
        /// </summary>
        /// <value>Margin progress text</value>
        public Thickness ProgressTextMargin
        {
            get { return (Thickness)GetValue(ProgressTextMarginProperty); }
            set { SetValue(ProgressTextMarginProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ProgressTextMargin" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProgressTextMarginProperty =
            DependencyProperty.Register(
                nameof(ProgressTextMargin),
                typeof(Thickness),
                typeof(ActivityIndicator),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the size of the progress ring on desktop.
        /// </summary>
        /// <value>Size of progress</value>
        public double ProgressSizeDesktop
        {
            get { return (double)GetValue(ProgressSizeDesktopProperty); }
            set { SetValue(ProgressSizeDesktopProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ProgressSizeDesktop" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProgressSizeDesktopProperty =
            DependencyProperty.Register(
                nameof(ProgressSizeDesktop),
                typeof(double),
                typeof(ActivityIndicator),
                new PropertyMetadata(48));

        /// <summary>
        /// Gets or sets the size of the progress ring on mobile.
        /// </summary>
        /// <value>Size of progress</value>
        public double ProgressSizeMobile
        {
            get { return (double)GetValue(ProgressSizeMobileProperty); }
            set { SetValue(ProgressSizeMobileProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="ProgressSizeMobile" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProgressSizeMobileProperty =
            DependencyProperty.Register(
                nameof(ProgressSizeMobile),
                typeof(double),
                typeof(ActivityIndicator),
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
                typeof(ActivityIndicator),
                new PropertyMetadata(false, IsVisibleChangedCallback));

        private static void IsVisibleChangedCallback(DependencyObject d, 
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as ActivityIndicator;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnIsVisibleChanged((bool)dpc.NewValue);
            }
        }

        private static void SubHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as ActivityIndicator;
            control?.SetSubHeaderVisibility();
        }

        private ProgressRing _progress;
        public TextBlock SubHeaderControl { get; private set; }

        public ActivityIndicator()
        {
            this.DefaultStyleKey = typeof(ActivityIndicator);
            this.Opacity = 0.0;
            this.Visibility = Visibility.Collapsed;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _progress = (ProgressRing) this.GetTemplateChild("PART_Progress");
            SubHeaderControl = (TextBlock) this.GetTemplateChild("PART_SubHeaderText");
            SetSubHeaderVisibility();
            OnIsVisibleChanged(this.IsVisible);
        }

        private void SetSubHeaderVisibility()
        {
            if (SubHeaderControl == null) return;
            SubHeaderControl.Visibility = string.IsNullOrWhiteSpace(SubHeaderText) 
                ? Visibility.Collapsed 
                : Visibility.Visible;
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
                this.Fade(1.0f, 250.0).Start();
                return;
            }

            await this.Fade(0.0f, 250.0).StartAsync();
            this.Visibility = Visibility.Collapsed;
        }
    }
}
