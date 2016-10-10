using System;
using Windows.UI.Xaml.Controls;

namespace MegaApp.Interfaces
{
    public interface INavigate
    {
        bool Navigate(Type viewType, Frame baseFrame = null);

        void GoBack(Frame baseFrame = null);
    }
}
