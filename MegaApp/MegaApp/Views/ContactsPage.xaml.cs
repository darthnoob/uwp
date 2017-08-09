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
    public class BaseContactsPage : PageEx<ContactsViewModel> { }

    public sealed partial class ContactsPage : BaseContactsPage
    {
        public ContactsPage()
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
            if (this.ContactsPagePivot.SelectedItem.Equals(this.ContactsPivot) ||
                this.ContactsPagePivot.SelectedItem.Equals(this.OutgoingPivot))
            {
                this.AddContactCommandBarButton.Visibility = Visibility.Visible;
            }
            else if (this.ContactsPagePivot.SelectedItem.Equals(this.IncomingPivot))
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
