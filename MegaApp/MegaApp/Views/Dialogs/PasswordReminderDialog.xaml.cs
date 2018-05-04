using Windows.System;
using Windows.UI.Xaml.Input;
using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BasePasswordReminderDialog : ContentDialogEx<PasswordReminderDialogViewModel> { }

    public sealed partial class PasswordReminderDialog : BasePasswordReminderDialog
    {
        public PasswordReminderDialog(bool atLogout)
        {
            this.InitializeComponent();

            this.ViewModel.AtLogout = atLogout;
        }

        private void OnPasswordKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;

            // On enter in password box, check the password
            if (this.ViewModel.CheckPasswordCommand.CanExecute(null))
                this.ViewModel.CheckPasswordCommand.Execute(null);

            e.Handled = true;
        }
    }
}
