using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
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
                    Label = "Cloud Drive",
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
                    Label = "Settings",
                    Icon = new SymbolIcon(Symbol.Setting),
                    TargetViewModel = typeof(SettingsViewModel)
                },
            };
        }
    }
}
