using System;
using Windows.UI.Xaml.Controls;

namespace MegaApp.Interfaces
{
    public interface INavigate
    {
        bool Navigate(Type viewType, bool useCoreFrame = false, INavigationObject navObj = null);

        void GoBack(bool useCoreFrame = false);

        void GoForward(bool useCoreFrame = false);
    }
}
