using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MegaApp.Services;

namespace MegaApp.UserControls
{
    public class MegaContentDialog : ContentDialog
    {
        public MegaContentDialog()
        {
            this.DefaultStyleKey = typeof(MegaContentDialog);
            this.CloseButtonLabel = ResourceService.UiResources.GetString("UI_Close");
            this.CloseButtonVisibility = Visibility.Collapsed;
            this.TitleMargin = new Thickness(24, 24, 0, 0);
        }

        /// <summary>
        /// The result of the dialog as <see cref="bool"/> value.
        /// </summary>
        public bool DialogResult;

        /// <summary>
        /// Get or set the close button command
        /// </summary>
        public ICommand CloseButtonCommand
        {
            get { return (ICommand)GetValue(CloseButtonCommandProperty); }
            set { SetValue(CloseButtonCommandProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="CloseButtonCommand"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CloseButtonCommandProperty =
            DependencyProperty.Register(
                nameof(CloseButtonCommand),
                typeof(ICommand),
                typeof(MegaContentDialog),
                new PropertyMetadata(null));

        /// <summary>
        /// Get or set the close button label
        /// </summary>
        public string CloseButtonLabel
        {
            get { return (string)GetValue(CloseButtonLabelProperty); }
            set { SetValue(CloseButtonLabelProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="CloseButtonLabel"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CloseButtonLabelProperty =
            DependencyProperty.Register(
                nameof(CloseButtonLabel),
                typeof(string),
                typeof(MegaContentDialog),
                new PropertyMetadata(null));

        /// <summary>
        /// Get or set the close button visibility
        /// </summary>
        public Visibility CloseButtonVisibility
        {
            get { return (Visibility)GetValue(CloseButtonVisibilityProperty); }
            set { SetValue(CloseButtonVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="CloseButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CloseButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(CloseButtonVisibility),
                typeof(Visibility),
                typeof(MegaContentDialog),
                new PropertyMetadata(null, new PropertyChangedCallback(OnCloseButtonVisibilityValueChanged)));

        private static void OnCloseButtonVisibilityValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MegaContentDialog dialog = d as MegaContentDialog;
            if (!e.NewValue.Equals(e.OldValue))
            {
                dialog.TitleMargin = (Visibility)e.NewValue == Visibility.Visible ?
                    new Thickness(24, 40, 0, 0) : new Thickness(24, 24, 0, 0);
            }
        }

        /// <summary>
        /// Get or set the margins of the title text of the dialog
        /// </summary>
        public Thickness TitleMargin
        {
            get { return (Thickness)GetValue(TitleMarginProperty); }
            private set { SetValue(TitleMarginProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="TitleMargin"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TitleMarginProperty =
            DependencyProperty.Register(
                nameof(TitleMargin),
                typeof(Thickness),
                typeof(MegaContentDialog),
                new PropertyMetadata(null));
    }
}
