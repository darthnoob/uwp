using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.ViewModels
{
    public class IncomingSharedFolderNodeViewModel : SharedFolderNodeViewModel
    {
        public IncomingSharedFolderNodeViewModel(MNode megaNode, SharedFoldersListViewModel parent)
            : base(megaNode, parent)
        {
            this.LeaveShareCommand = new RelayCommand(LeaveShare);

            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_IncomingSharedFolderPathData");
            this.Update(megaNode);            
        }

        #region Commands

        public ICommand LeaveShareCommand { get; }

        #endregion

        #region Methods

        public override async void Update(MNode megaNode, bool externalUpdate = false)
        {
            base.Update(megaNode, externalUpdate);

            var owner = SdkService.MegaSdk.getUserFromInShare(this.OriginalMNode);
            var contactAttributeRequestListener = new GetUserAttributeRequestListenerAsync();
            var firstName = await contactAttributeRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getUserAttribute(owner, (int)MUserAttrType.USER_ATTR_FIRSTNAME,
                contactAttributeRequestListener));
            var lastName = await contactAttributeRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.getUserAttribute(owner, (int)MUserAttrType.USER_ATTR_LASTNAME,
                contactAttributeRequestListener));

            OnUiThread(() =>
            {
                this.Owner = (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName)) ?
                    owner.getEmail() : string.Format("{0} {1}", firstName, lastName);
            });
        }        

        private async void LeaveShare()
        {
            if (this.Parent.ItemCollection.IsMultiSelectActive)
            {
                if (this.Parent.LeaveShareCommand.CanExecute(null))
                    this.Parent.LeaveShareCommand.Execute(null);
                return;
            }

            var dialogResult = await DialogService.ShowOkCancelAndWarningAsync(
                ResourceService.AppMessages.GetString("AM_LeaveSharedFolder_Title"),
                string.Format(ResourceService.AppMessages.GetString("AM_LeaveSharedFolderQuestion"), this.Name),
                ResourceService.AppMessages.GetString("AM_LeaveSharedFolderWarning"),
                ResourceService.UiResources.GetString("UI_Leave"), this.CancelText);

            if (!dialogResult) return;

            if (!await this.RemoveAsync())
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_LeaveSharedFolder_Title"),
                        string.Format(ResourceService.AppMessages.GetString("AM_LeaveSharedFolderFailed"), this.Name));
                });
            }
        }

        #endregion

        #region Properties

        public bool AllowRename => (this.AccessLevel == MShareType.ACCESS_FULL) && !this.Parent.ItemCollection.IsMultiSelectActive;

        #endregion

        #region UiResources

        public string LeaveShareText => ResourceService.UiResources.GetString("UI_LeaveShare");

        #endregion
    }
}
