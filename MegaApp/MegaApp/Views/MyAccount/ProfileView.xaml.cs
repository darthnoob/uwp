using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using MegaApp.Services;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Views.MyAccount
{
    public sealed partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            this.InitializeComponent();

            this.ViewModel = new ProfileViewModel();

            this.DataContext = this.ViewModel;
        }

        #region Properties

        public StackPanel ViewArea => this.MainStackPanel;

        public ProfileViewModel ViewModel { get; }

        private bool IsFirstNameChanged => 
            !this.FirstNameTextBox.Text.Equals(AccountService.UserData.Firstname);

        private bool IsLastNameChanged => 
            !this.LastNameTextBox.Text.Equals(AccountService.UserData.Lastname);

        #endregion

        #region Private Methods

        private void OnUserAttributeChanged(object sender, TextChangedEventArgs e)
        {
            if (IsFirstNameChanged || IsLastNameChanged)
                this.AttributesModifiedButtons.Visibility = Visibility.Visible;
            else
                this.AttributesModifiedButtons.Visibility = Visibility.Collapsed;
        }

        private async void OnSaveButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            bool result = true;
            if (IsFirstNameChanged)
                result = result & await this.ViewModel.SetFirstName(this.FirstNameTextBox.Text);

            if (IsLastNameChanged)
                result = result & await this.ViewModel.SetLastName(this.LastNameTextBox.Text);

            if(!result)
            {
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_UpdateProfileFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_UpdateProfileFailed"));
                return;
            }

            this.AttributesModifiedButtons.Visibility = Visibility.Collapsed;
        }

        private void OnCancelButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            this.FirstNameTextBox.Text = AccountService.UserData.Firstname;
            this.LastNameTextBox.Text = AccountService.UserData.Lastname;
        }

        #endregion
    }
}
