using System.Collections.Generic;
using System.Windows.Controls;

namespace System.Windows.Extensions
{
    internal static class MultiSelectTreeItemExtensions
    {
        
        // public static IEnumerable<MultiSelectTreeViewItem> Selected(this MultiSelectTreeViewItem selectedItem, ItemsControl host)
        // {
        //     var parents = GetParentFromTopToCurrent(selectedItem);
        //     foreach (var parent in parents)
        //     {
        //         var container = host.ItemContainerGenerator.ContainerFromItem(parent) as MultiSelectTreeViewItem;
        //         if (container != null)
        //         {
        //             container.IsExpanded = true;
        //             continue;
        //         }
        //
        //         break;
        //     }
        // }
        
        // private static IEnumerable<MultiSelectTreeViewItem> GetParentFromTopToCurrent(MultiSelectTreeViewItem node)
        // {
        //     var parents = new List<MultiSelectTreeViewItem>();
        //     return parents;
        // }
    }
}