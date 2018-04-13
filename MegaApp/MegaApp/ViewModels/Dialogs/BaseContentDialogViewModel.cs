using System;
using System.Windows.Input;
using MegaApp.Classes;

namespace MegaApp.ViewModels.Dialogs
{
    public class BaseContentDialogViewModel : BaseUiViewModel
    {
        public BaseContentDialogViewModel()
        {
            this.CloseCommand = new RelayCommand(this.Close);
        }

        #region Commands

        /// <summary>
        /// Command invoked when the user clicks the close button of the 
        /// top-right corner of the dialog
        /// </summary>
        public ICommand CloseCommand { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the user closes the dialog using the close
        /// button of the top-right corner of the dialog.
        /// </summary>
        public event EventHandler CloseDialog;

        /// <summary>
        /// Event invocator method called when the user closes the dialog using 
        /// the close button of the top-right corner of the dialog.
        /// </summary>
        protected virtual void OnCloseDialog()
        {
            this.CanClose = true;
            this.CloseDialog?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Close the dialog
        /// </summary>
        private void Close() => this.OnCloseDialog();

        #endregion

        #region Properties

        /// <summary>
        /// Flag to indicate if the dialog can be closed
        /// </summary>
        public bool CanClose = false;

        #endregion
    }
}
