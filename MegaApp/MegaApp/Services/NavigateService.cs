using System;
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
            var assembly = viewModel.GetType().GetTypeInfo().Assembly;
            var baseTypes = assembly.GetTypes().ToList().Where(t => t.GetTypeInfo().BaseType != null &&
                t.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                t.GetTypeInfo().BaseType.GetGenericTypeDefinition() == typeof(PageEx<>) &&
                t.GetTypeInfo().BaseType.GenericTypeArguments[0] == viewModel.GetType());

            foreach (var baseType in baseTypes)
            {
                var t = assembly.GetTypes().ToList().First(ty => ty.GetTypeInfo().BaseType != null &&
                                                                 ty.GetTypeInfo().BaseType == baseType);
                return t;
            }

            return null;
        }
    }
}
