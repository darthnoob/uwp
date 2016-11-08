using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using MegaApp.Services;

namespace MegaApp.Classes
{
    /// <summary>
    /// Class that provides functionality to show the user a MEGA styled message dialog
    /// </summary>
    public class CustomMessageDialog
    {
        #region Events

        public event EventHandler OkOrYesButtonTapped;
        public event EventHandler CancelOrNoButtonTapped;

        #endregion

        #region Controls

        protected MessageDialog DialogWindow { get; set; }        

        #endregion

        #region Private Properties

        private readonly string _title;
        private readonly string _message;
        private readonly AppInformation _appInformation;
        private TaskCompletionSource<MessageDialogResult> _taskCompletionSource;

        #endregion


        /// <summary>
        /// Create a CustomMessageDialog
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Main message of the dialog</param>
        /// <param name="appInformation">App information for restricting number of dialogs</param>        
        public CustomMessageDialog(string title, string message, AppInformation appInformation)
        {
            _title = title.ToUpper();
            _message = message;
            _appInformation = appInformation;

            // Create the message dialog and set its content
            DialogWindow = new MessageDialog(_message, _title);

            // Add commands and set their callbacks
            DialogWindow.Commands.Add(new UICommand(ResourceService.UiResources.GetString("UI_Ok"), 
                new UICommandInvokedHandler(this.OnOkOrYesButtonTapped), 0));

            // Set the command that will be invoked by default
            DialogWindow.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            DialogWindow.CancelCommandIndex = 0;
        }

        /// <summary>
        /// Create a CustomMessageDialog
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Main message of the dialog</param>
        /// <param name="appInformation">App information for restricting number of dialogs</param>
        /// <param name="dialogButtons">A value that indicaties the button or buttons to display</param>        
        public CustomMessageDialog(string title, string message, AppInformation appInformation,
            MessageDialogButtons dialogButtons)
        {
            _title = title.ToUpper();
            _message = message;
            _appInformation = appInformation;

            DialogWindow = new MessageDialog(_message, _title);
            
            // Create buttons defined in the constructor
            switch (dialogButtons)
            {
                case MessageDialogButtons.Ok:
                    DialogWindow.Commands.Add(new UICommand(ResourceService.UiResources.GetString("UI_Ok"),
                        new UICommandInvokedHandler(this.OnOkOrYesButtonTapped), 0));
                    DialogWindow.DefaultCommandIndex = 0;
                    DialogWindow.CancelCommandIndex = 0;
                    break;

                case MessageDialogButtons.OkCancel:
                    DialogWindow.Commands.Add(new UICommand(ResourceService.UiResources.GetString("UI_Ok"),
                        new UICommandInvokedHandler(this.OnOkOrYesButtonTapped), 0));
                    DialogWindow.Commands.Add(new UICommand(ResourceService.UiResources.GetString("UI_Cancel"),
                        new UICommandInvokedHandler(this.OnCancelOrNoButtonTapped), 1));
                    DialogWindow.DefaultCommandIndex = 0;
                    DialogWindow.CancelCommandIndex = 1;
                    break;

                case MessageDialogButtons.YesNo:
                    DialogWindow.Commands.Add(new UICommand(ResourceService.UiResources.GetString("UI_Yes"),
                        new UICommandInvokedHandler(this.OnOkOrYesButtonTapped), 0));
                    DialogWindow.Commands.Add(new UICommand(ResourceService.UiResources.GetString("UI_No"),
                        new UICommandInvokedHandler(this.OnCancelOrNoButtonTapped), 1));
                    DialogWindow.DefaultCommandIndex = 0;
                    DialogWindow.CancelCommandIndex = 1;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("dialogButtons", dialogButtons, null);
            }
        }

        /// <summary>
        /// Display the CustomMessageDialog on screen with the specified parameter from the constructor
        /// </summary>
        public void ShowDialog()
        {
            UiService.OnUiThread(async () => await DialogWindow.ShowAsync());
        }

        /// <summary>
        /// Display the CustomMessageDialog on screen with the specified parameter from the constructor
        /// </summary>
        /// <returns>Message dialog result specified by user button tap action</returns>
        public Task<MessageDialogResult> ShowDialogAsync()
        {
            // Needed to make a awaitable task
            _taskCompletionSource = new TaskCompletionSource<MessageDialogResult>();

            ShowDialog();

            // Return awaitable task
            return _taskCompletionSource.Task;
        }

        #region Virtual Methods

        /// <summary>
        /// Ok or Yes button has been tapped
        /// </summary>
        /// <param name="command">Command that was invoked</param>
        protected virtual void OnOkOrYesButtonTapped(IUICommand command)
        {
            this.DialogResult = MessageDialogResult.OkYes;
            _taskCompletionSource?.TrySetResult(this.DialogResult);
            OkOrYesButtonTapped?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Cancel or No button has been tapped
        /// </summary>
        /// <param name="command">Command that was invoked</param>
        protected virtual void OnCancelOrNoButtonTapped(IUICommand command)
        {
            this.DialogResult = MessageDialogResult.CancelNo;
            _taskCompletionSource?.TrySetResult(this.DialogResult);
            CancelOrNoButtonTapped?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Result of the dialog. Defined by the user action
        /// </summary>
        public MessageDialogResult DialogResult { get; private set; }

        #endregion
    }

    public enum MessageDialogButtons
    {
        /// <summary>
        /// Displays only an Ok button
        /// </summary>
        Ok,
        /// <summary>
        /// Displays an Ok and Cancel button
        /// </summary>
        OkCancel,
        /// <summary>
        /// Displays a Yes and No button
        /// </summary>
        YesNo,
    }

    public enum MessageDialogResult
    {
        /// <summary>
        /// User has pressed Ok or Yes
        /// </summary>
        OkYes,
        /// <summary>
        /// User has pressed Cancel or No
        /// </summary>
        CancelNo,
        /// <summary>
        /// User has pressed a custom defined button
        /// </summary>
        Custom,
    }
}
