using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
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
            if (string.IsNullOrWhiteSpace(folderBase64Handle) || string.IsNullOrWhiteSpace(folderName))
                return (int)MSortOrderType.ORDER_NONE;

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
            if (string.IsNullOrWhiteSpace(folderBase64Handle)) return;

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

            // Start a task to avoid freeze the UI and the app
            Task.Run(() => CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(priority, action.Invoke));
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

        /// <summary>
        /// Regular expression to check and hexadecimal color string. 
        /// <para>Supports both “argb” and “rgb” with or without “#” in front of it.</para>        
        /// </summary>
        private static Regex _hexColorMatchRegex =
            new Regex("^#?(?<a>[a-z0-9][a-z0-9])?(?<r>[a-z0-9][a-z0-9])(?<g>[a-z0-9][a-z0-9])(?<b>[a-z0-9][a-z0-9])$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Get a Color object from an hexadecimal color string (for example for the user avatar color).
        /// <para>Supports both “argb” and “rgb” with or without “#” in front of it.</para>
        /// <para>Meaning: The string “#rrggbb” or “#aarrggbb” or “rrggbb” or “aarrggbb” will be converted to a Color object.</para>
        /// </summary>
        /// <param name="hexColorString">Hexadecimal color string.</param>
        /// <returns>
        /// Color object corresponding to the hexadecimal color string.
        /// </returns>
        public static Color GetColorFromHex(string hexColorString)
        {
            if (string.IsNullOrWhiteSpace(hexColorString))
                return Colors.Transparent;

            // Regex match the string
            var match = _hexColorMatchRegex.Match(hexColorString);

            // If no matches return the MEGA red color.
            if (!match.Success)
                return Colors.Transparent;

            byte a = 255, r = 0, b = 0, g = 0;

            // a value is optional            
            if (match.Groups["a"].Success)
                a = Convert.ToByte(match.Groups["a"].Value, 16);

            // r,g,b values are not optional
            r = Convert.ToByte(match.Groups["r"].Value, 16);
            g = Convert.ToByte(match.Groups["g"].Value, 16);
            b = Convert.ToByte(match.Groups["b"].Value, 16);

            return Color.FromArgb(a, r, g, b);
        }
    }
}
