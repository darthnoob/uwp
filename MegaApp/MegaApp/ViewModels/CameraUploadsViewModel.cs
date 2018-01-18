using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.ApplicationModel.Background;
using mega;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class CameraUploadsViewModel: FolderViewModel
    {
        public CameraUploadsViewModel() 
            : base(SdkService.MegaSdk, ContainerType.CameraUploads)
        { 
            Items = new ObservableCollection<GroupedByDateItemViewModel>();
            ItemCollection.Items.CollectionChanged += ItemsOnCollectionChanged;

            ItemCollection.ItemCollectionChanged += OnItemCollectionChanged;
            ItemCollection.SelectedItemsCollectionChanged += OnSelectedItemsCollectionChanged;

            ItemCollection.OrderInverted += OnOrderInverted;
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
                            group = new GroupedByDateItemViewModel(this.MegaSdk) {Date = date};
                            group.ItemCollection.Items.Add(node);
                            Items.Add(group);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null) return;
                    foreach (var oldItem in e.OldItems)
                    {
                        var node = oldItem as IMegaNode;
                        if (node == null) continue;
                        if (!node.IsImage) continue;
                        for(int i = Items.Count-1; i >= 0; i--)
                        {
                            var deleteItem = Items[i].ItemCollection.Items.FirstOrDefault(
                                n => n.Base64Handle == node.Base64Handle);
                            if (deleteItem != null)
                            {
                                Items[i].ItemCollection.Items.Remove(deleteItem);
                                if (!Items[i].ItemCollection.HasItems)
                                    Items.Remove(Items[i]);
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnItemCollectionChanged(object sender, EventArgs args)
        {
            OnUiThread(() =>
            {
                OnPropertyChanged(nameof(this.ItemCollection.Items),
                    nameof(this.OrderTypeAndNumberOfItems),
                    nameof(this.OrderTypeAndNumberOfSelectedItems));
            });
        }

        private void OnSelectedItemsCollectionChanged(object sender, EventArgs args)
        {
            OnUiThread(() =>
            {
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems),
                    nameof(this.OrderTypeAndNumberOfSelectedItems));
            });
        }

        private void OnOrderInverted(object sender, EventArgs args)
        {
            UiService.SetSortOrder(this.FolderRootNode.Base64Handle, 
                this.ItemCollection.IsCurrentOrderAscending ? 
                    MSortOrderType.ORDER_MODIFICATION_ASC : 
                    MSortOrderType.ORDER_MODIFICATION_DESC);

            this.LoadChildNodes();
        }

        private async void SetBackgroundTask(bool value)
        {
            if (value)
            {
                if (!await TaskService.RequestBackgroundAccessAsync())
                {
                    CameraUploadsTaskIsOn = false;
                    return;
                }
                TaskService.UnregisterBackgroundTask(
                    TaskService.CameraUploadTaskEntryPoint,
                    TaskService.CameraUploadTaskName);

                TaskService.RegisterBackgroundTask(
                    TaskService.CameraUploadTaskEntryPoint,
                    TaskService.CameraUploadTaskName,
                    new TimeTrigger(TaskService.CameraUploadTaskTimeTrigger, false),
                    null);}
            else
            {
                TaskService.UnregisterBackgroundTask(
                    TaskService.CameraUploadTaskEntryPoint,
                    TaskService.CameraUploadTaskName);

                // Reset the date
                SettingsService.SaveSettingToFile(SettingsService.ImageDateSetting, DateTime.MinValue);
            }
        }

        #region Properties

        public ObservableCollection<GroupedByDateItemViewModel> Items { get; set; }

        private bool _cameraUploadsTaskIsOn;
        public bool CameraUploadsTaskIsOn
        {
            get
            {
                _cameraUploadsTaskIsOn = TaskService.IsBackGroundTaskActive(
                    TaskService.CameraUploadTaskEntryPoint,
                    TaskService.CameraUploadTaskName);
                return _cameraUploadsTaskIsOn;
            }
            set
            {
                if (!SetField(ref _cameraUploadsTaskIsOn, value)) return;
                var active = TaskService.IsBackGroundTaskActive(
                    TaskService.CameraUploadTaskEntryPoint,
                    TaskService.CameraUploadTaskName);

                if (_cameraUploadsTaskIsOn != active)
                    SetBackgroundTask(_cameraUploadsTaskIsOn);
            }
        }

        public string OrderTypeAndNumberOfItems => this.FolderRootNode != null ? 
            string.Format(ResourceService.UiResources.GetString("UI_ListSortedByDateModified"), 
                this.MegaSdk.getNumChildFiles(this.FolderRootNode.OriginalMNode)) : string.Empty;

        public string OrderTypeAndNumberOfSelectedItems => this.FolderRootNode != null ?
            string.Format(ResourceService.UiResources.GetString("UI_ListSortedByDateModifiedMultiSelect"),
                this.ItemCollection.SelectedItems.Count, this.ItemCollection.Items.Count) : string.Empty;

        #endregion

        #region UiResources

        public string EmptyContentHeaderText => ResourceService.EmptyStates.GetString("ES_CameraUploadsHeader");
        public string EmptyContentSubHeaderText => ResourceService.EmptyStates.GetString("ES_CameraUploadsSubHeader");

        public string OnText => ResourceService.UiResources.GetString("UI_On");
        public string OffText => ResourceService.UiResources.GetString("UI_Off");

        public string SelectOrDeselectAllText => ResourceService.UiResources.GetString("UI_SelectOrDeselectAll");

        #endregion
    }
}
