using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public abstract class BaseContentDialogViewModel : BaseUiViewModel
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
        public event EventHandler CloseButtonTapped;

        /// <summary>
        /// Event invocator method called when the user closes the dialog using 
        /// the close button of the top-right corner of the dialog.
        /// </summary>
        protected virtual void OnCloseButtonTapped()
        {
            this.CanClose = true;
            this.CloseButtonTapped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered to hide the dialog.
        /// </summary>
        public event EventHandler HideDialog;

        /// <summary>
        /// Event invocator method called to hide the dialog.
        /// </summary>
        protected virtual void OnHideDialog()
        {
            this.CanClose = true;
            this.HideDialog?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered to show the dialog.
        /// </summary>
        public event EventHandler ShowDialog;

        /// <summary>
        /// Event invocator method called to show the dialog.
        /// </summary>
        protected virtual void OnShowDialog() =>
            this.ShowDialog?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Methods

        /// <summary>
        /// Close the dialog
        /// </summary>
        private void Close() => this.OnCloseButtonTapped();

        #endregion

        #region Properties

        /// <summary>
        /// Flag to indicate if the dialog can be closed
        /// </summary>
        public bool CanClose = false;

        private Visibility _closeButtonVisibility;
        /// <summary>
        /// Indicates if the dialog will have a close button at the top-right corner
        /// </summary>
        public Visibility CloseButtonVisibility
        {
            get { return _closeButtonVisibility; }
            set { SetField(ref _closeButtonVisibility, value); }
        }

        private string _closeButtonLabel;
        /// <summary>
        /// Label of the close button (top right corner) of the dialog
        /// </summary>
        public string CloseButtonLabel
        {
            get { return _closeButtonLabel; }
            set { SetField(ref _closeButtonLabel, value); }
        }

        private Style _dialogStyle;
        /// <summary>
        /// Style of the dialog
        /// </summary>
        public Style DialogStyle
        {
            get { return _dialogStyle; }
            set { SetField(ref _dialogStyle, value); }
        }

        private string _titleText;
        /// <summary>
        /// Title of the dialog
        /// </summary>
        public string TitleText
        {
            get { return _titleText; }
            set { SetField(ref _titleText, value); }
        }

        private string _messageText;
        /// <summary>
        /// Message or description text of the dialog
        /// </summary>
        public string MessageText
        {
            get { return _messageText; }
            set { SetField(ref _messageText, value); }
        }

        #endregion

        #region UiResources

        public string CloseText => ResourceService.UiResources.GetString("UI_Close");

        #endregion
    }

    public enum MegaDialogStyle
    {
        /// <summary>
        /// Displays a "MegaAlertDialogStyle"
        /// </summary>
        AlertDialog,

        /// <summary>
        /// Displays a "MegaContentDialogStyle"
        /// </summary>
        ContentDialog
    }
}
