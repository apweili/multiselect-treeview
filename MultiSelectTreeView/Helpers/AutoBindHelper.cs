using System.Windows.Controls;
using System.Windows.Extensions;
using System.Windows.Interfaces;

namespace System.Windows.Helpers
{
    internal static class AutoBindHelper
    {
        public static bool TryToAutoBindObject(DependencyObject container, object viewModel)
        {
            var itemContainerControl = container as MultiSelectTreeViewItem;
            if (itemContainerControl == null)
            {
                return false;
            }
            
            var autoBindableModel = viewModel as IAutoBindableModel;
            if (autoBindableModel == null)
            {
                return false;
            }
            
            autoBindableModel.BindToContainer(itemContainerControl);
            return true;
        }
    }
}