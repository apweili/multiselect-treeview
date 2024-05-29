using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Enums;
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
            BindIsExpandedProperty(autoBindExpandableModel, container);
            BindSelectionCheckStateProperty(autoBindExpandableModel, container);
        }

        public static void BindImageSourceToContainer(this IAutoBindImageSourceModel modelWithImageSource,
            MultiSelectTreeViewItem container)
        {
            container.Remarks = modelWithImageSource.ImageSource;
        }
        
        public static void AddBindingsToContainer(this IAutoBindingsProvider bindingsProvider,
            MultiSelectTreeViewItem container)
        {
            var bindingInfos = bindingsProvider.GetBindingInfos;
            foreach (var bindingInfo in bindingInfos)
            {
                container.SetBinding(bindingInfo.DependencyProperty, bindingInfo.Binding);
            }
        }
        
        public static IEnumerable<IAutoBindExpandableModel> Deselect(this IAutoBindExpandableModel model)
        {
            if (model.SelectionCheckState == SelectionCheckState.Deselected)
            {
                return new List<IAutoBindExpandableModel>();
            }
            
            model.SelectionCheckState = SelectionCheckState.Deselected;
            UpdateParentToRoot(model);
            return TravarseToLevelNodeWithAction(model, SetDeselectedState);
        }
        
        private static void SetDeselectedState(IAutoBindExpandableModel model)
        {
            model.SelectionCheckState = SelectionCheckState.Deselected;
        }

        public static IEnumerable<IAutoBindExpandableModel> Select(this IAutoBindExpandableModel model)
        {
            if (model.SelectionCheckState == SelectionCheckState.FullSelected)
            {
                return new List<IAutoBindExpandableModel>();
            }
            
            var parent = model.Parent;
            while (parent != null)
            {
                parent.IsExpanded = true;
                parent = parent.Parent;
            }

            model.SelectionCheckState = SelectionCheckState.FullSelected;
            UpdateParentToRoot(model);
            return TravarseToLevelNodeWithAction(model, SetSelectedState);
        }

        private static void UpdateParentToRoot(IAutoBindExpandableModel model)
        {
            var parent = model.Parent;
            if (parent == null)
            {
                return;
            }

            var childrenTotalCount = parent.Children.Count();
            var selectedItemsCount = parent.Children.Count(c => c.SelectionCheckState == SelectionCheckState.FullSelected);
            var deselectedStateItemsCount = parent.Children.Count(c => c.SelectionCheckState == SelectionCheckState.Deselected);
            if (childrenTotalCount == selectedItemsCount)
            {
                parent.SelectionCheckState = SelectionCheckState.FullSelected;
            }
            else if (deselectedStateItemsCount == childrenTotalCount)
            {
                parent.SelectionCheckState = SelectionCheckState.Deselected;
            }
            else
            {
                parent.SelectionCheckState = SelectionCheckState.PartSelected;
            }
   
            UpdateParentToRoot(parent); 
        }

        private static void SetSelectedState(IAutoBindExpandableModel model)
        {
            model.IsExpanded = true;
            model.SelectionCheckState = SelectionCheckState.FullSelected;
        }

        private static IEnumerable<IAutoBindExpandableModel> TravarseToLevelNodeWithAction(IAutoBindExpandableModel model, Action<IAutoBindExpandableModel> action)
        {
            action.Invoke(model);
            if (model.Children == null)
            {
                yield return model;
            }
            else
            {
                foreach (var child in model.Children.SelectMany(c => TravarseToLevelNodeWithAction(c, action)))
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

        private static void BindIsExpandedProperty(IAutoBindExpandableModel autoBindExpandableModel,
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
        
        private static void BindSelectionCheckStateProperty(IAutoBindExpandableModel autoBindExpandableModel,
            MultiSelectTreeViewItem container)
        {
            var bindingForExpand = new Binding
            {
                Source = autoBindExpandableModel,
                Path = new PropertyPath(nameof(IAutoBindExpandableModel.SelectionCheckState)),
                Mode = BindingMode.OneWay
            };
            container.SetBinding(MultiSelectTreeViewItem.SelectionCheckStateProperty, bindingForExpand);
        }
        
        private static IEnumerable<IAutoBindExpandableModel> GetParentFromTopToCurrent(IAutoBindExpandableModel node)
        {
            var parents = new List<IAutoBindExpandableModel>();
            var parent = node.Parent;
            while (parent != null)
            {
                parents.Add(parent);
                parent = parent.Parent;
            }

            parents.Reverse();
            return parents;
        }
    }
}