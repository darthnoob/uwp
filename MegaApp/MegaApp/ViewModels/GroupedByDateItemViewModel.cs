using System;
using System.Collections.ObjectModel;
using MegaApp.Interfaces;

namespace MegaApp.ViewModels
{
    public class GroupedByDateItemViewModel
    {
        public GroupedByDateItemViewModel()
        {
            ItemCollection = new NodeCollectionViewModel();
        }

        #region Properties

        public DateTime Date { get; set; }

        public NodeCollectionViewModel ItemCollection { get; }

        public string DateAsString => Date.Date == DateTime.Today ? "Today" : Date.ToString("dd MMM yyyy");

        #endregion
    }
}
