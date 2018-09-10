using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.ViewModels.Login;

namespace MegaApp.ViewModels
{
    public class MainViewModel : LoginViewModel
    {
        public MainViewModel() : base(SdkService.MegaSdk)
        {
            this.MenuItems = MenuItemViewModel.CreateMenuItems();
            this.OptionItems = MenuItemViewModel.CreateOptionItems();
            
            AccountService.UserData.UserEmailChanged += UserEmailChanged;
            AccountService.UserData.UserNameChanged += UserNameChanged;

            SdkService.ApiUrlChanged += ApiUrlChanged;

            this.CloseOfflineBannerCommand = new RelayCommand(CloseOfflineBanner);
        }

        #region Commands

        public ICommand CloseOfflineBannerCommand { get; }

        #endregion

        #region Methods

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

        protected void UserNameChanged(object sender, EventArgs e)
        {
            if (MyAccountMenuItem == null) return;
            OnUiThread(() => MyAccountMenuItem.Label = AccountService.UserData.UserName);
        }

        protected void UserEmailChanged(object sender, EventArgs e)
        {
            if (MyAccountMenuItem == null) return;
            OnUiThread(() => MyAccountMenuItem.SubLabel = AccountService.UserData.UserEmail);
        }

        private async void ApiUrlChanged(object sender, EventArgs e)
        {
            // If the user is logged in, do a new fetch nodes
            if (Convert.ToBoolean(MegaSdk.isLoggedIn()))
            {
                this.ProgressHeaderText = ResourceService.ProgressMessages.GetString("PM_Reloading");

                // Fetch nodes from MEGA
                var fetchNodesResult = await this.FetchNodes();
                if (fetchNodesResult != FetchNodesResult.Success)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Fetch nodes failed.");
                    if (!AccountService.IsAccountBlocked)
                        this.ShowFetchNodesFailedAlertDialog();
                    return;
                }
            }

            ToastService.ShowTextNotification("API URL changed");
        }

        private void CloseOfflineBanner()
        {
            if (!(this.ContentViewModel is SavedForOfflineViewModel)) return;
            (this.ContentViewModel as SavedForOfflineViewModel).ShowOfflineBanner = false;
            OnPropertyChanged(nameof(this.ShowOfflineBanner));
        }

        #endregion

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
            set
            {
                SetField(ref _contentViewModel, value);
                OnPropertyChanged(nameof(this.ShowOfflineBanner),
                    nameof(this.ShowOfflineBannerCloseButton));
            }
        }

        private IList<MenuItemViewModel> _menuItems;
        /// <summary>
        /// List of default menu items
        /// </summary>
        public IList<MenuItemViewModel> MenuItems
        {
            get { return _menuItems; }
            set { SetField(ref _menuItems, value); }
        }

        private IList<MenuItemViewModel> _optionItems;
        /// <summary>
        /// List of option menu items
        /// </summary>
        public IList<MenuItemViewModel> OptionItems
        {
            get { return _optionItems; }
            set { SetField(ref _optionItems, value); }
        }

        /// <summary>
        /// Navigation action used to arrive to the MainPage
        /// </summary>
        private NavigationActionType NavActionType { get; set; }

        public bool ShowOfflineBanner => this.ContentViewModel is SavedForOfflineViewModel ?
            (this.ContentViewModel as SavedForOfflineViewModel).ShowOfflineBanner : true;

        public bool ShowOfflineBannerCloseButton =>
            this.ContentViewModel is SavedForOfflineViewModel;

        #endregion

        #region UiResources

        public string CloseText => ResourceService.UiResources.GetString("UI_Close");

        #endregion

        #region VisualResources

        public string ClosePathData => ResourceService.VisualResources.GetString("VR_ClosePathData");

        #endregion
    }
}
