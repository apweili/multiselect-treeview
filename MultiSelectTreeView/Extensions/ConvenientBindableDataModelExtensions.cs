using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interfaces;

namespace System.Windows.Extensions
{
    internal static class ConvenientBindableDataModelExtensions
    {
        public static void BindExpandableToContainer(this IAutoBindExpandableModel autoBindExpandableModel,
            MultiSelectTreeViewItem container)
        {
            SetItemsSource(autoBindExpandableModel, container);
            BindIsExpanded(autoBindExpandableModel, container);
        }
        
        public static void BindImageSourceToContainer(this IAutoBindImageSourceModel modelWithImageSource,
            MultiSelectTreeViewItem container)
        {
            BindImageSource(modelWithImageSource, container);
        }

        private static void SetItemsSource(IAutoBindExpandableModel autoBindExpandableModel,
            MultiSelectTreeViewItem container)
        {
            container.ItemsSource = autoBindExpandableModel.Children; 
        }
        
        private static void BindIsExpanded(IAutoBindExpandableModel autoBindExpandableModel,
            MultiSelectTreeViewItem container)
        {
            var bindingForExpand = new Binding
            {
                Source = autoBindExpandableModel,
                Path = new PropertyPath(nameof(IAutoBindExpandableModel.IsExpanded)),
                Mode = BindingMode.TwoWay
            };
            container.SetBinding(MultiSelectTreeViewItem.IsExpandedProperty, bindingForExpand);
        }
        
        private static void BindImageSource(IAutoBindImageSourceModel modelWithImageSource,
            MultiSelectTreeViewItem container)
        {
            container.Remarks = modelWithImageSource.ImageSource;
        }
    }
}