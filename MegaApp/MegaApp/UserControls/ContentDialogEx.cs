using System;
using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp.UserControls
{
    /// <summary>
    /// Typed ContentDialog extension that implements a view-model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ContentDialogEx<T> : MegaContentDialog
        where T : BaseContentDialogViewModel, new()
    {
        public ContentDialogEx()
        {
            // Create the viewmodel and bind it to the dialog main datacontext
            this.ViewModel = (T)Activator.CreateInstance(typeof(T));
            this.DataContext = this.ViewModel;

            this.ViewModel.CloseButtonTapped += OnCloseButtonTapped;
            this.ViewModel.HideDialog += OnHideDialog;
            this.ViewModel.ShowDialog+= OnShowDialog;
        }

        /// <summary>
        /// Current view-model binded to the datacontext
        /// </summary>
        public T ViewModel { get; }

        #region Methods

        private void OnCloseButtonTapped(object sender, EventArgs e) => this.Hide();

        private void OnHideDialog(object sender, EventArgs e) => this.Hide();

        private void OnShowDialog(object sender, EventArgs e) => this.ShowAsync();

        protected void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!this.ViewModel.CanClose)
                args.Cancel = true;
        }

        #endregion
    }
}
