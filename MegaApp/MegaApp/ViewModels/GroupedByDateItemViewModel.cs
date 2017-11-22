using System;
using MegaApp.Interfaces;

namespace MegaApp.ViewModels
{
    public class GroupedByDateItemViewModel
    {
        public GroupedByDateItemViewModel()
        {
            ItemCollection = new CollectionViewModel<IMegaNode>();
        }

        #region Properties

        public DateTime Date { get; set; }

        public CollectionViewModel<IMegaNode> ItemCollection { get; }

        public string DateAsString => Date.Date == DateTime.Today ? "Today" : Date.ToString("dd MMM yyyy");

        #endregion
    }
}
