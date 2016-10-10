using System.Collections;
using MegaApp.ViewModels;
using MegaApp.Views;

namespace MegaApp.Containers
{
    public class LoginAndCreateAccountViewModelContainer : BaseSdkViewModel
    {
        public LoginViewModel LoginViewModel { get; private set; }
        public CreateAccountViewModel CreateAccountViewModel { get; private set; }

        public LoginAndCreateAccountViewModelContainer()
            :base(App.MegaSdk)
        {
            LoginViewModel = new LoginViewModel(App.MegaSdk);
            CreateAccountViewModel = new CreateAccountViewModel(App.MegaSdk);
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
