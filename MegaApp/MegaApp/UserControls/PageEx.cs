using System;
using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels;

namespace MegaApp.UserControls
{
    /// <summary>
    /// Page extension that implements a view-model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageEx<T> : Page
        where T:BasePageViewModel, new()
    {
        public PageEx()
        {
            this.ViewModel = (T)Activator.CreateInstance(typeof(T));
            this.DataContext = this.ViewModel;
        }

        public T ViewModel { get; }
    }
}
