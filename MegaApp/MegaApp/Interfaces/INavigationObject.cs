using System;
using System.Collections.Generic;
using MegaApp.Enums;

namespace MegaApp.Interfaces
{
    public interface INavigationObject
    {
        NavigationActionType Action { get; set; }

        IDictionary<NavigationParamType, object> Parameters { get; set; }

        Type SourceViewModel { get; set; }
    }
}