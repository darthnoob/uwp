using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class OkCancelAndWarningDialogViewModel : BaseViewModel
    {
        public OkCancelAndWarningDialogViewModel()
        {
            this.OkButtonCommand = new RelayCommand(Accept);
            this.CancelButtonCommand = new RelayCommand(Cancel);
        }

        #region Commands

        public ICommand OkButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the user taps the "Ok" button.
        /// </summary>
        public event EventHandler OkButtonTapped;

        /// <summary>
        /// Event invocator method called when the user taps the "Ok" button.
        /// </summary>
        protected virtual void OnOkButtonTapped()
        {
            this.OkButtonTapped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the user taps the "Cancel" button.
        /// </summary>
        public event EventHandler CancelButtonTapped;

        /// <summary>
        /// Event invocator method called when the user taps the "Cancel" button.
        /// </summary>
        protected virtual void OnCancelButtonTapped()
        {
            this.CancelButtonTapped?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        private void Accept()
        {
            OnOkButtonTapped();
        }

        private void Cancel()
        {
            OnCancelButtonTapped();
        }

        #endregion

        #region Properties

        private string _titleText;
        /// <summary>
        /// 'Title' of the dialog
        /// </summary>
        public string TitleText
        {
            get { return _titleText; }
            set { SetField(ref _titleText, value); }
        }

        private string _okButtonLabelText;
        /// <summary>
        /// Label for the "Ok" button
        /// </summary>
        public string OkButtonLabelText
        {
            get { return _okButtonLabelText; }
            set { SetField(ref _okButtonLabelText, value); }
        }

        private string _cancelButtonLabelText;
        /// <summary>
        /// Label for the "Cancel" button
        /// </summary>
        public string CancelButtonLabelText
        {
            get { return _cancelButtonLabelText; }
            set { SetField(ref _cancelButtonLabelText, value); }
        }

        private string _messageText;
        /// <summary>
        /// Message to display in the dialog
        /// </summary>
        public string MessageText
        {
            get { return _messageText; }
            set { SetField(ref _messageText, value); }
        }

        private string _warningText;
        /// <summary>
        /// Warning to display in the dialog
        /// </summary>
        public string WarningText
        {
            get { return _warningText; }
            set { SetField(ref _warningText, value); }
        }

        #endregion

        #region VisualResources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }
}
