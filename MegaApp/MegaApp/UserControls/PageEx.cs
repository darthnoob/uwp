using System;
using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels;

namespace MegaApp.UserControls
{
    /// <summary>
    /// Typed Page extension that implements a view-model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageEx<T> : PageEx
        where T:BasePageViewModel, new()
    {
        public PageEx()
        {
            // Create the viewmodel and bind it to the page main datacontext
            this.ViewModel = (T)Activator.CreateInstance(typeof(T));
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// Current view-model binded to the datacontext
        /// </summary>
        public T ViewModel { get; }
    }

    /// <summary>
    /// Page extension
    /// </summary>
    public class PageEx : Page
    {
        public virtual bool CanGoBack => false;

        public virtual void GoBack() {}
    }

}
