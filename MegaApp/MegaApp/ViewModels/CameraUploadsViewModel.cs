using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using MegaApp.Enums;
using MegaApp.Interfaces;

namespace MegaApp.ViewModels
{
    public class CameraUploadsViewModel: FolderViewModel
    {
        public CameraUploadsViewModel() : base(ContainerType.CameraUploads)
        { 
            Items = new ObservableCollection<GroupedByDateItemViewModel>();
            ItemCollection.Items.CollectionChanged += ItemsOnCollectionChanged;
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null) return;
                    foreach (var newItem in e.NewItems)
                    {
                        var node = newItem as IMegaNode;
                        if(node == null) continue;
                        if(!node.IsImage) continue;
                        var date = Convert.ToDateTime(node.ModificationTime);
                        var group = Items.FirstOrDefault(i => i.Date.Date == date.Date);

                        if (group != null)
                        {
                            group.ItemCollection.Items.Add(node);
                        }
                        else
                        {
                            group = new GroupedByDateItemViewModel {Date = date};
                            group.ItemCollection.Items.Add(node);
                            Items.Add(group);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Properties

        public ObservableCollection<GroupedByDateItemViewModel> Items { get; set; }

        #endregion
    }
}
