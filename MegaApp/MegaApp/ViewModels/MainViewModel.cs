using System.Collections.Generic;
using System.Linq;
using MegaApp.UserControls;

namespace MegaApp.ViewModels
{
    public class MainViewModel : BasePageViewModel
    {
        public MainViewModel()
        {
            this.MenuItems = MenuItem.CreateMenuItems();
            this.OptionItems = MenuItem.CreateOptionItems();
        }

        /// <summary>
        /// Initialize the viewmodel
        /// </summary>
        public void Initialize()
        {
            // Set the default menu item to home/first item
            this.SelectedItem = this.MenuItems.FirstOrDefault();
        }

        #region Properties

        private MenuItem _selectedItem;
        /// <summary>
        /// Current selected default menu item
        /// </summary>
        public MenuItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (!SetField(ref _selectedItem, value)) return;
                if (_selectedItem == null) return; // exit else both item lists will be set to null
                this.SelectedOptionItem = null;
                // Navigate to destination with targetviewmodel type
                NavigateTo(_selectedItem.TargetViewModel);
            }
        }

        private MenuItem _selectedOptionItem;
        /// <summary>
        /// Current selected option menu item
        /// </summary>
        public MenuItem SelectedOptionItem
        {
            get { return _selectedOptionItem; }
            set
            {
                if (!SetField(ref _selectedOptionItem, value)) return;
                if (_selectedOptionItem == null) return; // exit else both item lists will be set to null
                this.SelectedItem = null;
                // Navigate to destination with targetviewmodel type
                NavigateTo(_selectedOptionItem.TargetViewModel);
            }
        }

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
        public IList<MenuItem> MenuItems { get; }


        /// <summary>
        /// List of option menu items
        /// </summary>
        public IList<MenuItem> OptionItems { get; }

        #endregion
    }
}
