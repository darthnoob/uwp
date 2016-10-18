using System;
using System.Collections.Generic;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public abstract class BaseNavigableViewModel: BaseUiViewModel
    {
        protected BaseNavigableViewModel()
        {
            this.Navigation = NavigateService.Instance;
        }

        /// <summary>
        /// Navigate to page that holds the specified viewmodel type
        /// </summary>
        /// <param name="viewModelType">Type of viewmodel to navigate to</param>
        /// <param name="action">Optional navigation action parameter</param>
        /// <param name="parameters">Optional navigation data parameters</param>
        public async void NavigateTo(Type viewModelType, 
                                     NavigationActionType action = NavigationActionType.Default,
                                     IDictionary<NavigationParamType, object> parameters = null)
        {
            if(viewModelType == null) throw new ArgumentNullException(nameof(viewModelType));

            var pageType = NavigateService.GetViewType(viewModelType);
            if (pageType == null) throw new ArgumentException("Viewmodel is not bound to a view");

            var navObj = NavigationObject.Create(this.GetType(), action, parameters);
            await this.OnUiThread(() => this.Navigation.Navigate(pageType, navObj));
        }

        /// <summary>
        /// Navigation interface to navigate views
        /// </summary>
        public INavigate Navigation { get; }
       
    }
}
