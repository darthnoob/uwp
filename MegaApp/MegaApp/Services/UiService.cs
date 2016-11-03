using System.Collections.Generic;
using mega;
using MegaApp.Enums;
using System;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

namespace MegaApp.Services
{
    static class UiService
    {
        private static Dictionary<string, int> _folderSorting;
        private static Dictionary<string, int> _folderViewMode;

        public static int GetSortOrder(string folderBase64Handle, string folderName)
        {
            if (_folderSorting == null)
                _folderSorting = new Dictionary<string, int>();

            if (_folderSorting.ContainsKey(folderBase64Handle))
                return _folderSorting[folderBase64Handle];

            return folderName.Equals("Camera Uploads") ? (int)MSortOrderType.ORDER_MODIFICATION_DESC :
                (int)MSortOrderType.ORDER_DEFAULT_ASC;
        }

        public static void SetSortOrder(string folderBase64Handle, int sortOrder)
        {
            if (_folderSorting == null)
                _folderSorting = new Dictionary<string, int>();

            if (_folderSorting.ContainsKey(folderBase64Handle))
                _folderSorting[folderBase64Handle] = sortOrder;
            else
                _folderSorting.Add(folderBase64Handle, sortOrder);
        }

        public static ViewMode GetViewMode(string folderBase64Handle, string folderName)
        {
            if (_folderViewMode == null)
                _folderViewMode = new Dictionary<string, int>();

            if (_folderViewMode.ContainsKey(folderBase64Handle))
                return (ViewMode)_folderViewMode[folderBase64Handle];

            return folderName.Equals("Camera Uploads") ? ViewMode.LargeThumbnails : ViewMode.ListView;
        }

        public static void SetViewMode(string folderBase64Handle, ViewMode viewMode)
        {
            if (_folderViewMode == null)
                _folderViewMode = new Dictionary<string, int>();

            if (_folderViewMode.ContainsKey(folderBase64Handle))
                _folderViewMode[folderBase64Handle] = (int)viewMode;
            else
                _folderViewMode.Add(folderBase64Handle, (int)viewMode);
        }

        /// <summary>
        /// Invoke the code/action on the UI Thread. If not on UI thread, dispatch to UI with the Dispatcher
        /// </summary>
        /// <param name="action">Action to invoke on the user interface thread</param>
        /// <param name="priority">The priority of the dispatcher</param>
        public static async void OnUiThread(Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            // If no action defined then do nothing and return to save time
            if (action == null) return;

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                // We are already on UI thread. Just invoke the action
                action.Invoke();
            }
            else
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal, action.Invoke);
            }
        }
    }
}
