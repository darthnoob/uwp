using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace MegaApp.Interfaces
{
    public interface IApplicationBar
    {
        void TranslateAppBarItems(IList<AppBarButton> iconButtons, 
            IList<AppBarButton> menuItems, IList<string> iconStrings, IList<string> menuStrings);
    }
}