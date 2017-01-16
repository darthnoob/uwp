using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    /// <summary>
    /// Base viewmodel that has background thread interaction with the UI
    /// Has OnUiThread function to dispatch data to the screen
    /// </summary>
    public abstract class BaseUiViewModel: BaseViewModel    
    {
        /// <summary>
        /// Invoke the code/action on the UI Thread. If not on UI thread, dispatch to UI with the Dispatcher
        /// </summary>
        /// <param name="action">Action to invoke on the user interface thread</param>
        /// <param name="priority">The priority of the dispatcher</param>
        public void OnUiThread(Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            // If no action defined then do nothing and return to save time
            if (action == null) return;

            UiService.OnUiThread(action);
        }
    }
}
