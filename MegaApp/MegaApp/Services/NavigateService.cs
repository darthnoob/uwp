using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml.Controls;
using MegaApp.UserControls;
using MegaApp.ViewModels;
using INavigate = MegaApp.Interfaces.INavigate;

namespace MegaApp.Services
{
    /// <summary>
    /// Service class used to navigate between views in the app
    /// </summary>
    public class NavigateService: INavigate
    {
        private static NavigateService _instance;
        // Singleton instance
        public static NavigateService Instance => _instance ?? (_instance = new NavigateService());
        // App rootFrame to navigate
        public static Frame MainFrame { get; set; }

        public static IList<Type> PageExTypes { get; set; }

        public static IList<Type> TypeList { get; set; }

        private static Frame GetFrame(Frame baseFrame = null)
        {
            return baseFrame ?? MainFrame;
        }

        public bool Navigate(Type viewType, Frame baseFrame = null)
        {
            return GetFrame(baseFrame).Navigate(viewType);
        }

        public void GoBack(Frame baseFrame = null)
        {
            var navigateFrame = GetFrame(baseFrame);
            if(navigateFrame.CanGoBack) navigateFrame.GoBack();
        }

        /// <summary>
        /// Get the view which implements the BasePageViewModel by reflection
        /// </summary>
        /// <param name="viewModel">The BasePageViewModel that must be implemented</param>
        /// <returns>Type of the view that implements the specified viewmodel</returns>
        public static Type GetViewType(BasePageViewModel viewModel)
        {
            if (TypeList == null)
            {
                var assembly = viewModel.GetType().GetTypeInfo().Assembly;
                TypeList = assembly.GetTypes().ToList();
            }
            
            if (PageExTypes == null)
            {
                PageExTypes = TypeList.Where(t => t.GetTypeInfo().BaseType != null &&
                    t.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                    t.GetTypeInfo().BaseType.GetGenericTypeDefinition() == typeof(PageEx<>) &&
                    t.GetTypeInfo().BaseType.GenericTypeArguments[0] == viewModel.GetType()).ToList();
            }

            foreach (var pageExType in PageExTypes)
            {
                var viewType = TypeList.First(t => t.GetTypeInfo().BaseType != null &&
                    t.GetTypeInfo().BaseType == pageExType);
                return viewType;
            }

            return null;
        }
    }
}
