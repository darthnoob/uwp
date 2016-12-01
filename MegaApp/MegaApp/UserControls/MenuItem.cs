using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.UserControls
{
    /// <summary>
    /// Class that represents a menu item
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Icon to show in the menu. Can be of type FontIcon, SymbolIcon or PathIcon
        /// </summary>
        public IconElement Icon { get; set; }

        /// <summary>
        /// Name to display as label for the menu item
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Secondary label for the menu item
        /// </summary>
        public string SubLabel { get; set; }

        /// <summary>
        /// Type of the viewmodel to navigate on selection
        /// </summary>
        public Type TargetViewModel { get; set; }

        /// <summary>
        /// Create list of default menu items
        /// </summary>
        /// <returns>Default menu item list</returns>
        public static IList<MenuItem> CreateMenuItems()
        {
            return new List<MenuItem>()
            {
                new MenuItem()
                {
                    Label = CloudDriveText,
                    SubLabel = CameraUploadsText + " & " + RubbishBinText,
                    Icon = new SymbolIcon(Symbol.Home),
                    TargetViewModel = typeof(CloudDriveViewModel)
                },
            };
        }
        
        /// <summary>
        /// Create list of option menu items
        /// </summary>
        /// <returns>Option menu items list</returns>
        public static IList<MenuItem> CreateOptionItems()
        {
            return new List<MenuItem>()
            {
                new MenuItem()
                {
                    Label = MyAccountText,
                    SubLabel = "& " + SettingsText,
                    Icon = new SymbolIcon(Symbol.Setting),
                    TargetViewModel = typeof(SettingsViewModel)
                },
            };
        }

        #region Ui_Resources

        private static string CameraUploadsText => ResourceService.UiResources.GetString("UI_CameraUploads");
        private static string CloudDriveText => ResourceService.UiResources.GetString("UI_CloudDriveName");
        private static string MyAccountText => ResourceService.UiResources.GetString("UI_MyAccount");
        private static string RubbishBinText => ResourceService.UiResources.GetString("UI_RubbishBinName");
        private static string SettingsText => ResourceService.UiResources.GetString("UI_Settings");

        #endregion
    }
}
