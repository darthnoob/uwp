using System;
using System.ComponentModel;
using System.Windows.Input;
using mega;
using MegaApp.Enums;
using MegaApp.Services;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactsBaseViewModel<T> : BaseSdkViewModel
    {
        public ContactsBaseViewModel(bool? isOutgoing = null)
        {
            this.isOutgoing = isOutgoing;
        }

        #region Events

        /// <summary>
        /// Event triggered when the add contact menu option is tapped
        /// </summary>
        public event EventHandler AddContactTapped;

        /// <summary>
        /// Event invocator method called when the add contact menu option is tapped
        /// </summary>
        protected virtual void OnAddContactTapped()
        {
            this.AddContactTapped?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Commands
        
        public virtual ICommand AddContactCommand { get; }
        public virtual ICommand RemoveContactCommand { get; }

        public virtual ICommand AcceptContactRequestCommand { get; }
        public virtual ICommand IgnoreContactRequestCommand { get; }
        public virtual ICommand CancelContactRequestCommand { get; }
        public virtual ICommand DeclineContactRequestCommand { get; }
        public virtual ICommand RemindContactRequestCommand { get; }

        public virtual ICommand OpenContactProfileCommand { get; }
        public virtual ICommand CloseContactProfileCommand { get; }

        public virtual ICommand InvertOrderCommand { get; }

        #endregion

        #region Methods

        private void ListOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ItemCollection.Items))
            {
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems));
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfSelectedItems));
            }

            if (e.PropertyName == nameof(this.ItemCollection.SelectedItems))
            {
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfSelectedItems));
            }
        }

        #endregion

        #region Properties

        private bool? isOutgoing { get; set; }

        private CollectionViewModel<T> _itemCollection;
        public CollectionViewModel<T> ItemCollection
        {
            get { return _itemCollection; }
            set
            {
                if (_itemCollection != null)
                    _itemCollection.PropertyChanged -= ListOnPropertyChanged;

                SetField(ref _itemCollection, value);

                if (_itemCollection != null)
                    _itemCollection.PropertyChanged += ListOnPropertyChanged;
            }
        }

        private ContactsContentType _contentType;
        public ContactsContentType ContentType
        {
            get { return _contentType; }
            set { SetField(ref _contentType, value); }
        }

        public string OrderTypeAndNumberOfItems
        {
            get
            {
                switch(this.CurrentOrder)
                {
                    case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByName"),
                            this.ItemCollection.Items.Count);

                    default:
                        return string.Empty;
                }
            }
        }

        public string OrderTypeAndNumberOfSelectedItems
        {
            get
            {
                switch (this.CurrentOrder)
                {
                    case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByNameMultiSelect"),
                            this.ItemCollection.SelectedItems.Count, this.ItemCollection.Items.Count);

                    default:
                        return string.Empty;
                }
            }
        }

        private MSortOrderType _currentOrder;
        public MSortOrderType CurrentOrder
        {
            get { return _currentOrder; }
            set
            {
                SetField(ref _currentOrder, value);

                OnPropertyChanged(nameof(this.IsCurrentOrderAscending));
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems));
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfSelectedItems));
            }
        }

        public bool IsCurrentOrderAscending
        {
            get
            {
                switch(this.CurrentOrder)
                {
                    case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    default:
                        return true;

                    case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                        return false;
                }
            }
        }

        #endregion

        #region UiResources

        public string AddContactText => ResourceService.UiResources.GetString("UI_AddContact");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string RemoveContactText => ResourceService.UiResources.GetString("UI_RemoveContact");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");
        public string ViewProfileText => ResourceService.UiResources.GetString("UI_ViewProfile");

        public string AcceptContactText => ResourceService.UiResources.GetString("UI_AcceptContact");
        public string CancelInviteText => ResourceService.UiResources.GetString("UI_CancelInvite");
        public string DenyContactText => ResourceService.UiResources.GetString("UI_DenyContact");
        public string RemindContactText => ResourceService.UiResources.GetString("UI_RemindContact");

        public string DeselectAllText => ResourceService.UiResources.GetString("UI_DeselectAll");
        public string SelectAllText => ResourceService.UiResources.GetString("UI_SelectAll");

        #endregion

        #region VisualResources

        public string AddContactPathData => ResourceService.VisualResources.GetString("VR_AddContactPathData");
        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");
        public string RemovePathData => ResourceService.VisualResources.GetString("VR_RemovePathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");
        public string ViewProfilePathData => ResourceService.VisualResources.GetString("VR_ViewProfilePathData");

        public string AcceptPathData => ResourceService.VisualResources.GetString("VR_ConfirmPathData");
        public string DeclinePathData => ResourceService.VisualResources.GetString("VR_CancelPathData");

        #endregion
    }
}
