using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace MegaApp.Behaviors
{
    public class MultiSelect: Behavior<ListViewBase>
    {
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems",
            typeof(IList),
            typeof(MultiSelect),
            new PropertyMetadata(new List<object>()));

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }


        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var addedItem in e.AddedItems)
            {
                if (!this.SelectedItems.Contains(addedItem))
                {
                    this.SelectedItems.Add(addedItem);
                }
            }
            foreach (var removedItem in e.RemovedItems)
            {
                if (this.SelectedItems.Contains(removedItem))
                {
                    this.SelectedItems.Remove(removedItem);
                }
            }
        }
    }
}
