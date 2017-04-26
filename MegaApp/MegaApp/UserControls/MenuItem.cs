using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.UserControls
{
    /// <summary>
    /// Class that represents a menu item
    /// </summary>
    public class MenuItem
    {
        #region Properties

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
        /// Tooltip for the menu item
        /// </summary>
        public string ToolTip { get; set; }

        /// <summary>
        /// Type of the viewmodel to navigate on selection
        /// </summary>
        public Type TargetViewModel { get; set; }

        #endregion

        #region Public Methods

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
                    ToolTip = CloudDriveText,
                    Icon = new SymbolIcon(Symbol.Home),
                    TargetViewModel = typeof(CloudDriveViewModel)
                },

                new MenuItem()
                {
                    Label = TransferManagerText,
                    ToolTip = TransferManagerText,
                    Icon = GetIconFromXamlPath(MenuTransfersPathData),
                    TargetViewModel = typeof(TransferManagerViewModel)
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
                    ToolTip = MyAccountText,
                    Icon = GetIconFromXamlPath(MenuSettingsPathData),
                    TargetViewModel = typeof(SettingsMyAccountViewModel)
                },
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets an icon from a XAML path data
        /// </summary>
        /// <param name="xamlPathData">XAML path data to draw the icon</param>
        /// <returns>Icon corresponding with the XAML path data</returns>
        private static PathIcon GetIconFromXamlPath(string xamlPathData)
        {
            var geometry = (Geometry)XamlReader.Load(
                "<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>"
                + xamlPathData + "</Geometry>");

            return new PathIcon()
            {
                Data = geometry,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        #endregion

        #region Ui_Resources

        private static string CameraUploadsText => ResourceService.UiResources.GetString("UI_CameraUploads");
        private static string CloudDriveText => ResourceService.UiResources.GetString("UI_CloudDriveName");
        private static string MyAccountText => ResourceService.UiResources.GetString("UI_MyAccount");
        private static string RubbishBinText => ResourceService.UiResources.GetString("UI_RubbishBinName");
        private static string SettingsText => ResourceService.UiResources.GetString("UI_Settings");
        private static string TransferManagerText => ResourceService.UiResources.GetString("UI_TransferManager");

        #endregion

        #region VisualResources

        private static string MenuContactsPathData => ResourceService.VisualResources.GetString("VR_MenuContactsPathData");
        private static string MenuSaveForOfflinePathData => ResourceService.VisualResources.GetString("VR_MenuSaveForOfflinePathData");
        private static string MenuSettingsPathData => ResourceService.VisualResources.GetString("VR_MenuSettingsPathData");
        private static string MenuSharedPathData => ResourceService.VisualResources.GetString("VR_MenuSharedPathData");
        private static string MenuTransfersPathData => ResourceService.VisualResources.GetString("VR_MenuTransfersPathData");

        #endregion
    }
}
