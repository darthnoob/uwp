using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using mega;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.Views;
using IApplicationBar = MegaApp.Interfaces.IApplicationBar;

namespace MegaApp.ViewModels
{
    public abstract class BaseSdkViewModel : BasePageViewModel, IApplicationBar
    {
        private static bool isMessageVisible = false;

        protected BaseSdkViewModel()
        {
            this.MegaSdk = SdkService.MegaSdk;
        }

        #region Methods

        /// <summary>
        /// Returns if there is a network available and the user is online (logged in).
        /// <para>If there is not a network available, show the corresponding error message if enabled.</para>
        /// <para>If the user is not logged in, also Navigates to the "LoginPage".</para>
        /// </summary>        
        /// <param name="showMessageDialog">
        /// Boolean parameter to indicate if show error messages.
        /// <para>Default value is false.</para>
        /// </param>
        /// <returns>True if the user is online. False in other case.</returns>
        public bool IsUserOnline(bool showMessageDialog = false)
        {
            if (!NetworkService.IsNetworkAvailable(showMessageDialog)) return false;

            bool isOnline = Convert.ToBoolean(this.MegaSdk.isLoggedIn());
            if (!isOnline)
            {
                if (showMessageDialog)
                {
                    if (!isMessageVisible)
                    {
                        var customMessageDialog = new CustomMessageDialog(
                            ResourceService.AppMessages.GetString("AM_UserNotOnline_Title"),
                            ResourceService.AppMessages.GetString("AM_UserNotOnline"),
                            App.AppInformation,
                            MessageDialogButtons.Ok);

                        customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                        {
                            isMessageVisible = false;
                            OnUiThread(() => (Window.Current.Content as Frame).Navigate(typeof(LoginAndCreateAccountPage)));
                        };

                        customMessageDialog.ShowDialog();
                        isMessageVisible = true;
                    }
                }
                else
                {
                    OnUiThread(() =>
                        (Window.Current.Content as Frame).Navigate(typeof(LoginAndCreateAccountPage)));
                }
            }

            return isOnline;
        }

        #endregion

        #region IApplicationBar

        public void TranslateAppBarItems(IList<AppBarButton> iconButtons,
            IList<AppBarButton> menuItems, IList<string> iconStrings, IList<string> menuStrings)
        {
            //if (iconButtons != null && iconStrings != null)
            //{
            //    for (var i = 0; i < iconButtons.Count; i++)
            //    {
            //        if (iconButtons[i] == null) throw new IndexOutOfRangeException("iconButtons");
            //        if (iconStrings[i] == null) throw new IndexOutOfRangeException("iconStrings");

            //        iconButtons[i].Label = iconStrings[i].ToLower();
            //    }
            //}

            //if (menuItems != null && menuStrings != null)
            //{
            //    for (var i = 0; i < menuItems.Count; i++)
            //    {
            //        if (menuItems[i] == null) throw new IndexOutOfRangeException("menuItems");
            //        if (menuStrings[i] == null) throw new IndexOutOfRangeException("menuStrings");

            //        menuItems[i].Label = menuStrings[i].ToLower();
            //    }
            //}
        }

        #endregion

        #region Properties

        /// <summary>
        /// Instance of a MegaSDK class
        /// </summary>
        public MegaSDK MegaSdk { get; }

        #endregion
    }
}
