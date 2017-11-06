using System;
using System.Collections.Generic;
using System.Linq;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.ViewModels.Login;

namespace MegaApp.ViewModels
{
    public class MainViewModel : LoginViewModel
    {
        public MainViewModel()
        {
            this.MenuItems = MenuItemViewModel.CreateMenuItems();
            this.OptionItems = MenuItemViewModel.CreateOptionItems();
            
            AccountService.UserData.UserEmailChanged += UserEmailChanged;
            AccountService.UserData.UserNameChanged += UserNameChanged;
        }

        /// <summary>
        /// Initialize the viewmodel
        /// </summary>
        /// <param name="navActionType">Navigation action used to arrive to the MainPage</param>
        public void Initialize(NavigationActionType navActionType = NavigationActionType.Default)
        {
            // Set the navigation action used to arrive to the MainPage
            this.NavActionType = navActionType;

            // Set the default menu item to home/first item
            this.SelectedItem = this.MenuItems.FirstOrDefault();
        }

        private void UserNameChanged(object sender, EventArgs e)
        {
            if (MyAccountMenuItem == null) return;
            OnUiThread(() => MyAccountMenuItem.Label = AccountService.UserData.UserName);
        }

        private void UserEmailChanged(object sender, EventArgs e)
        {
            if (MyAccountMenuItem == null) return;
            OnUiThread(() => MyAccountMenuItem.SubLabel = AccountService.UserData.UserEmail);
        }

        #region Properties

        /// <summary>
        /// Flag to temporarily disable the navigation when a menu item is selected
        /// <para>Default value: TRUE</para>
        /// </summary>
        public bool NavigateOnMenuItemSelected = true;

        private MenuItemViewModel _selectedItem;
        /// <summary>
        /// Current selected default menu item
        /// </summary>
        public MenuItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (!SetField(ref _selectedItem, value)) return;
                if (_selectedItem == null) return; // exit else both item lists will be set to null
                this.SelectedOptionItem = null;

                // Navigate to destination with TargetViewModel type
                if(this.NavigateOnMenuItemSelected)
                    NavigateTo(_selectedItem.TargetViewModel, this.NavActionType);
            }
        }

        private MenuItemViewModel _selectedOptionItem;
        /// <summary>
        /// Current selected option menu item
        /// </summary>
        public MenuItemViewModel SelectedOptionItem
        {
            get { return _selectedOptionItem; }
            set
            {
                if (!SetField(ref _selectedOptionItem, value)) return;
                if (_selectedOptionItem == null) return; // exit else both item lists will be set to null
                this.SelectedItem = null;

                // Navigate to destination with TargetViewModel type
                if (this.NavigateOnMenuItemSelected)
                    NavigateTo(_selectedOptionItem.TargetViewModel, this.NavActionType);
            }
        }

        /// <summary>
        /// My account option menu item
        /// </summary>
        private MenuItemViewModel MyAccountMenuItem => OptionItems.First(m => m.TargetViewModel == typeof(MyAccountViewModel));

        /// <summary>
        /// State of the controls attached to this viewmodel
        /// </summary>
        private BasePageViewModel _contentViewModel;
        public BasePageViewModel ContentViewModel
        {
            get { return _contentViewModel; }
            set { SetField(ref _contentViewModel, value); }
        }

        /// <summary>
        /// List of default menu items
        /// </summary>
        public IList<MenuItemViewModel> MenuItems { get; }

        /// <summary>
        /// List of option menu items
        /// </summary>
        public IList<MenuItemViewModel> OptionItems { get; }

        /// <summary>
        /// Navigation action used to arrive to the MainPage
        /// </summary>
        private NavigationActionType NavActionType { get; set; }

        #endregion
    }
}
