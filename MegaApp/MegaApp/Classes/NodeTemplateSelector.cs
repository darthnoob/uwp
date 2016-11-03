using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using mega;
using MegaApp.ViewModels;

namespace MegaApp.Classes
{
    public class NodeTemplateSelector: DataTemplateSelector
    {
        public DataTemplate FolderItemTemplate { get; set; }
        public DataTemplate FileItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var nodeViewModel = item as NodeViewModel;

            if (nodeViewModel == null) return base.SelectTemplate(item, container);

            switch (nodeViewModel.Type)
            {
                case MNodeType.TYPE_FOLDER:
                {
                    return FolderItemTemplate;
                }
                default:
                {
                    return FileItemTemplate;
                }
            }
        }
        
    }
}
