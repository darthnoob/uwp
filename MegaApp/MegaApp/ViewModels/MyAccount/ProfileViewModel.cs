using System.Threading.Tasks;
using mega;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.MyAccount
{
    public class ProfileViewModel : MyAccountBaseViewModel
    {
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

        #region UiResources

        // Personal information
        public string PersonalInformationTitle => ResourceService.UiResources.GetString("UI_PersonalInformation");
        public string FirstNameText => ResourceService.UiResources.GetString("UI_FirstName");
        public string LastNameText => ResourceService.UiResources.GetString("UI_LastName");
        public string SaveText => ResourceService.UiResources.GetString("UI_Save");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");

        // Email & Password
        public string EmailAndPasswordTitle => ResourceService.UiResources.GetString("UI_EmailAndPassword");

        #endregion
    }
}
