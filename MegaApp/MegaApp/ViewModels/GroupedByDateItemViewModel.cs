using System;
using mega;
using MegaApp.Extensions;
using MegaApp.Interfaces;

namespace MegaApp.ViewModels
{
    public class GroupedByDateItemViewModel
    {
        public GroupedByDateItemViewModel(MegaSDK megaSdk)
        {
            ItemCollection = new CollectionViewModel<IMegaNode>(megaSdk);
        }

        #region Properties

        public DateTime Date { get; set; }

        public CollectionViewModel<IMegaNode> ItemCollection { get; }

        public string DateAsString => Date.Date == DateTime.Today ? "Today" : Date.DateToString();

        #endregion
    }
}
