using System;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views.Dialogs;

namespace MegaApp.ViewModels.MyAccount
{
    public class ProfileViewModel : MyAccountBaseViewModel
    {
        public ProfileViewModel()
        {
            this.ChangeEmailCommand = new RelayCommand(this.ChangeEmail);
            this.ChangePasswordCommand = new RelayCommand(this.ChangePassword);
        }

        #region Commands

        public ICommand ChangeEmailCommand { get; }
        public ICommand ChangePasswordCommand { get; }

        #endregion

        #region Public Methods

        public async Task<bool> SetFirstName(string newFirstName)
        {
            var setUserAttributeRequestListener = new SetUserAttributeRequestListenerAsync();
            var result = await setUserAttributeRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.setUserAttribute((int)MUserAttrType.USER_ATTR_FIRSTNAME,
                    newFirstName, setUserAttributeRequestListener));

            if (result)
                UserData.Firstname = newFirstName;

            return result;
        }

        public async Task<bool> SetLastName(string newLastName)
        {
            var setUserAttributeRequestListener = new SetUserAttributeRequestListenerAsync();
            var result = await setUserAttributeRequestListener.ExecuteAsync(() =>
                SdkService.MegaSdk.setUserAttribute((int)MUserAttrType.USER_ATTR_LASTNAME,
                    newLastName, setUserAttributeRequestListener));

            if (result)
                UserData.Lastname = newLastName;

            return result;
        }

        #endregion

        #region Private Methods

        private void ChangeEmail()
        {
            
        }

        private async void ChangePassword()
        {
            var changePasswordDialog = new ChangePasswordDialog();
            await changePasswordDialog.ShowAsync();
        }

        #endregion

        #region UiResources

        // Personal information
        public string PersonalInformationTitle => ResourceService.UiResources.GetString("UI_PersonalInformation");
        public string FirstNameText => ResourceService.UiResources.GetString("UI_FirstName");
        public string LastNameText => ResourceService.UiResources.GetString("UI_LastName");
        public string SaveText => ResourceService.UiResources.GetString("UI_Save");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");

        // Email & Password
        public string EmailAndPasswordTitle => ResourceService.UiResources.GetString("UI_EmailAndPassword");
        public string ChangeEmailText => ResourceService.UiResources.GetString("UI_ChangeEmail");
        public string ChangePasswordText => ResourceService.UiResources.GetString("UI_ChangePassword");

        #endregion
    }
}
