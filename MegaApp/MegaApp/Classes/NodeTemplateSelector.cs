using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using MegaApp.ViewModels;
using MegaApp.ViewModels.Offline;

namespace MegaApp.Classes
{
    public abstract class BaseNodeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderItemTemplate { get; set; }
        public DataTemplate FileItemTemplate { get; set; }
    }

    public class NodeTemplateSelector: BaseNodeTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var nodeViewModel = item as NodeViewModel;
            if (nodeViewModel == null) return base.SelectTemplate(item, container);

            if (nodeViewModel.IsFolder)
                return FolderItemTemplate;
            else
                return FileItemTemplate;
        }
    }

    public class OfflineNodeTemplateSelector : BaseNodeTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var offlineNodeViewModel = item as OfflineNodeViewModel;
            if (offlineNodeViewModel == null) return base.SelectTemplate(item, container);

            if (offlineNodeViewModel.IsFolder)
                return FolderItemTemplate;
            else
                return FileItemTemplate;
        }
    }
}
