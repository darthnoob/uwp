using System;
using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels;

namespace MegaApp.UserControls
{
    /// <summary>
    /// Typed UserControl extension that implements a view-model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UserControlEx<T> : UserControl
        where T:BaseViewModel, new()
    {
        public UserControlEx()
        {
            // Create the viewmodel and bind it to the view main datacontext
            this.ViewModel = (T)Activator.CreateInstance(typeof(T));
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// Current view-model binded to the datacontext
        /// </summary>
        public T ViewModel { get; }
    }
}
