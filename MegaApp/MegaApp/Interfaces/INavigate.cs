using System;
using Windows.UI.Xaml.Controls;

namespace MegaApp.Interfaces
{
    public interface INavigate
    {
        bool Navigate(Type viewType, INavigationObject navObj = null, Frame baseFrame = null);

        void GoBack(Frame baseFrame = null);

        void GoForward(Frame baseFrame = null);
    }
}
