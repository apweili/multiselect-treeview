using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interfaces;
using System.Windows.Models;

namespace System.Windows.Extensions
{
    internal static class AutoBindableDataModelExtensions
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

        public static IEnumerable<IAutoBindExpandableModel> Selected(this IAutoBindExpandableModel model)
        {
            var parent = model.Parent;
            while (parent != null)
            {
                parent.IsExpanded = true;
                parent = parent.Parent;
            }
            
            return SelectedAndExpand(model);
        }

        private static IEnumerable<IAutoBindExpandableModel> SelectedAndExpand(IAutoBindExpandableModel model)
        {
            model.IsExpanded = true;
            if (model.Children == null)
            {
                yield return model;
            }
            else
            {
                foreach (var child in model.Children.SelectMany(SelectedAndExpand))
                {
                    yield return child;
                }
            }
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