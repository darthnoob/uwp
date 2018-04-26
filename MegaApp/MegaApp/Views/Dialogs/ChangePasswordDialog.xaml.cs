using System;
using MegaApp.UserControls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.Views.Dialogs
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseChangePasswordDialog : ContentDialogEx<ChangePasswordDialogViewModel> { }

    public sealed partial class ChangePasswordDialog : BaseChangePasswordDialog
    {
        public ChangePasswordDialog()
        {
            this.InitializeComponent();

            this.ViewModel.PasswordChanged += OnPasswordChanged;
            this.ViewModel.Canceled += OnCanceled;
        }

        
        #region Private Methods

        private void OnPasswordChanged(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void OnCanceled(object sender, EventArgs e)
        {
            this.Hide();
        }        

        #endregion        
    }
}
