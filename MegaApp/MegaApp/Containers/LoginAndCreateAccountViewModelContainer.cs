using System;
using System.Collections;
using System.Linq;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;

namespace MegaApp.Containers
{
    class LoginAndCreateAccountViewModelContainer : BaseSdkViewModel
    {
        public LoginViewModel LoginViewModel { get; private set; }
        public CreateAccountViewModel CreateAccountViewModel { get; private set; }

        public LoginAndCreateAccountViewModelContainer(LoginAndCreateAccountPage loginAndCreateAccountPage)
            :base(App.MegaSdk)
        {
            LoginViewModel = new LoginViewModel(App.MegaSdk, loginAndCreateAccountPage);
            CreateAccountViewModel = new CreateAccountViewModel(App.MegaSdk, loginAndCreateAccountPage);
        }

        public void ChangeMenu(IList iconButtons, IList menuItems)
        {
            //this.TranslateAppBarItems(
            //    iconButtons.Cast<ApplicationBarIconButton>().ToList(),
            //    menuItems.Cast<ApplicationBarMenuItem>().ToList(),
            //    new[] { UiResources.Accept, UiResources.Cancel },
            //    null);
        }
    }
}
