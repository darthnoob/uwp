using System.Collections;

namespace MegaApp.ViewModels
{
    public class LoginAndCreateAccountViewModel : BaseSdkViewModel
    {
        public LoginViewModel LoginViewModel { get; private set; }
        public CreateAccountViewModel CreateAccountViewModel { get; private set; }

        public LoginAndCreateAccountViewModel()
        {
            this.LoginViewModel = new LoginViewModel();
            this.CreateAccountViewModel = new CreateAccountViewModel();
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
