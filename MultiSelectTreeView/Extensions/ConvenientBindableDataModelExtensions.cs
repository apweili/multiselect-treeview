using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interfaces;

namespace System.Windows.Extensions
{
    internal static class ConvenientBindableDataModelExtensions
    {
        public static void BindToContainer(this IAutoBindableModel autoBindableModel,
            MultiSelectTreeViewItem container)
        {
            BindHeader(autoBindableModel, container);
            SetItemsSource(autoBindableModel, container);
            BindIsExpanded(autoBindableModel, container);
        }

        private static void BindHeader(IAutoBindableModel autoBindableModel,
            MultiSelectTreeViewItem container)
        {
            var bindingForHeader = new Binding
            {
                Source = autoBindableModel
            };
            container.SetBinding(TreeViewItem.IsExpandedProperty, bindingForHeader); 
        }
        
        private static void SetItemsSource(IAutoBindableModel autoBindableModel,
            MultiSelectTreeViewItem container)
        {
            container.ItemsSource = autoBindableModel.Children; 
        }
        
        private static void BindIsExpanded(IAutoBindableModel autoBindableModel,
            MultiSelectTreeViewItem container)
        {
            var bindingForExpand = new Binding
            {
                Path = new PropertyPath(nameof(IAutoBindableModel.IsExpanded)),
                Mode = BindingMode.TwoWay
            };
            container.SetBinding(TreeViewItem.IsExpandedProperty, bindingForExpand);
        }
    }
}