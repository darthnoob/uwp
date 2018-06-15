using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class TwoButtonsDialogViewModel : BaseContentDialogViewModel
    {
        public TwoButtonsDialogViewModel() : base()
        {
            this.PrimaryButtonCommand = new RelayCommand(PrimaryButtonAction);
            this.SecondaryButtonCommand = new RelayCommand(SecondaryButtonAction);
        }

        #region Commands

        /// <summary>
        /// Command invoked when the user tap the primary button.
        /// </summary>
        public virtual ICommand PrimaryButtonCommand { get; }

        /// <summary>
        /// Command invoked when the user tap the secondary button.
        /// </summary>
        public virtual ICommand SecondaryButtonCommand { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the user taps the primary button.
        /// </summary>
        public event EventHandler PrimaryButtonTapped;

        /// <summary>
        /// Event invocator method called when the user taps the primary button.
        /// </summary>
        protected virtual void OnPrimaryButtonTapped()
        {
            this.CanClose = true;
            this.PrimaryButtonTapped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the user taps the secondary button.
        /// </summary>
        public event EventHandler SecondaryButtonTapped;

        /// <summary>
        /// Event invocator method called when the user taps the secondary button.
        /// </summary>
        protected virtual void OnSecondaryButtonTapped()
        {
            this.CanClose = true;
            this.SecondaryButtonTapped?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Action to do by the primary button of the dialog
        /// </summary>
        protected virtual void PrimaryButtonAction() => OnPrimaryButtonTapped();

        /// <summary>
        /// Action to do by the secondary button of the dialog
        /// </summary>
        protected virtual void SecondaryButtonAction() => OnSecondaryButtonTapped();

        #endregion

        #region Properties

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
        /// Message of the dialog
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

        private string _primaryButtonLabel;
        /// <summary>
        /// Label of the primary button of the dialog
        /// </summary>
        public string PrimaryButtonLabel
        {
            get { return _primaryButtonLabel; }
            set { SetField(ref _primaryButtonLabel, value); }
        }

        private string _secondaryButtonLabel;
        /// <summary>
        /// Label of the secondary button of the dialog
        /// </summary>
        public string SecondaryButtonLabel
        {
            get { return _secondaryButtonLabel; }
            set { SetField(ref _secondaryButtonLabel, value); }
        }

        #endregion

        #region UiResources

        public string OkText => ResourceService.UiResources.GetString("UI_Ok");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string YesText => ResourceService.UiResources.GetString("UI_Yes");
        public string NoText => ResourceService.UiResources.GetString("UI_No");

        #endregion

        #region VisualResources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }

    public enum TwoButtonsDialogType
    {
        /// <summary>
        /// Displays Ok and Cancel buttons
        /// </summary>
        OkCancel,

        /// <summary>
        /// Displays Yes and No buttons
        /// </summary>
        YesNo,

        /// <summary>
        /// Displays custom buttons
        /// </summary>
        Custom
    }
}
