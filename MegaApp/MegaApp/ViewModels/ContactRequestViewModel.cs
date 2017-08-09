using System;
using System.Windows.Input;
using Windows.UI;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class ContactRequestViewModel : BaseViewModel
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

        #region PrivateMethods

        private async void AcceptContactRequest()
        {
            var acceptContactRequest = new ReplyContactRequestListenerAsync();
            await acceptContactRequest.ExecuteAsync(() =>
                SdkService.MegaSdk.replyContactRequest(this.MegaContactRequest,
                MContactRequestReplyActionType.REPLY_ACTION_ACCEPT, acceptContactRequest));
        }

        private async void DeclineContactRequest()
        {
            var declineContactRequest = new ReplyContactRequestListenerAsync();
            await declineContactRequest.ExecuteAsync(() =>
                SdkService.MegaSdk.replyContactRequest(this.MegaContactRequest,
                MContactRequestReplyActionType.REPLY_ACTION_DENY, declineContactRequest));
        }

        private async void RemindContactRequest()
        {
            var remindContactRequest = new InviteContactRequestListenerAsync();
            await remindContactRequest.ExecuteAsync(() =>
                SdkService.MegaSdk.inviteContact(this.TargetEmail, this.SourceMessage,
                MContactRequestInviteActionType.INVITE_ACTION_REMIND, remindContactRequest));
        }

        private async void CancelContactRequest()
        {
            var cancelContactRequest = new InviteContactRequestListenerAsync();
            await cancelContactRequest.ExecuteAsync(() =>
                SdkService.MegaSdk.inviteContact(this.TargetEmail, this.SourceMessage,
                MContactRequestInviteActionType.INVITE_ACTION_DELETE, cancelContactRequest));
        }

        #endregion

        #region Properties

        public MContactRequest MegaContactRequest { get; set; }
        public ulong Handle { get; set; }
        public string SourceMessage { get; set; }
        public int Status { get; set; }

        private bool _isOutgoing;
        public bool IsOutgoing
        {
            get { return _isOutgoing; }
            set
            {
                SetField(ref _isOutgoing, value);
                OnPropertyChanged("Email");
                OnPropertyChanged("AvatarLetter");
            }
        }

        private string _sourceEmail;
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

        public string Email => IsOutgoing ? this.TargetEmail : this.SourceEmail;

        public string AvatarLetter => !string.IsNullOrWhiteSpace(this.Email) ?
            this.Email.Substring(0, 1).ToUpper() : null;

        /// <summary>
        /// Background color for the avatar
        /// </summary>
        private Color _avatarColor;
        public Color AvatarColor
        {
            get { return _avatarColor; }
            set { SetField(ref _avatarColor, value); }
        }

        private long _creationTime;
        public long CreationTime
        {
            get { return _creationTime; }
            set { SetField(ref _creationTime, value); }
        }

        private long _modificationTime;
        public long ModificationTime
        {
            get { return _modificationTime; }
            set { SetField(ref _modificationTime, value); }
        }

        public string Date => OriginalDateTime.AddSeconds(this.ModificationTime).ToString("dd/MM/yy");

        #endregion

        #region Ui_Resources

        public string AcceptText => ResourceService.UiResources.GetString("UI_Accept");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string DeclineText => ResourceService.UiResources.GetString("UI_Decline");
        public string RemindText => ResourceService.UiResources.GetString("UI_Remind");

        #endregion

        #region VisualResources

        public string AcceptPathData => ResourceService.VisualResources.GetString("VR_ConfirmPathData");
        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string DeclinePathData => ResourceService.VisualResources.GetString("VR_CancelPathData");

        #endregion
    }
}
