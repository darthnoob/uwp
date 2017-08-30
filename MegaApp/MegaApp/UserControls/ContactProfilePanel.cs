using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels.Contacts;

namespace MegaApp.UserControls
{
    public class ContactProfilePanel : ContentControl
    {
        /// <summary>
        /// Gets or sets the Contact
        /// </summary>
        /// <value>The header text</value>
        public ContactViewModel Contact
        {
            get { return (ContactViewModel)GetValue(ContactProperty); }
            set { SetValue(ContactProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="Contact" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContactProperty =
            DependencyProperty.Register(
                nameof(Contact),
                typeof(ContactViewModel),
                typeof(ContactProfilePanel),
                new PropertyMetadata(null, ContactChangedCallback));

        private static void ContactChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as ContactProfilePanel;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnContactChanged((ContactViewModel)dpc.NewValue);
            }
        }

        private Button _removeContact;

        public ContactProfilePanel()
        {
            this.DefaultStyleKey = typeof(ContactProfilePanel);
            this.Opacity = 1;
            this.Visibility = Visibility.Visible;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._removeContact = (Button)this.GetTemplateChild("PART_RemoveContactButton");
            if(this._removeContact != null) this._removeContact.Tapped += (sender, args) => OnRemoveContactTapped();
        }

        protected void OnContactChanged(ContactViewModel contact)
        {
            
        }

        private async void OnRemoveContactTapped()
        {
            await this.Contact.RemoveContactAsync();
        }
    }
}
