using System;
using System.Windows.Input;
using Windows.UI;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class ContactRequestViewModel : BaseViewModel, IMegaContactRequest
    {
        // Offset DateTime value to calculate the correct creation and modification time
        private static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public ContactRequestViewModel(MContactRequest contactRequest)
        {
            this.AcceptContactRequestCommand = new RelayCommand(AcceptContactRequest);
            this.DeclineContactRequestCommand = new RelayCommand(DeclineContactRequest);
            this.RemindContactRequestCommand = new RelayCommand(RemindContactRequest);
            this.CancelContactRequestCommand = new RelayCommand(CancelContactRequest);

            MegaContactRequest = contactRequest;
            Handle = contactRequest.getHandle();
            SourceEmail = contactRequest.getSourceEmail();
            SourceMessage = contactRequest.getSourceMessage();
            TargetEmail = contactRequest.getTargetEmail();
            CreationTime = contactRequest.getCreationTime();
            ModificationTime = contactRequest.getModificationTime();
            Status = contactRequest.getStatus();
            IsOutgoing = contactRequest.isOutgoing();

            AvatarColor = UiService.GetColorFromHex(
                SdkService.MegaSdk.getUserHandleAvatarColor(Handle.ToString()));
        }

        #region Commands

        public ICommand AcceptContactRequestCommand { get; }
        public ICommand DeclineContactRequestCommand { get; }
        public ICommand RemindContactRequestCommand { get; }
        public ICommand CancelContactRequestCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Accept the contact request
        /// </summary>
        public async void AcceptContactRequest()
        {
            var acceptContactRequest = new ReplyContactRequestListenerAsync();
            await acceptContactRequest.ExecuteAsync(() =>
                SdkService.MegaSdk.replyContactRequest(this.MegaContactRequest,
                MContactRequestReplyActionType.REPLY_ACTION_ACCEPT, acceptContactRequest));
        }

        /// <summary>
        /// Decline the contact request
        /// </summary>
        public async void DeclineContactRequest()
        {
            var declineContactRequest = new ReplyContactRequestListenerAsync();
            await declineContactRequest.ExecuteAsync(() =>
                SdkService.MegaSdk.replyContactRequest(this.MegaContactRequest,
                MContactRequestReplyActionType.REPLY_ACTION_DENY, declineContactRequest));
        }

        /// <summary>
        /// Remind the contact request
        /// </summary>
        public async void RemindContactRequest()
        {
            var remindContactRequest = new InviteContactRequestListenerAsync();
            await remindContactRequest.ExecuteAsync(() =>
                SdkService.MegaSdk.inviteContact(this.TargetEmail, this.SourceMessage,
                MContactRequestInviteActionType.INVITE_ACTION_REMIND, remindContactRequest));
        }

        /// <summary>
        /// Cancel the contact request
        /// </summary>
        public async void CancelContactRequest()
        {
            var cancelContactRequest = new InviteContactRequestListenerAsync();
            await cancelContactRequest.ExecuteAsync(() =>
                SdkService.MegaSdk.inviteContact(this.TargetEmail, this.SourceMessage,
                MContactRequestInviteActionType.INVITE_ACTION_DELETE, cancelContactRequest));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Original MContactRequest from the Mega SDK that is the base of the contact request
        /// </summary>
        public MContactRequest MegaContactRequest { get; private set; }

        /// <summary>
        /// Unique identifier of the contact request
        /// </summary>
        public ulong Handle { get; private set; }

        /// <summary>
        /// Status of the contact request
        /// </summary>
        public int Status { get; private set; }

        /// <summary>
        /// Returns if the request is an incoming contact request
        /// </summary>
        public bool IsOutgoing { get; private set; }

        /// <summary>
        /// The message that the creator of the contact request has added
        /// </summary>
        public string SourceMessage { get; private set; }
        
        private string _sourceEmail;
        /// <summary>
        /// The email of the request creator
        /// </summary>
        public string SourceEmail
        {
            get { return _sourceEmail; }
            set
            {
                SetField(ref _sourceEmail, value);
                OnPropertyChanged("Email");
                OnPropertyChanged("AvatarLetter");
            }
        }

        private string _targetEmail;
        /// <summary>
        /// The email of the recipient
        /// </summary>
        public string TargetEmail
        {
            get { return _targetEmail; }
            set
            {
                SetField(ref _targetEmail, value);
                OnPropertyChanged("Email");
                OnPropertyChanged("AvatarLetter");
            }
        }

        /// <summary>
        /// Email to display
        /// </summary>
        public string Email => IsOutgoing ? this.TargetEmail : this.SourceEmail;

        /// <summary>
        /// Avatar letter for the contact request
        /// </summary>
        public string AvatarLetter => !string.IsNullOrWhiteSpace(this.Email) ?
            this.Email.Substring(0, 1).ToUpper() : null;

        /// <summary>
        /// Color for the contact request avatar
        /// </summary>
        public Color AvatarColor { get; private set; }

        /// <summary>
        /// The creation time of the contact request
        /// </summary>
        public long CreationTime { get; private set; }

        /// <summary>
        /// The last update time of the contact request
        /// </summary>
        public long ModificationTime { get; private set; }

        /// <summary>
        /// Formatted date of the las update time of the contact request
        /// </summary>
        public string Date => OriginalDateTime.AddSeconds(this.ModificationTime).ToString("dd/MM/yy");

        private bool _isMultiSelected;
        /// <summary>
        /// Indicates if the contact request is currently selected in a multi-select scenario
        /// Needed as path for the ListView to auto select/deselect
        /// </summary>
        public bool IsMultiSelected
        {
            get { return _isMultiSelected; }
            set { SetField(ref _isMultiSelected, value); }
        }

        #endregion

        #region Ui_Resources

        public string AcceptContactText => ResourceService.UiResources.GetString("UI_AcceptContact");
        public string CancelInviteText => ResourceService.UiResources.GetString("UI_CancelInvite");
        public string DenyContactText => ResourceService.UiResources.GetString("UI_DenyContact");
        public string RemindContactText => ResourceService.UiResources.GetString("UI_RemindContact");

        #endregion

        #region VisualResources

        public string AcceptPathData => ResourceService.VisualResources.GetString("VR_ConfirmPathData");
        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string DeclinePathData => ResourceService.VisualResources.GetString("VR_CancelPathData");

        #endregion
    }
}
