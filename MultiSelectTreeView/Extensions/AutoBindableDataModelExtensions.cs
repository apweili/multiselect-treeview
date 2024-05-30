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

        public static IList<IAutoBindExpandableModel> Deselect(this IAutoBindExpandableModel model)
        {
            if (model.SelectionCheckState == SelectionCheckState.Deselected)
            {
                return new List<IAutoBindExpandableModel>();
            }

            model.SelectionCheckState = SelectionCheckState.Deselected;
            UpdateParent(model);
            return TraverseToLevelNodeWithAction(model, SetDeselectedState).ToList();
        }

        private static void SetDeselectedState(IAutoBindExpandableModel model)
        {
            model.SelectionCheckState = SelectionCheckState.Deselected;
        }

        public static IList<IAutoBindExpandableModel> Select(this IAutoBindExpandableModel model)
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
            UpdateParent(model);
            return TraverseToLevelNodeWithAction(model, SetSelectedState).ToList();
        }

        private static void UpdateParent(IAutoBindExpandableModel model)
        {
            var parent = model.Parent;
            if (parent == null)
            {
                return;
            }

            var childrenTotalCount = parent.Children.Count();
            var selectedItemsCount =
                parent.Children.Count(c => c.SelectionCheckState == SelectionCheckState.FullSelected);
            var deselectedStateItemsCount =
                parent.Children.Count(c => c.SelectionCheckState == SelectionCheckState.Deselected);
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

            UpdateParent(parent);
        }

        private static void SetSelectedState(IAutoBindExpandableModel model)
        {
            model.IsExpanded = true;
            model.SelectionCheckState = SelectionCheckState.FullSelected;
        }

        private static IEnumerable<IAutoBindExpandableModel> TraverseToLevelNodeWithAction(
            IAutoBindExpandableModel model, Action<IAutoBindExpandableModel> action)
        {
            action?.Invoke(model);
            yield return model;
            if (model.Children == null)
            {
                yield break;
            }
            
            foreach (var child in model.Children.SelectMany(c => TraverseToLevelNodeWithAction(c, action)))
            {
                yield return child;
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

        public static void GetSelectionInfoConsideringAggregateNode(this IAutoBindExpandableModel model,
            out IList<IAutoBindExpandableModel> itemsToSelect, out IList<IAutoBindExpandableModel> itemsToRemove)
        {
            itemsToSelect = new List<IAutoBindExpandableModel>();
            itemsToRemove = new List<IAutoBindExpandableModel>();
            var rootParent = model;
            while (rootParent.Parent != null)
            {
                rootParent = rootParent.Parent;
            }

            TraverseToLevelNodeForRecordSelectionInfo(rootParent, ref itemsToSelect, ref itemsToRemove);
        }

        private static void TraverseToLevelNodeForRecordSelectionInfo(IAutoBindExpandableModel model,
            ref IList<IAutoBindExpandableModel> itemsToSelect, ref IList<IAutoBindExpandableModel> itemsToRemove)
        {
            var parent = model.Parent;
            if ((parent == null || parent.SelectionCheckState != SelectionCheckState.FullSelected) &&
                model.SelectionCheckState == SelectionCheckState.FullSelected)
            {
                itemsToSelect.Add(model);
            }
            else
            {
                itemsToRemove.Add(model);
            }
            
            if (model.Children == null)
            {
                return;
            }

            foreach (var child in model.Children)
            {
                TraverseToLevelNodeForRecordSelectionInfo(child, ref itemsToSelect, ref itemsToRemove);
            }
        } 
    }
}