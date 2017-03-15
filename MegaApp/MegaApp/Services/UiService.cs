using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using mega;
using MegaApp.Enums;

namespace MegaApp.Services
{
    static class UiService
    {
        private static Dictionary<string, int> _folderSorting;
        private static Dictionary<string, int> _folderViewMode;

        /// <summary>
        /// Gets sort order of a folder.
        /// </summary>
        /// <param name="folderBase64Handle">Folder base 64 handle.</param>
        /// <param name="folderName">Folder name.</param>
        /// <returns>Sort order. Possible values: <see cref="MSortOrderType"/></returns>
        public static int GetSortOrder(string folderBase64Handle, string folderName)
        {
            if (_folderSorting == null)
                _folderSorting = new Dictionary<string, int>();

            if (_folderSorting.ContainsKey(folderBase64Handle))
                return _folderSorting[folderBase64Handle];

            return folderName.Equals("Camera Uploads") ? (int)MSortOrderType.ORDER_MODIFICATION_DESC :
                (int)MSortOrderType.ORDER_DEFAULT_ASC;
        }

        /// <summary>
        /// Sets sort order of a folder.
        /// </summary>
        /// <param name="folderBase64Handle">Folder base 64 handle.</param>
        /// <param name="sortOrder">Sort order. Possible values: <see cref="MSortOrderType"/></param>
        public static void SetSortOrder(string folderBase64Handle, int sortOrder)
        {
            if (_folderSorting == null)
                _folderSorting = new Dictionary<string, int>();

            if (_folderSorting.ContainsKey(folderBase64Handle))
                _folderSorting[folderBase64Handle] = sortOrder;
            else
                _folderSorting.Add(folderBase64Handle, sortOrder);
        }

        /// <summary>
        /// Gets the content view mode of a folder.
        /// </summary>
        /// <param name="folderBase64Handle">Folder base 64 handle.</param>
        /// <param name="folderName">Folder name.</param>
        /// <returns>Folder content view mode. Possible values: <see cref="FolderContentViewMode"/></returns>
        public static FolderContentViewMode GetViewMode(string folderBase64Handle, string folderName)
        {
            if (_folderViewMode == null)
                _folderViewMode = new Dictionary<string, int>();

            if (_folderViewMode.ContainsKey(folderBase64Handle))
                return (FolderContentViewMode)_folderViewMode[folderBase64Handle];

            return folderName.Equals("Camera Uploads") ? FolderContentViewMode.GridView : FolderContentViewMode.ListView;
        }

        /// <summary>
        /// Sets the content view mode of a folder.
        /// </summary>
        /// <param name="folderBase64Handle">Folder base 64 handle.</param>
        /// <param name="viewMode">Folder content view mode. Possible values: <see cref="FolderContentViewMode"/></param>        
        public static void SetViewMode(string folderBase64Handle, FolderContentViewMode viewMode)
        {
            if (_folderViewMode == null)
                _folderViewMode = new Dictionary<string, int>();

            if (_folderViewMode.ContainsKey(folderBase64Handle))
                _folderViewMode[folderBase64Handle] = (int)viewMode;
            else
                _folderViewMode.Add(folderBase64Handle, (int)viewMode);
        }

        /// <summary>
        /// Invoke the code/action on the UI Thread.
        /// </summary>
        /// <param name="action">Action to invoke on the user interface thread</param>
        /// <param name="priority">The priority of the dispatcher</param>
        public static void OnUiThread(Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            // If no action defined then do nothing and return to save time
            if (action == null) return;

            CoreApplication.MainView.Dispatcher.RunAsync(priority, action.Invoke);
        }

        /// <summary>
        /// Invoke the code/action on the UI Thread.
        /// </summary>
        /// <param name="action">Action to invoke on the user interface thread</param>
        /// <param name="priority">The priority of the dispatcher</param>
        /// <returns>Result of the action</returns>
        public static async Task OnUiThreadAsync(Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            // If no action defined then do nothing and return to save time
            if (action == null) return;

            await CoreApplication.MainView.Dispatcher.RunAsync(priority, action.Invoke);
        }

        /// <summary>
        /// Set the background color of the status bar if is present in the device.
        /// </summary>
        /// <param name="color">Background color for the status bar.</param>
        public static void SetStatusBarBackground(Color color)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusbar = StatusBar.GetForCurrentView();
                statusbar.BackgroundColor = color;
                statusbar.BackgroundOpacity = 1;
            }
        }

        /// <summary>
        /// Hide the status bar if is present in the device
        /// </summary>
        public static async void HideStatusBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusbar = StatusBar.GetForCurrentView();
                await statusbar.HideAsync();
            }
        }

        /// <summary>
        /// Show the status bar if is present in the device
        /// </summary>
        public static async void ShowStatusBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusbar = StatusBar.GetForCurrentView();
                await statusbar.ShowAsync();
            }
        }
    }
}
