using System;
using System.Collections.Generic;
using MegaApp.Enums;
using MegaApp.Interfaces;

namespace MegaApp.Classes
{
    public class NavigationObject: INavigationObject
    {
        /// <summary>
        /// Create a NavigationObject
        /// </summary>
        /// <param name="sourceViewModel">Type of viewmodel that initiaties the navigation</param>
        /// <param name="action">Type of navigation action</param>
        /// <param name="parameters">Navigation parameters</param>
        /// <returns>Navigation object</returns>
        public static NavigationObject Create(Type sourceViewModel,
            NavigationActionType action = NavigationActionType.Default,
            IDictionary<NavigationParamType, object> parameters = null)
        {
            return new NavigationObject
            {
                SourceViewModel = sourceViewModel,
                Action = action,
                Parameters = parameters == null
                    ? new Dictionary<NavigationParamType, object>()
                    : new Dictionary<NavigationParamType, object>(parameters)
            };
        }

        public NavigationActionType Action { get; set; }
        public IDictionary<NavigationParamType, object> Parameters { get; set; }
        public Type SourceViewModel { get; set; }
    }
}
