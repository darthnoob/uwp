using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BasePasswordReminderDialog : ContentDialogEx<PasswordReminderDialogViewModel> { }

    public sealed partial class PasswordReminderDialog : BasePasswordReminderDialog
    {
        public PasswordReminderDialog()
        {
            this.InitializeComponent();
        }
    }
}
