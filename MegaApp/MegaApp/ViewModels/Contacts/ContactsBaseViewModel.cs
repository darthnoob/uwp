using System.Windows.Input;
using mega;
using MegaApp.Enums;
using MegaApp.Services;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactsBaseViewModel<T> : BaseSdkViewModel
    {
        public ContactsBaseViewModel(MegaSDK megaSdk, bool? isOutgoing = null) : base(megaSdk)
        {
            this._isOutgoing = isOutgoing;
        }

        #region Commands
        
        public ICommand AddContactCommand { get; set; }
        public ICommand RemoveContactCommand { get; set; }

        public ICommand AcceptContactRequestCommand { get; set; }
        public ICommand IgnoreContactRequestCommand { get; set; }
        public ICommand CancelContactRequestCommand { get; set; }
        public ICommand DeclineContactRequestCommand { get; set; }
        public ICommand RemindContactRequestCommand { get; set; }

        public ICommand OpenContactProfileCommand { get; set; }
        public ICommand CloseContactProfileCommand { get; set; }

        #endregion

        #region Properties

        private bool? _isOutgoing { get; set; }

        private CollectionViewModel<T> _itemCollection;
        public CollectionViewModel<T> ItemCollection
        {
            get { return _itemCollection; }
            set { SetField(ref _itemCollection, value); }
        }

        private ContactsContentType _contentType;
        public ContactsContentType ContentType
        {
            get { return _contentType; }
            set { SetField(ref _contentType, value); }
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

        public string SelectOrDeselectAllText => ResourceService.UiResources.GetString("UI_SelectOrDeselectAll");

        #endregion

        #region VisualResources

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
