using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    /// <summary>
    /// Class that represents a menu item
    /// </summary>
    public class MenuItemViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// Icon to show in the menu. Can be of type FontIcon, SymbolIcon or PathIcon
        /// </summary>
        public IconElement Icon { get; set; }

        /// <summary>
        /// Name to display as label for the menu item
        /// </summary>
        private string _label;
        public string Label
        {
            get { return _label; }
            set { SetField(ref _label, value); }
        }

        /// <summary>
        /// Secondary label for the menu item
        /// </summary>
        private string _subLabel;
        public string SubLabel
        {
            get { return _subLabel; }
            set { SetField(ref _subLabel, value); }
        }

        /// <summary>
        /// Tooltip for the menu item
        /// </summary>
        public string ToolTip { get; set; }

        /// <summary>
        /// Type of the viewmodel to navigate on selection
        /// </summary>
        public Type TargetViewModel { get; set; }

        public bool IsMyAccountMenuItem => TargetViewModel.Equals(typeof(MyAccountViewModel));

        public UserDataViewModel UserData => AccountService.UserData;

        #endregion

        #region Public Methods

        /// <summary>
        /// Create list of default menu items
        /// </summary>
        /// <returns>Default menu item list</returns>
        public static IList<MenuItemViewModel> CreateMenuItems()
        {
            return new List<MenuItemViewModel>()
            {
                new MenuItemViewModel()
                {
                    Label = CloudDriveText,
                    SubLabel = CameraUploadsText + " & " + RubbishBinText,
                    ToolTip = CloudDriveText,
                    Icon = GetIconFromXamlPath(MenuCloudPathData),
                    TargetViewModel = typeof(CloudDriveViewModel)
                },

                new MenuItemViewModel()
                {
                    Label = ContactsText,
                    ToolTip = ContactsText,
                    Icon = GetIconFromXamlPath(MenuContactsPathData),
                    TargetViewModel = typeof(ContactsManagerViewModel)
                },

                new MenuItemViewModel()
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
        public static IList<MenuItemViewModel> CreateOptionItems()
        {
            return new List<MenuItemViewModel>()
            {
                new MenuItemViewModel()
                {
                    Label = AccountService.UserData.UserName,
                    SubLabel = AccountService.UserData.UserEmail,
                    ToolTip = MyAccountText,
                    Icon = new SymbolIcon(Symbol.Contact),
                    TargetViewModel = typeof(MyAccountViewModel)
                },

                new MenuItemViewModel()
                {
                    Label = SettingsText,
                    ToolTip = SettingsText,
                    Icon = GetIconFromXamlPath(MenuSettingsPathData),
                    TargetViewModel = typeof(SettingsViewModel)
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
        private static string ContactsText => ResourceService.UiResources.GetString("UI_Contacts");
        private static string CloudDriveText => ResourceService.UiResources.GetString("UI_CloudDriveName");
        private static string MyAccountText => ResourceService.UiResources.GetString("UI_MyAccount");
        private static string RubbishBinText => ResourceService.UiResources.GetString("UI_RubbishBinName");
        private static string SettingsText => ResourceService.UiResources.GetString("UI_Settings");
        private static string TransferManagerText => ResourceService.UiResources.GetString("UI_TransferManager");

        #endregion

        #region VisualResources

        private static string MenuCloudPathData => ResourceService.VisualResources.GetString("VR_MenuCloudPathData");
        private static string MenuContactsPathData => ResourceService.VisualResources.GetString("VR_MenuContactsPathData");
        private static string MenuSaveForOfflinePathData => ResourceService.VisualResources.GetString("VR_MenuSaveForOfflinePathData");
        private static string MenuSettingsPathData => ResourceService.VisualResources.GetString("VR_MenuSettingsPathData");
        private static string MenuSharedPathData => ResourceService.VisualResources.GetString("VR_MenuSharedPathData");
        private static string MenuTransfersPathData => ResourceService.VisualResources.GetString("VR_MenuTransfersPathData");

        #endregion
    }
}
