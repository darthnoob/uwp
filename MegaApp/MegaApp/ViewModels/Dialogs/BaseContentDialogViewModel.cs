using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class BaseContentDialogViewModel : BaseUiViewModel
    {
        public BaseContentDialogViewModel(bool hasCloseButton = false)
        {
            this.HasCloseButton = hasCloseButton;
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

        private bool _hasCloseButton;
        /// <summary>
        /// Flag to indicate if the dialog will have a close button at the top-right corner
        /// </summary>
        public bool HasCloseButton
        {
            get { return _hasCloseButton; }
            set
            {
                SetField(ref _hasCloseButton, value);
                OnPropertyChanged(nameof(this.TitleMargin));
            }
        }

        /// <summary>
        /// Margin for the title of the dialog.
        /// Varies depending on if it has a close button at the top-right corner or not
        /// </summary>
        public Thickness TitleMargin => this.HasCloseButton ?
            new Thickness(24, 0, 0, 0) : new Thickness(24, 24, 0, 0);

        #endregion

        #region UiResources

        public string CloseText => ResourceService.UiResources.GetString("UI_Close");

        #endregion
    }
}
