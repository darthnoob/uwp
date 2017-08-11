using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generic in it's declaration.
    public class BaseContactsManagerPage : PageEx<ContactsManagerViewModel> { }

    public sealed partial class ContactsManagerPage : BaseContactsManagerPage
    {
        public ContactsManagerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Initialize(App.GlobalListener);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.Deinitialize(App.GlobalListener);
            base.OnNavigatedFrom(e);
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.ContactsPivot) ||
                this.ContactsManagerPagePivot.SelectedItem.Equals(this.OutgoingPivot))
            {
                this.AddContactCommandBarButton.Visibility = Visibility.Visible;
            }
            else if (this.ContactsManagerPagePivot.SelectedItem.Equals(this.IncomingPivot))
            {
                this.AddContactCommandBarButton.Visibility = Visibility.Collapsed;
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();
        }
    }
}
