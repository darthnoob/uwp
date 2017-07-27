using System;

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
