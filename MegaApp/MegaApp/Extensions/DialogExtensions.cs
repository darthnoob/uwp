using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MegaApp.UserControls;

namespace MegaApp.Extensions
{
    /// <summary>
    /// This class contains extension methods to dialog classes to accommodate queuing up dialogs if more than one is in current view
    /// </summary>
    public static class DialogExtensions
    {
        private static TaskCompletionSource<MessageDialog> _messageDialogShowRequest;        
        private static TaskCompletionSource<MegaContentDialog> _contentDialogShowRequest;

        /// <summary>
        /// Begins an asynchronous operation showing a <see cref="MessageDialog"/>.
        /// If another <see cref="MessageDialog"/> is already shown using this method, it will wait for that previous dialog
        /// to be dismissed before showing the new one.
        /// </summary>
        /// <param name="dialog">The <see cref="MessageDialog"/>.</param>
        /// <returns>The <see cref="MessageDialog"/> result.</returns>
        /// <exception cref="InvalidOperationException">This method can only be invoked from the UI thread.</exception>
        public static async Task<IUICommand> ShowAsyncQueue(this MessageDialog dialog)
        {
            if (!Window.Current.Dispatcher.HasThreadAccess)
                throw new InvalidOperationException("This method can only be invoked from UI thread.");

            while (_messageDialogShowRequest != null)
                await _messageDialogShowRequest.Task;

            while (_contentDialogShowRequest != null)
                await _contentDialogShowRequest.Task;

            var request = _messageDialogShowRequest = new TaskCompletionSource<MessageDialog>();
            var result = await dialog.ShowAsync();
            _messageDialogShowRequest = null;
            request.SetResult(dialog);

            return result;
        }

        /// <summary>
        /// Begins an asynchronous operation showing a <see cref="MegaContentDialog"/>.
        /// If another <see cref="MegaContentDialog"/> is already shown using this method, it will wait for that previous dialog
        /// to be dismissed before showing the new one.
        /// </summary>
        /// <param name="dialog">The <see cref="MegaContentDialog"/>.</param>
        /// <returns>The <see cref="MegaContentDialog"/> result.</returns>
        /// <exception cref="InvalidOperationException">This method can only be invoked from the UI thread.</exception>
        public static async Task<ContentDialogResult> ShowAsyncQueue(this MegaContentDialog dialog)
        {
            if (!Window.Current.Dispatcher.HasThreadAccess)
                throw new InvalidOperationException("This method can only be invoked from UI thread.");

            while (_contentDialogShowRequest != null)
                await _contentDialogShowRequest.Task;

            while (_messageDialogShowRequest != null)
                await _messageDialogShowRequest.Task;

            var request = _contentDialogShowRequest = new TaskCompletionSource<MegaContentDialog>();
            var result = await dialog.ShowAsync();
            _contentDialogShowRequest = null;
            request.SetResult(dialog);

            return result;
        }

        /// <summary>
        /// Begins an asynchronous operation showing a <see cref="MegaContentDialog"/>.
        /// If another <see cref="MegaContentDialog"/> is already shown using this method, it will wait for that previous dialog
        /// to be dismissed before showing the new one.
        /// </summary>
        /// <param name="dialog">The <see cref="MegaContentDialog"/>.</param>
        /// <returns>The <see cref="MegaContentDialog"/> result as <see cref="bool"/> value.</returns>
        /// <exception cref="InvalidOperationException">This method can only be invoked from the UI thread.</exception>
        public static async Task<bool> ShowAsyncQueueBool(this MegaContentDialog dialog)
        {
            var result = await ShowAsyncQueue(dialog);
            return result == ContentDialogResult.Primary || dialog.DialogResult;
        }
    }
}
