using System;
using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels;

namespace MegaApp.UserControls
{
    /// <summary>
    /// Typed ContentDialog extension that implements a view-model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ContentDialogEx<T> : ContentDialog
        where T : BaseViewModel, new()
    {
        public ContentDialogEx()
        {
            // Create the viewmodel and bind it to the dialog main datacontext
            this.ViewModel = (T)Activator.CreateInstance(typeof(T));
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// Current view-model binded to the datacontext
        /// </summary>
        public T ViewModel { get; }
    }
}
